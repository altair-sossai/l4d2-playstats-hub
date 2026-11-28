const text = {
    en: { aim: 'Aimbot', team: 'Killed the team on purpose', quit: 'Left three matches in a row', offend: 'Offended other players', evade: 'Second account of a banned player' },
    pt: { aim: 'Aimbot', team: 'Matou a equipe de propósito', quit: 'Abandonou três partidas seguidas', offend: 'Ofendeu outros jogadores', evade: 'Segunda conta de um jogador banido' },
    es: { aim: 'Aimbot', team: 'Mató al equipo a propósito', quit: 'Abandonó tres partidas seguidas', offend: 'Ofendió a otros jugadores', evade: 'Segunda cuenta de un jugador baneado' }
};

const minutesInADay = 1440;
const sharedAddress = '198.51.100.40';
const firstPlayer = 20;
const playerCount = 8;

async function call(runner, path, body) {
    const response = await fetch(runner.context.base + path, {
        method: 'POST',
        headers: { Authorization: runner.context.tokens[0], 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });

    if (!response.ok)
        throw new Error(`POST ${path} answered ${response.status}`);
}

function connect(runner, [communityId, name]) {
    return call(runner, '/api/game-info/player-connection-info', { communityId, ipAddress: sharedAddress, name });
}

async function fillBans(runner, players) {
    const words = text[runner.context.lang];
    const administrator = runner.context.livePlayers.find(([id]) => id === runner.context.admin);
    const bans = [
        [players[4], 0, words.evade, 'sm_addban', true],
        [players[3], 30 * minutesInADay, words.offend, 'sm_voteban', false],
        [players[2], 3 * minutesInADay, words.quit, 'l4d2_mix', false],
        [players[1], 7 * minutesInADay, words.team, 'sm_ban', true],
        [players[0], 0, words.aim, 'sm_ban', true]
    ];

    await connect(runner, players[0]);

    for (const [[player, name], minutes, reason, command, byAdministrator] of bans) {
        await call(runner, '/api/bans', {
            player,
            name,
            minutes,
            reason,
            command,
            adminCommunityId: byAdministrator ? runner.context.admin : null,
            adminName: byAdministrator && administrator ? administrator[1] : null
        });
    }

    await connect(runner, players[5]);
    await connect(runner, players[6]);
}

module.exports = {
    fillBans,
    bannedPlayers: players => players.slice(firstPlayer, firstPlayer + playerCount),
    banReason: lang => text[lang].quit
};
