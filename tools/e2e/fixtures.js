const fictionalBase = 76561201960265728n;
const playerOffset = 8000000n;

const id = offset => String(fictionalBase + offset);

const admin = id(0n);
const unknownPlayer = id(9000000n);
const configuration = 'ZoneMod v2.9.2b';
const siteName = 'PlayStats E2E';
const serverName = 'E2E Competitive #1';
const serverAddress = '203.0.113.10:27015';

const names = ['NightOwl', 'RustyHammer', 'PillsHere', 'CapybaraKing', 'xRaven', 'Lazy Panda', 'turbo_otter', 'B1sc0it', 'WitchSlayer', '[ZK] Viper', 'nomad77', 'FrostyMoose', 'Coxinha', 'SilentFalcon', 'tatu_bola', 'RocketLlama'];
const playersWithoutAliases = 3;

const players = names.map((name, index) => ({
    id: id(playerOffset + BigInt(index)),
    name,
    aliases: index < names.length - playersWithoutAliases ? [`${name} old`] : []
}));

const scores = count => Array.from({ length: count }, (_, index) => [250 + index * 37, 210 + index * 41]);

const matches = [
    { maps: ['c8m1_apartment', 'c8m2_subway', 'c8m3_sewers'], lineup: [0, 1, 2, 3, 4, 5, 6, 7], scores: scores(3) },
    { maps: ['c2m1_highway', 'c2m2_fairgrounds', 'c2m3_coaster'], lineup: [8, 9, 10, 11, 12, 13, 14, 15], scores: scores(3) },
    { maps: ['c1m1_hotel', 'c1m2_streets', 'c1m3_mall'], lineup: [0, 4, 8, 12, 1, 5, 9, 13], scores: scores(3) },
    { maps: ['c5m1_waterfront', 'c5m2_park', 'c5m3_cemetery'], lineup: [2, 6, 10, 14, 3, 7, 11, 15], scores: scores(3) },
    { maps: ['c10m1_caves', 'c10m2_drainage'], lineup: [0, 1, 2, 3, 8, 9, 10, 11], scores: scores(2) }
];

const community = {
    MinimumMatchesToVote: 1,
    VotesPerDay: 3,
    MinimumComparisonsToBeListed: 1,
    OtherNamesShown: 4
};

const seedVoters = [1, 2, 3, 4, 5, 6].map(index => players[index].id);
const seedVotesPerVoter = 3;
const voter = players[0].id;

module.exports = { admin, unknownPlayer, configuration, siteName, serverName, serverAddress, players, matches, community, seedVoters, seedVotesPerVoter, voter };
