const plugins = { l4d2_playstats_sync: '2.0.0', l4d2_gameinfo_sync: '2.0.0', l4d2_ranking: '2.0.0', l4d2_show_patent_icon: '2.0.0', l4d2_weapon_skins: '4.0.0' };

const variants = [
    {
        hostname: 'L4D2 Competitive #1',
        map: 'c2m3_coaster',
        configuration: 'ZoneMod v2.9.2b',
        scoreboard: { survivorScore: 1812, infectedScore: 1640, bonus: 320, maxBonus: 800, currentProgress: 0.5, currentProgressPoints: 300, isTankInPlay: true, tankIsDead: false },
        round: { isInReady: false, isInPause: false, inSecondHalfOfRound: true, maxChapterProgressPoints: 600, tankPercent: 0.62, witchPercent: 0.3, antiCheatStatus: 1 },
        survivors: [
            ['76561199157667941', 'hard', 100, 0, 0.52],
            ['76561199429077716', 'good', 73, 1, 0.5],
            ['76561198141521946', 'Altair', 40, 2, 0.49],
            ['76561199139309093', 'unReal', 100, 3, 0.51]
        ],
        infected: [
            ['76561198372057945', 'Zenk', 2, 250, 61],
            ['76561198055718285', 'whyyy', 3, 250, 18],
            ['76561198406137741', 'Fernando', 5, 325, 0],
            ['76561198060690576', 'LULALA', 8, 6000, 220]
        ],
        spectators: [['76561198062756277', 'Guigo', true]],
        chat: [['76561198141521946', 'Altair', 2, 'tank na rampa'], ['76561199157667941', 'hard', 2, 'focus tank']]
    },
    {
        hostname: 'L4D2 Competitive #2',
        map: 'c8m2_subway',
        configuration: 'ZoneMod v2.9.2b',
        scoreboard: { survivorScore: 640, infectedScore: 702, bonus: 400, maxBonus: 800, currentProgress: 0.31, currentProgressPoints: 186, isTankInPlay: false, tankIsDead: false },
        round: { isInReady: false, isInPause: false, inSecondHalfOfRound: false, maxChapterProgressPoints: 600, tankPercent: 0.71, witchPercent: 0.44, antiCheatStatus: 1 },
        survivors: [
            ['76561199196110803', 'W is Coming', 100, 4, 0.31],
            ['76561198167146671', 'HOMERSIMPSONS', 88, 5, 0.3],
            ['76561198260753359', 'tom', 61, 6, 0.29],
            ['76561198138299776', 'Kira', 100, 7, 0.31]
        ],
        infected: [
            ['76561198035052692', 'lixoso', 1, 50, 12],
            ['76561199840839030', 'Spw', 4, 600, 35],
            ['76561198087117032', 'Pain', 6, 250, 0],
            ['76561197967940501', 'girlyB*', 2, 250, 8]
        ],
        spectators: [],
        chat: [['76561198260753359', 'tom', 2, 'gl hf']]
    }
];

function survivor([communityId, name, health, character, progress]) {
    return { communityId, name, isAdmin: false, latency: 0.045, character, permanentHealth: health, temporaryHealth: 0, incapacitated: false, isPlayerAlive: true, blackAndWhite: false, primaryWeapon: 2, secondaryWeapon: 1, meleeWeapon: 0, progress };
}

function infected([communityId, name, type, health, damage]) {
    return { communityId, name, isAdmin: false, latency: 0.06, type, health, maxHealth: health, isInfectedGhost: false, isPlayerAlive: true, damage };
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

async function feed(base, token, index = 0, events = true) {
    const variant = variants[index % variants.length];

    await heartbeat(base, token, index);
    await call(base, token, 'PUT', '/api/game-info/configuration', { teamSize: 4, name: variant.configuration });
    await call(base, token, 'PUT', '/api/game-info/round', variant.round);
    await call(base, token, 'PUT', '/api/game-info/scoreboard', variant.scoreboard);
    await call(base, token, 'PUT', '/api/game-info/players', {
        survivors: variant.survivors.map(survivor),
        infecteds: variant.infected.map(infected),
        spectators: variant.spectators.map(([communityId, name, isAdmin]) => ({ communityId, name, isAdmin, latency: 0.03 }))
    });
    if (!events)
        return;

    await call(base, token, 'POST', '/api/game-info/events', { type: 'roundLive', secondHalf: variant.round.inSecondHalfOfRound });

    if (variant.scoreboard.isTankInPlay) {
        const tank = variant.infected.find(player => player[2] === 8);

        await call(base, token, 'POST', '/api/game-info/events', { type: 'tankSpawned', actor: { name: tank[1], isBot: false, communityId: tank[0] } });
    }

    const [hunter] = variant.infected;
    const [skeeter] = variant.survivors;

    await call(base, token, 'POST', '/api/game-info/events', { type: 'skeet', actor: { name: skeeter[1], isBot: false, communityId: skeeter[0] }, hunter: { name: hunter[1], isBot: false, communityId: hunter[0] }, skeetType: 'Sniper', isTeamSkeet: false });

    for (const [communityId, name, team, message] of variant.chat)
        await call(base, token, 'PUT', '/api/game-info/messages', { public: true, team, communityId, name, isAdmin: false, message });
}

async function dequeueCommands(base, token) {
    await call(base, token, 'GET', '/api/game-info/server-command/dequeue');
}

module.exports = { heartbeat, feed, dequeueCommands };
