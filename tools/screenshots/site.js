const { Runner } = require('./runner');
const live = require('./live');
const { sleep } = require('./cdp');

const phone = 390;

async function discover(runner) {
    const page = runner.page;
    const links = async (address, pattern) => {
        await page.goto(runner.url(address), 300);

        return page.evaluate(`Array.from(new Set(Array.from(document.querySelectorAll('a[href]')).map(a => a.getAttribute('href')).filter(href => ${pattern}.test(href))))`);
    };

    await runner.signIn(runner.context.admin);

    const vars = {};

    await page.goto(runner.url('/'), 300);
    runner.context.livePlayers = await page.evaluate(`Array.from(new Map(Array.from(document.querySelectorAll('table a[href^="/player/"]')).filter(a => a.textContent.trim()).map(a => [a.getAttribute('href').split('/').pop(), a.textContent.trim()])).entries())`);
    runner.context.player = runner.context.player || (runner.context.livePlayers[0] || [])[0];

    const matches = await links('/matches', /^\/matches\/\d+$/);

    vars.match = matches[0];
    vars.mapStatistics = (await links(vars.match, /^\/matches\/\d+\/statistics\/\d+$/))[0];
    vars.servers = await links('/admin/servers', /^\/admin\/servers\/\d+$/);
    vars.player = runner.context.player;
    vars.admin = runner.context.admin;
    vars.voter = runner.context.voter || vars.player;
    vars.banned = runner.context.banned || vars.player;

    return vars;
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

function shots(vars) {
    const player = vars.player;
    const admin = vars.admin;
    const openDropdown = selector => `$(${JSON.stringify(selector)}).first().closest('.dropdown').addClass('open')`;
    const tab = name => `$('#player-tabs a[href="#${name}"]').tab('show'); window.scrollTo(0, 0)`;
    const markPanelWithTable = index => `Array.from(document.querySelectorAll('.panel')).filter(panel => panel.querySelector('table') && !panel.parentElement.closest('.panel'))[${index}].id = 'shot-target'`;
    const hideFloating = `$('#connect-icon').remove()`;
    const fillBackup = `$('#Enabled').prop('checked', true); $('#Destination_ContainerUrl').prop('checked', true); $('#ContainerUrl').val('https://example.blob.core.windows.net/hub-backups?sv=2025-01-05&sp=racwdl&sig=example'); $('#ConnectionString').attr('placeholder', 'DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...'); $('.alert').each(function () { this.innerHTML = this.innerHTML.split(' ').map(word => word.endsWith('secrets.key.') ? '/etc/l4d2-playstats-hub/secrets.key.' : word).join(' '); })`;
    const firstLiveServer = `location.href = document.querySelector('a[href^="/servers/"]').getAttribute('href')`;

    return [
        { name: 'ranking', path: '/', full: true, maxHeight: 1100 },
        { name: 'ranking-period', path: '/', before: [hideFloating, openDropdown('.period-selector .dropdown-toggle')], clip: '.period-selector, .period-selector .dropdown-menu, table', padding: 12, maxHeight: 400 },
        { name: 'history', path: '/history', full: true, maxHeight: 1000 },
        { name: 'patents', path: '/patents', full: true },
        { name: 'matches', path: '/matches', full: true, maxHeight: 1250 },
        { name: 'match', path: vars.match, full: true, maxHeight: 960 },
        { name: 'match-map-results', path: vars.match, before: [hideFloating, markPanelWithTable(1)], clip: '#shot-target', padding: 0 },
        { name: 'match-team-statistics', path: vars.match, before: [hideFloating, markPanelWithTable(2)], clip: '#shot-target', padding: 0 },
        { name: 'match-map-statistics', path: vars.mapStatistics, full: true, maxHeight: 1300 },
        { name: 'players', path: '/players', full: true, maxHeight: 1100 },
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
        { name: 'anti-cheat', path: '/anti-cheat', full: true, maxHeight: 1500 },
        { name: 'ingame-ranking', path: `/ingame?steamid=${player}`, width: 900, height: 450 },
        { name: 'ingame-player', path: `/ingame/player?steamid=${player}`, width: 900, height: 450 },
        { name: 'ingame-matches', path: `/ingame/matches?steamid=${player}`, width: 900, height: 450 },
        { name: 'theme-light-ranking', theme: 'light', path: '/', full: true, maxHeight: 800 },
        { name: 'theme-light-match', theme: 'light', path: vars.match, full: true, maxHeight: 960 },
        { name: 'phone-menu', width: phone, path: '/', before: [`$('.navbar-toggle').click()`, { sleep: 800 }] },
        { name: 'phone-ranking', width: phone, path: '/' },
        { name: 'phone-player', width: phone, path: `/player/${player}`, full: true, maxHeight: 1400 },
        { name: 'phone-server-live', width: phone, path: '/servers', before: [{ run: feedLive }, { go: '/servers' }, { eval: firstLiveServer, navigates: true, settle: 2500 }], full: true, maxHeight: 1400 },
        { name: 'phone-skins', width: phone, user: player, path: '/skins' },
        { name: 'admin-menu', path: '/admin', before: [openDropdown('.navbar .glyphicon-cog')], clip: '.navbar, .navbar .dropdown.open .dropdown-menu', padding: 0 },
        { name: 'admin-overview', path: '/admin', full: true },
        { name: 'admin-servers', path: '/admin/servers', full: true },
        { name: 'admin-server', path: vars.servers[0], full: true },
        { name: 'admin-modules', path: '/admin/modules', full: true },
        { name: 'admin-settings', path: '/admin/settings', full: true },
        { name: 'admin-game-modes', path: '/admin/game-modes', full: true, maxHeight: 1400 },
        { name: 'admin-campaigns', path: '/admin/campaigns', full: true, maxHeight: 1100 },
        { name: 'admin-administrators', path: '/admin/administrators', full: true },
        { name: 'admin-skins-purchases', path: '/admin/skins', full: true, maxHeight: 1100 },
        { name: 'admin-skins-catalog', path: '/admin/skins/catalog', full: true },
        { name: 'admin-skins-settings', path: '/admin/skins/settings', full: true },
        { name: 'admin-skins-prices', path: '/admin/skins/prices', full: true, maxHeight: 1100 },
        { name: 'admin-community', path: '/admin/community', full: true, maxHeight: 1400 },
        {
            name: 'admin-chat-bans-add',
            path: '/admin/chat-bans',
            height: 900,
            before: [`$('.cb-add').click()`, { sleep: 800 }, `$('#cb-add-player').val(${JSON.stringify(vars.banned)}).trigger('input'); $('#cb-add-reason').val('Spam in the chat during matches')`, { sleep: 600 }],
            after: [`$('.cb-add-save').click()`, { sleep: 1500 }]
        },
        { name: 'admin-chat-bans', path: '/admin/chat-bans', full: true },
        { name: 'admin-remote-commands', path: '/admin/remote-commands', full: true },
        { name: 'admin-player-connections', path: `/admin/player-connections?communityId=${player}`, full: true },
        { name: 'admin-anti-cheat', path: '/admin/anti-cheat', before: [{ run: fillAntiCheat }, { go: '/admin/anti-cheat' }, hideFloating], full: true },
        { name: 'admin-backup', path: '/admin/backup', before: [hideFloating, fillBackup], full: true },
        { name: 'admin-phone-menu', width: phone, path: '/admin/servers', before: [`$('.admin-menu-toggle').click()`, { sleep: 800 }], full: true, maxHeight: 1100 }
    ];
}

async function captureSite(page, context) {
    const runner = new Runner(page, context);

    await runner.language();

    const vars = await discover(runner);

    await runner.run(shots(vars));
}

module.exports = { captureSite };
