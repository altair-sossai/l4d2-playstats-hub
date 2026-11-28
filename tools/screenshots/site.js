const { Runner } = require('./runner');
const live = require('./live');
const { fillChampionships, slugs } = require('./championships');
const { fillBans, bannedPlayers, banReason } = require('./bans');
const { sleep } = require('./cdp');

const phone = 390;
const adminHeight = 1000;
const modalHeight = 900;

async function discover(runner) {
    const page = runner.page;
    const links = async (address, pattern) => {
        await page.goto(runner.url(address), 300);

        return page.evaluate(`Array.from(new Set(Array.from(document.querySelectorAll('a[href]')).map(a => a.getAttribute('href')).filter(href => ${pattern}.test(href))))`);
    };

    await runner.signIn(runner.context.admin);

    const vars = {};

    await page.goto(runner.url('/'), 300);
    runner.context.livePlayers = await page.evaluate(`(() => {
        const titles = new Map(Array.from(document.querySelectorAll('table a[href^="/player/"][title]')).map(a => [a.getAttribute('href'), a.getAttribute('title')]));

        return Array.from(new Map(Array.from(document.querySelectorAll('table a[href^="/player/"]')).filter(a => a.textContent.trim()).map(a => [a.getAttribute('href').split('/').pop(), titles.get(a.getAttribute('href')) || a.textContent.trim()])).entries());
    })()`);
    runner.context.player = runner.context.player || (runner.context.livePlayers[0] || [])[0];

    const matches = await links('/matches', /^\/matches\/\d+$/);

    vars.match = matches[0];
    vars.mapStatistics = (await links(vars.match, /^\/matches\/\d+\/statistics\/\d+$/))[0];
    vars.servers = await links('/admin/servers', /^\/admin\/servers\/\d+$/);
    vars.player = runner.context.player;
    vars.admin = runner.context.admin;
    vars.voter = runner.context.voter || vars.player;
    vars.banned = runner.context.banned || vars.player;

    await page.goto(runner.url('/admin/community?tab=voters'), 300);

    const voters = await page.evaluate(`Array.from(document.querySelectorAll('[data-block-voter]')).map(button => button.getAttribute('data-block-voter'))`);

    vars.blocked = voters.filter(id => ![vars.player, vars.voter, vars.admin].includes(id)).slice(2, 4);
    vars.forced = runner.context.livePlayers.slice(10, 17);
    vars.newAdmin = runner.context.livePlayers[5] || [vars.player, ''];

    vars.bans = bannedPlayers(runner.context.livePlayers);

    return vars;
}

function steam2(id) {
    const account = BigInt(id) - 76561197960265728n;

    return `STEAM_1:${account % 2n}:${account / 2n}`;
}

async function blockVoters(runner, players) {
    await runner.signIn(runner.context.admin);
    await runner.page.goto(runner.url('/admin/community?tab=voters'), 300);
    await runner.page.evaluate(`(async () => {
        const token = $('input[name="__RequestVerificationToken"]').first().val();

        for (const id of ${JSON.stringify(players)})
            await $.post('/admin/community/voters/block', { communityId: id, removeVotes: 'false', returnUrl: '/admin/community?tab=blocked', __RequestVerificationToken: token });

        return true;
    })()`);
}

async function refundPlayer(runner) {
    const player = runner.context.player;

    await runner.signIn(runner.context.admin);
    await runner.page.goto(runner.url(`/admin/skins?player=${player}`), 300);
    await runner.page.evaluate(`Promise.all(Array.from(document.querySelectorAll('[data-player="${player}"][data-gift="0"]')).map(item => $.ajax({
        url: '/api/shop/refund',
        type: 'POST',
        contentType: 'application/json',
        headers: { RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() },
        data: JSON.stringify({ communityId: '${player}', skinId: item.getAttribute('data-skin') })
    }))).then(() => true)`);
}

async function fillAntiCheat(runner) {
    const players = runner.context.livePlayers.slice(0, 8);
    const whitelist = players.slice(0, 2);
    const required = players.slice(2);

    await runner.signIn(runner.context.admin);
    await runner.page.goto(runner.url('/admin/anti-cheat'), 300);
    await runner.page.evaluate(`(async () => {
        const headers = { RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val() };
        const send = (type, url, data) => $.ajax({ url: url, type: type, headers: headers, contentType: 'application/json', data: JSON.stringify(data) });

        for (const [id, name] of ${JSON.stringify(whitelist)})
            await send('POST', '/api/anti-cheat-access/whitelist', { player: id, name: name });

        for (const [id, name] of ${JSON.stringify(required)})
            await send('POST', '/api/anti-cheat-access/required', { player: id, name: name });

        await send('PUT', '/api/anti-cheat-access/mode', { mode: 'listed' });

        return true;
    })()`);
}

async function feedLive(runner) {
    const tokens = runner.context.tokens;

    for (let index = 0; index < tokens.length; index++)
        await live.feed(runner.context.base, tokens[index], index, !runner.fed, runner.context.livePlayers);

    runner.fed = true;
    await sleep(17000);
}

function shots(vars, lang) {
    const player = vars.player;
    const admin = vars.admin;
    const openDropdown = selector => `$(${JSON.stringify(selector)}).first().closest('.dropdown').addClass('open')`;
    const tab = name => `$('#player-tabs a[href="#${name}"]').tab('show'); window.scrollTo(0, 0)`;
    const markPanelWithTable = index => `Array.from(document.querySelectorAll('.panel')).filter(panel => panel.querySelector('table') && !panel.parentElement.closest('.panel'))[${index}].id = 'shot-target'`;
    const hideFloating = `$('#connect-icon').remove()`;
    const fillBackup = `$('#Enabled').prop('checked', true); $('#Destination_ContainerUrl').prop('checked', true); $('#ContainerUrl').val('https://example.blob.core.windows.net/hub-backups?sv=2025-01-05&sp=racwdl&sig=example'); $('#ConnectionString').attr('placeholder', 'DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...'); $('.alert').each(function () { this.innerHTML = this.innerHTML.split(' ').map(word => word.endsWith('secrets.key.') ? '/etc/l4d2-playstats-hub/secrets.key.' : word).join(' '); })`;
    const firstLiveServer = `location.href = document.querySelector('a[href^="/servers/"]').getAttribute('href')`;
    const adminPage = (name, path, extra = {}) => ({ name, path, height: adminHeight, full: true, ...extra, before: [hideFloating, ...(extra.before || [])] });
    const adminModal = (name, path, open, extra = {}) => ({ name, path, height: modalHeight, ...extra, before: [hideFloating, open, { sleep: 800 }, ...(extra.before || [])] });
    const editFirst = attribute => `$('[${attribute}]').not('[${attribute}=""]').first().click()`;
    const forcedImport = vars.forced.slice(0, -1).map(([id, name]) => `"${steam2(id)}" "${name.replace(/"/g, '')}"`).join('\n');
    const [forcedId, forcedName] = vars.forced[vars.forced.length - 1] || [player, 'Player'];
    const [newAdminId, newAdminName] = vars.newAdmin;
    const cup = `/championships/${slugs.cup}`;
    const championships = {};
    const asCaptain = { run: runner => runner.signIn(championships.captain) };
    const asAdmin = { run: runner => runner.signIn(admin) };
    const championshipPage = (name, key, tab, extra = {}) => ({ name, height: adminHeight, full: true, ...extra, before: [{ run: runner => runner.visit(`${championships[key]}?tab=${tab}`) }, hideFloating] });
    const captainPage = (name, tab) => ({ name, before: [asCaptain, { go: `${cup}/captain?tab=${tab}` }, hideFloating], full: true, maxHeight: 1000, after: [asAdmin] });
    const draftShot = url => runner => runner.shot(adminPage('admin-championship-draft', `${url}?tab=draft`, { maxHeight: 1100 }));
    const prepareChampionships = async runner => Object.assign(championships, await fillChampionships(runner, url => draftShot(url)(runner)));

    return [
        { name: 'ranking', path: '/', full: true, maxHeight: 1100 },
        { name: 'ranking-period', path: '/', before: [hideFloating, openDropdown('.period-selector .dropdown-toggle')], clip: '.period-selector, .period-selector .dropdown-menu, table', padding: 12, maxHeight: 400 },
        { name: 'patents', path: '/patents', full: true },
        { name: 'matches', path: '/matches', full: true, maxHeight: 1250 },
        { name: 'match', path: vars.match, full: true, maxHeight: 960 },
        { name: 'match-map-results', path: vars.match, before: [hideFloating, markPanelWithTable(1)], clip: '#shot-target', padding: 0 },
        { name: 'match-team-statistics', path: vars.match, before: [hideFloating, markPanelWithTable(2)], clip: '#shot-target', padding: 0 },
        { name: 'match-map-statistics', path: vars.mapStatistics, full: true, maxHeight: 1300 },
        { name: 'players', path: '/players', full: true, maxHeight: 1100 },
        { name: 'commands', path: '/commands', height: 900 },
        { name: 'servers', path: '/servers', before: [{ run: feedLive }, { go: '/servers' }], full: true, maxHeight: 900 },
        { name: 'server-live', path: '/servers', before: [{ eval: firstLiveServer, navigates: true, settle: 2500 }], full: true, maxHeight: 1500 },
        { before: [{ run: refundPlayer }], always: true },
        { name: 'skins-signed-out', user: null, path: '/skins', height: 900 },
        { name: 'skins', user: player, path: '/skins', height: 900 },
        { name: 'skins-how-it-works', user: player, path: '/skins', before: [hideFloating, `$('#sk-howto').collapse('show')`], clip: '#sk-howto', padding: 8 },
        { name: 'skins-details', user: player, path: '/skins', height: 1400, before: [hideFloating, `$('#sk-shop .sk-card.sk-open').not('.sk-owned').first().click()`, { sleep: 800 }], clip: '#sk-modal .modal-content', from: '#sk-modal .sk-modal-tags', padding: 0 },
        { name: 'skins-buy', user: player, path: '/skins', height: 900, before: [`$('#sk-only-affordable').prop('checked', true).trigger('change')`, { sleep: 500 }, `$('#sk-shop .sk-card:visible').not('.sk-owned').find('.sk-buy').first().click()`, { sleep: 800 }] },
        { name: 'skins-bought', user: player, height: 900, before: [`$('.sk-confirm-buy').click()`, { sleep: 2500 }] },
        { name: 'skins-mine', user: player, path: '/skins#mine', height: 900, before: [`$('a[data-target="#sk-mine"]').tab('show')`, { sleep: 500 }] },
        { name: 'skins-gift', user: admin, path: '/skins', height: 900, before: [`$('#sk-shop .sk-card.sk-open').first().click()`, { sleep: 800 }, `$('#sk-modal .sk-admin-gift:visible').first().click()`, { sleep: 1000 }, `$('#gm-modal .gm-player-id').val('https://steamcommunity.com/profiles/${player}').trigger('input')`] },
        { name: 'player', path: `/player/${player}`, full: true, maxHeight: 1080 },
        { name: 'player-campaigns', path: `/player/${player}`, before: [tab('campaigns')], full: true, maxHeight: 1100 },
        { name: 'player-skins', path: `/player/${player}`, before: [tab('skins')], full: true, maxHeight: 1000 },
        { name: 'player-relations', path: `/player/${player}`, before: [tab('relations')], full: true, maxHeight: 1200 },
        { name: 'player-votes', path: `/player/${player}`, before: [tab('votes-received')], full: true, maxHeight: 1100 },
        { name: 'player-compare', path: `/player/${player}/${admin}`, full: true, maxHeight: 1080 },
        { name: 'community', path: '/community', full: true, maxHeight: 1000 },
        { name: 'community-vote', user: vars.voter, path: '/community/vote', full: true, maxHeight: 640 },
        { name: 'community-votes', path: '/community/votes', before: [hideFloating], full: true, maxHeight: 1200 },
        { before: [{ run: runner => blockVoters(runner, vars.blocked) }], always: true },
        { name: 'community-vote-blocked', user: vars.blocked[0], path: '/community/vote', full: true, maxHeight: 640 },
        { name: 'anti-cheat', path: '/anti-cheat', full: true, maxHeight: 1500 },
        { name: 'ingame-ranking', path: `/ingame?steamid=${player}`, width: 900, height: 450 },
        { name: 'ingame-player', path: `/ingame/player?steamid=${player}`, width: 900, height: 450 },
        { name: 'ingame-matches', path: `/ingame/matches?steamid=${player}`, width: 900, height: 450 },
        { name: 'ingame-commands', path: `/ingame/commands?steamid=${player}`, width: 900, height: 450 },
        { name: 'theme-light-ranking', theme: 'light', path: '/', full: true, maxHeight: 800 },
        { name: 'theme-light-match', theme: 'light', path: vars.match, full: true, maxHeight: 960 },
        { name: 'phone-menu', width: phone, path: '/', before: [`$('.navbar-toggle').click()`, { sleep: 800 }] },
        { name: 'phone-ranking', width: phone, path: '/' },
        { name: 'phone-player', width: phone, path: `/player/${player}`, full: true, maxHeight: 1400 },
        { name: 'phone-server-live', width: phone, path: '/servers', before: [{ run: feedLive }, { go: '/servers' }, { eval: firstLiveServer, navigates: true, settle: 2500 }], full: true, maxHeight: 1400 },
        { name: 'phone-skins', width: phone, user: player, path: '/skins' },
        { name: 'admin-menu', path: '/admin', before: [hideFloating], clip: '.navbar', padding: 0 },
        adminPage('admin-overview', '/admin'),
        adminPage('admin-servers', '/admin/servers'),
        adminModal('admin-servers-add', '/admin/servers', `$('#server-add-modal').modal('show')`, { before: [`$('#server-add-modal #Name').val('L4D2 Competitive #3'); $('#server-add-modal #Address').val('203.0.113.10:27017')`] }),
        adminPage('admin-servers-live', '/admin/servers?tab=live'),
        adminPage('admin-server', vars.servers[0]),
        adminPage('admin-modules', '/admin/modules'),
        adminPage('admin-settings', '/admin/settings'),
        adminPage('admin-settings-appearance', '/admin/settings?tab=appearance'),
        adminPage('admin-settings-ranking', '/admin/settings?tab=ranking'),
        adminPage('admin-game-modes', '/admin/game-modes', { maxHeight: 1400 }),
        adminModal('admin-game-modes-rule', '/admin/game-modes', editFirst('data-rule-edit')),
        adminPage('admin-game-modes-seen', '/admin/game-modes?tab=seen', { maxHeight: 1400 }),
        adminPage('admin-campaigns', '/admin/campaigns', { maxHeight: 1100 }),
        adminModal('admin-campaigns-edit', '/admin/campaigns', editFirst('data-campaign-edit')),
        adminModal('admin-administrators-add', '/admin/administrators', `$('[data-admin-edit=""]').first().click()`, {
            before: [`$('#communityId').val(${JSON.stringify(newAdminId)}); $('#name').val(${JSON.stringify(newAdminName)})`],
            after: [{ eval: `$('#admin-edit-modal form').submit()`, navigates: true }]
        }),
        adminPage('admin-administrators', '/admin/administrators'),
        adminPage('admin-skins-purchases', '/admin/skins', { maxHeight: 1100 }),
        adminPage('admin-skins-catalog', '/admin/skins/catalog'),
        adminPage('admin-skins-settings', '/admin/skins/settings'),
        adminPage('admin-skins-prices', '/admin/skins/prices', { maxHeight: 1100 }),
        adminPage('admin-community', '/admin/community', { maxHeight: 1400 }),
        adminPage('admin-community-voters', '/admin/community?tab=voters', { maxHeight: 1500 }),
        adminModal('admin-community-block', '/admin/community?tab=voters', `$('[data-block-voter]').first().click()`),
        adminPage('admin-community-blocked', '/admin/community?tab=blocked'),
        adminModal('admin-chat-bans-add', '/admin/chat-bans', `$('.cb-add').click()`, {
            before: [`$('#cb-add-player').val(${JSON.stringify(vars.banned)}).trigger('input'); $('#cb-add-reason').val('Spam in the chat during matches')`, { sleep: 600 }],
            after: [{ eval: `$('.cb-add-save').click()`, confirm: true }, { sleep: 1500 }]
        }),
        adminPage('admin-chat-bans', '/admin/chat-bans'),
        adminPage('admin-chat-bans-limits', '/admin/chat-bans?tab=limits'),
        adminPage('admin-remote-commands', '/admin/remote-commands'),
        adminPage('admin-player-connections', `/admin/player-connections?communityId=${player}`),
        adminPage('admin-anti-cheat', '/admin/anti-cheat', { before: [{ run: fillAntiCheat }, { go: '/admin/anti-cheat' }, hideFloating] }),
        adminPage('admin-anti-cheat-page', '/admin/anti-cheat?tab=page'),
        adminPage('admin-backup', '/admin/backup', { before: [fillBackup] }),
        adminPage('admin-backup-storage', '/admin/backup?tab=storage', { before: [fillBackup] }),
        adminPage('admin-patents', '/admin/patents'),
        { name: 'admin-phone-menu', width: phone, path: '/admin/servers', before: [`$('.admin-menu-toggle').click()`, { sleep: 800 }], full: true, maxHeight: 1100 },
        adminModal('admin-forced-names-import', '/admin/forced-names', `$('#fn-import-modal').modal('show')`, {
            before: [`$('#fn-import-modal textarea').val(${JSON.stringify(forcedImport)})`],
            after: [{ eval: `$('#fn-import-modal form').submit()`, navigates: true }]
        }),
        adminModal('admin-forced-names-add', '/admin/forced-names', `$('[data-fn-edit=""]').first().click()`, {
            before: [`$('#fn-player').val(${JSON.stringify(forcedId)}); $('#fn-name').val(${JSON.stringify(forcedName)})`],
            after: [{ eval: `$('#fn-edit-modal form').submit()`, navigates: true }]
        }),
        adminPage('admin-forced-names', '/admin/forced-names'),
        { before: [{ run: runner => fillBans(runner, vars.bans) }], always: true },
        { name: 'bans', path: '/bans', before: [hideFloating], full: true, maxHeight: 900 },
        { name: 'player-ban', path: `/player/${vars.bans[0][0]}`, before: [hideFloating], height: 420 },
        adminPage('admin-bans', '/admin/bans'),
        adminPage('admin-bans-alerts', '/admin/bans?tab=alerts'),
        adminModal('admin-bans-add', '/admin/bans', `$('[data-ban-add=""]').first().click()`, {
            before: [`$('#ban-player').val(${JSON.stringify(vars.bans[7][0])}); $('#ban-add-duration').val('10080').trigger('change'); $('#ban-add-reason').val(${JSON.stringify(banReason(lang))})`]
        }),
        { before: [{ run: prepareChampionships }], always: true },
        { name: 'championships', path: '/championships', before: [hideFloating], full: true, maxHeight: 1300 },
        { name: 'championship', path: cup, before: [hideFloating], full: true, maxHeight: 1500 },
        { name: 'championship-teams', path: `${cup}/teams`, before: [hideFloating], full: true, maxHeight: 1300 },
        { name: 'championship-standings', path: `${cup}/standings`, before: [hideFloating], full: true, maxHeight: 1000 },
        { name: 'championship-bracket', path: `${cup}/bracket`, before: [hideFloating], full: true, maxHeight: 1000 },
        { name: 'championship-series', before: [{ run: runner => runner.visit(`${cup}/series/${championships.series}`) }, hideFloating], full: true, maxHeight: 1100 },
        { name: 'championship-join', user: forcedId, path: `/championships/${slugs.open}`, before: [hideFloating], full: true, maxHeight: 1100 },
        captainPage('championship-captain', 'series'),
        captainPage('championship-captain-team', 'team'),
        adminPage('admin-championships', '/admin/championships'),
        championshipPage('admin-championship-settings', 'cup', 'settings', { maxHeight: 1400 }),
        championshipPage('admin-championship-teams', 'cup', 'teams'),
        championshipPage('admin-championship-stages', 'cup', 'stages'),
        championshipPage('admin-championship-games', 'cup', 'games', { maxHeight: 1200 }),
        championshipPage('admin-championship-requests', 'open', 'participants', { maxHeight: 1100 }),
        { name: 'notifications-bell', path: '/players', height: 720, before: [hideFloating, `$('.nav-notifications').addClass('open')`] },
        { name: 'notifications', path: '/notifications', before: [hideFloating], full: true, maxHeight: 1000 },
        { name: 'phone-championship', width: phone, path: cup, before: [hideFloating], full: true, maxHeight: 1400 },
        { name: 'phone-bans', width: phone, path: '/bans' }
    ];
}

async function captureSite(page, context) {
    const runner = new Runner(page, context);

    await runner.language();

    const vars = await discover(runner);

    await runner.run(shots(vars, context.lang));
}

module.exports = { captureSite };
