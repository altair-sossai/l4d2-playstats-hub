const fs = require('fs');
const path = require('path');

const samplePath = path.join(__dirname, '..', '..', 'tests', 'L4D2PlayStats.Hub.Tests', 'Samples', '2023-02-04_03-13_0011_c8m1_apartment.txt');
const steamBase = 76561197960265728n;
const minute = 60000;
const mapGapInMinutes = 20;
const lastFileMinutesAgo = 5;

function steamOf(communityId) {
    const account = BigInt(communityId) - steamBase;

    return `STEAM_1:${account % 2n}:${account / 2n}`;
}

function pad(value, length = 2) {
    return String(value).padStart(length, '0');
}

function sampleRoster(content) {
    const ids = [];

    for (const team of ['A', 'B']) {
        const section = content.split(`[Players:${team}]:`)[1].split('[')[0];

        for (const line of section.trim().split(/\r?\n/))
            ids.push(line.split(';')[2]);
    }

    return ids;
}

function sampleStart(content) {
    const line = content.split('[RoundHalf:0]')[1].trim().split(/\r?\n/)[0].split(';');

    return Number(line[15]);
}

function build(sample, roster, map, when, round, scores, configuration) {
    const original = sampleRoster(sample);
    const mapping = new Map(original.map((steamId, index) => [steamId, roster[index]]));
    const shift = Math.round(when.getTime() / 1000) - 300 - sampleStart(sample);
    const date = `${when.getUTCFullYear()}-${pad(when.getUTCMonth() + 1)}-${pad(when.getUTCDate())}`;
    const time = `${pad(when.getUTCHours())}:${pad(when.getUTCMinutes())}`;
    const [head] = sample.split('[PlayerNames:]:');

    let content = head
        .replace(/\[Gameround:\d+\]\r?\n[^\r\n]+/, `[Gameround:${round}]\n${round};${date};${time};4;${configuration};${map};`)
        .replace(/STEAM_1:\d:\d+/g, steamId => mapping.has(steamId) ? steamOf(mapping.get(steamId).id) : steamId)
        .replace(/\b1675\d{6}\b/g, stamp => String(Number(stamp) + shift))
        .replace(/\[Scoring:\]\r?\n([^\r\n]+)/, (_, line) => {
            const parts = line.split(';');

            parts[2] = String(scores[0]);
            parts[5] = String(scores[1]);

            return `[Scoring:]\n${parts.join(';')}`;
        });

    content += '[PlayerNames:]:\n' + roster.map((player, index) => `${index + 1};${steamOf(player.id)};${player.name}`).join('\n') + '\n';

    return { fileName: `${date}_${time.replace(':', '-')}_${pad(round, 4)}_${map}.txt`, content };
}

function generate(players, matches, now, configuration) {
    const sample = fs.readFileSync(samplePath, 'utf8');
    const files = [];
    const total = matches.reduce((sum, match) => sum + match.maps.length, 0);
    const monthStart = Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), 1);
    const end = Math.max(monthStart, now.getTime() - lastFileMinutesAgo * minute);
    const gap = Math.min(mapGapInMinutes, Math.floor((end - monthStart) / minute / total));
    const appearances = new Map();
    let slot = total - 1;
    let round = 1;

    for (const match of matches) {
        const roster = match.lineup.map(playerIndex => {
            const player = players[playerIndex];
            const aliases = player.aliases || [];
            const seen = appearances.get(player.id) || 0;

            appearances.set(player.id, seen + 1);

            return { id: player.id, name: seen > 0 && aliases.length > 0 ? aliases[(seen - 1) % aliases.length] : player.name };
        });
        let scoreA = 0;
        let scoreB = 0;

        match.maps.forEach((map, index) => {
            scoreA += match.scores[index][0];
            scoreB += match.scores[index][1];
            files.push(build(sample, roster, map, new Date(end - slot * gap * minute), round++, [scoreA, scoreB], configuration));
            slot--;
        });
    }

    return files;
}

module.exports = { generate, steamOf };
