const plugins = { l4d2_playstats_sync: '2.0.0', l4d2_gameinfo_sync: '2.0.0', l4d2_ranking: '2.0.0', l4d2_show_patent_icon: '2.0.0', l4d2_weapon_skins: '4.0.0' };

const fallbackPlayers = ['NightOwl', 'RustyHammer', 'PillsHere', 'CapybaraKing', 'xRaven', 'Lazy Panda', 'turbo_otter', 'B1sc0it', 'WitchSlayer', '[ZK] Viper', 'nomad77', 'FrostyMoose', 'Coxinha', 'SilentFalcon', 'tatu_bola', 'RocketLlama', 'DeadstopPro', 'GrumpyYeti', 'kazumo']
    .map((name, index) => [String(76561201960265728n + 9000000n + BigInt(index)), name]);

const variants = [
    {
        hostname: 'L4D2 Competitive #1',
        map: 'c2m3_coaster',
        configuration: 'ZoneMod v2.9.2b',
        scoreboard: { survivorScore: 1812, infectedScore: 1640, bonus: 320, maxBonus: 800, currentProgress: 0.5, currentProgressPoints: 300, isTankInPlay: true, tankIsDead: false },
        round: { isInReady: false, isInPause: false, inSecondHalfOfRound: true, maxChapterProgressPoints: 600, tankPercent: 0.62, witchPercent: 0.3, antiCheatStatus: 1 },
        survivors: [[0, 100, 0, 0.52], [1, 73, 1, 0.5], [4, 40, 2, 0.49], [2, 100, 3, 0.51]],
        infected: [[3, 2, 250, 61], [5, 3, 250, 18], [6, 5, 325, 0], [7, 8, 6000, 220]],
        spectators: [[12, true]],
        chat: [[4, 2, 'tank na rampa'], [0, 2, 'focus tank']]
    },
    {
        hostname: 'L4D2 Competitive #2',
        map: 'c8m2_subway',
        configuration: 'ZoneMod v2.9.2b',
        scoreboard: { survivorScore: 640, infectedScore: 702, bonus: 400, maxBonus: 800, currentProgress: 0.31, currentProgressPoints: 186, isTankInPlay: false, tankIsDead: false },
        round: { isInReady: false, isInPause: false, inSecondHalfOfRound: false, maxChapterProgressPoints: 600, tankPercent: 0.71, witchPercent: 0.44, antiCheatStatus: 1 },
        survivors: [[9, 100, 4, 0.31], [10, 88, 5, 0.3], [13, 61, 6, 0.29], [14, 100, 7, 0.31]],
        infected: [[11, 1, 50, 12], [16, 4, 600, 35], [17, 6, 250, 0], [18, 2, 250, 8]],
        spectators: [],
        chat: [[13, 2, 'gl hf']]
    }
];

function lineup(players) {
    return index => (players && players[index]) || fallbackPlayers[index % fallbackPlayers.length];
}

function survivor([communityId, name], [, health, character, progress]) {
    return { communityId, name, isAdmin: false, latency: 0.045, character, permanentHealth: health, temporaryHealth: 0, incapacitated: false, isPlayerAlive: true, blackAndWhite: false, primaryWeapon: 2, secondaryWeapon: 1, meleeWeapon: 0, progress };
}

function infected([communityId, name], [, type, health, damage]) {
    return { communityId, name, isAdmin: false, latency: 0.06, type, health, maxHealth: health, isInfectedGhost: false, isPlayerAlive: true, damage };
}

function actor([communityId, name]) {
    return { name, isBot: false, communityId };
}

async function call(base, token, method, path, body) {
    const response = await fetch(base + path, {
        method,
        headers: { Authorization: token, 'Content-Type': 'application/json' },
        body: body === undefined ? undefined : JSON.stringify(body)
    });

    if (!response.ok)
        throw new Error(`${method} ${path} answered ${response.status}`);
}

async function heartbeat(base, token, index = 0) {
    const variant = variants[index % variants.length];

    await call(base, token, 'POST', '/api/servers/heartbeat', { hostname: variant.hostname, map: variant.map, configuration: variant.configuration, sourceModVersion: '1.12.0.7230', statisticsWriting: true, plugins });
}

async function feed(base, token, index = 0, events = true, players = null) {
    const variant = variants[index % variants.length];
    const player = lineup(players);

    await heartbeat(base, token, index);
    await call(base, token, 'PUT', '/api/game-info/configuration', { teamSize: 4, name: variant.configuration });
    await call(base, token, 'PUT', '/api/game-info/round', variant.round);
    await call(base, token, 'PUT', '/api/game-info/scoreboard', variant.scoreboard);
    await call(base, token, 'PUT', '/api/game-info/players', {
        survivors: variant.survivors.map(slot => survivor(player(slot[0]), slot)),
        infecteds: variant.infected.map(slot => infected(player(slot[0]), slot)),
        spectators: variant.spectators.map(([slot, isAdmin]) => {
            const [communityId, name] = player(slot);

            return { communityId, name, isAdmin, latency: 0.03 };
        })
    });
    if (!events)
        return;

    await call(base, token, 'POST', '/api/game-info/events', { type: 'roundLive', secondHalf: variant.round.inSecondHalfOfRound });

    if (variant.scoreboard.isTankInPlay) {
        const tank = variant.infected.find(slot => slot[1] === 8);

        await call(base, token, 'POST', '/api/game-info/events', { type: 'tankSpawned', actor: actor(player(tank[0])) });
    }

    await call(base, token, 'POST', '/api/game-info/events', { type: 'skeet', actor: actor(player(variant.survivors[0][0])), hunter: actor(player(variant.infected[0][0])), skeetType: 'Sniper', isTeamSkeet: false });

    for (const [slot, team, message] of variant.chat) {
        const [communityId, name] = player(slot);

        await call(base, token, 'PUT', '/api/game-info/messages', { public: true, team, communityId, name, isAdmin: false, message });
    }
}

async function dequeueCommands(base, token) {
    await call(base, token, 'GET', '/api/game-info/server-command/dequeue');
}

module.exports = { heartbeat, feed, dequeueCommands };
