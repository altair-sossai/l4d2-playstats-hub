const fs = require('fs');
const path = require('path');
const fixtures = require('./fixtures');
const { sleep } = require('./browser');
const { detailsOnAPhone } = require('./phone');

const desktop = 1280;
const phone = 390;
const quote = value => JSON.stringify(value);
const extra = offset => String(BigInt(fixtures.players[0].id) + 100n + BigInt(offset));
const sharedAddress = '198.51.100.90';
const alertAddress = '198.51.100.91';
const languages = [
    { culture: 'pt-BR', title: 'Banimentos' },
    { culture: 'es-ES', title: 'Baneos' },
    { culture: 'en-US', title: 'Bans' }
];

function expect(condition, message) {
    if (!condition)
        throw new Error(message);
}

async function as(browser, user, width = desktop, address = null) {
    await browser.signIn(user);
    await browser.page.viewport(width, width < 700 ? 844 : 800);

    if (address)
        await browser.open(address);
}

async function ok(browser, label) {
    const state = await browser.eval(`(() => { const entry = performance.getEntriesByType('navigation')[0]; const alert = document.querySelector('.main-container > .alert-danger:not(.player-ban)'); return { status: entry ? entry.responseStatus : 200, error: alert ? alert.innerText.trim() : null }; })()`);

    expect(!state.status || state.status < 400, `${label} answered ${state.status}`);
    expect(!state.error, `${label} showed the error: ${state.error}`);
}

async function noHorizontalScroll(browser, label) {
    const sizes = await browser.eval(`({ scroll: document.documentElement.scrollWidth, client: document.documentElement.clientWidth })`);

    expect(sizes.scroll <= sizes.client + 1, `${label} scrolls sideways at ${sizes.client}px: the page is ${sizes.scroll}px wide`);
}

async function api(context, path, body) {
    const response = await fetch(context.base + path, {
        method: 'POST',
        headers: { Authorization: context.token, 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });

    expect(response.ok, `POST ${path} answered ${response.status}`);

    return response.json();
}

function connect(context, player, address, name) {
    return api(context, '/api/game-info/player-connection-info', { communityId: player, ipAddress: address, name });
}

async function rows(browser, pane) {
    return browser.eval(`Array.from(document.querySelectorAll('#tab-${pane} tbody tr')).map(row => row.getAttribute('data-community-id'))`);
}

async function openAdd(browser, opener = 'button[data-ban-add=""]') {
    await browser.click(opener, { settle: 100 });
    await browser.modalShown('#ban-add-modal');
}

async function saveAdd(browser, label) {
    await browser.submit('#ban-add-modal [type=submit]');
    await ok(browser, label);
}

async function shot(browser, context, name) {
    if (!context.out)
        return;

    fs.mkdirSync(context.out, { recursive: true });

    const metrics = await browser.page.send('Page.getLayoutMetrics');
    const height = Math.min(Math.ceil(metrics.cssContentSize ? metrics.cssContentSize.height : metrics.contentSize.height), 5000);
    const width = await browser.eval('document.documentElement.clientWidth');
    const image = await browser.page.send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: true, clip: { x: 0, y: 0, width, height, scale: 1 } });

    fs.writeFileSync(path.join(context.out, `${name}.png`), Buffer.from(image.data, 'base64'));
}

async function successText(browser) {
    return browser.eval(`Array.from(document.querySelectorAll('.alert-success')).map(alert => alert.innerText.trim()).join(' | ')`);
}

function banChecks(context) {
    return [
        {
            name: 'bans-admin',
            run: async browser => {
                const player = extra(0);

                await as(browser, fixtures.admin, desktop, '/admin/bans');
                await ok(browser, 'The bans page');
                expect(await browser.eval(`!!document.querySelector('.admin-menu a[href="/admin/bans"]')`), 'The administration menu has no Bans item');
                expect(await browser.eval(`document.querySelectorAll('.field-help').length`) > 0, 'The bans page has no help icons');

                await openAdd(browser);
                expect(await browser.eval(`$('#ban-add-modal .ban-ends-group').is(':hidden') && $('#ban-add-duration').val() === '0'`), 'A new ban does not start as permanent, with the end date hidden');
                await browser.fill({ '#ban-add-duration': 'custom' });
                expect(await browser.eval(`$('#ban-add-modal .ban-ends-group').is(':visible') && document.querySelector('#ban-add-ends').required`), 'Choosing "Until a date" does not show and require the end date');
                await browser.fill({ '#ban-player': player, '#ban-add-duration': '1440', '#ban-add-reason': 'E2E wallhack' });
                await shot(browser, context, 'bans-admin-add');
                await saveAdd(browser, 'Banning a player');

                expect((await rows(browser, 'active')).includes(player), `The banned player ${player} is not on the active list`);
                expect((await successText(browser)).includes('The ban was saved'), 'Banning shows no confirmation');
                expect(await browser.eval(`document.querySelector('.ban-row[data-community-id="${player}"] .ban-reason').innerText.includes('E2E wallhack')`), 'The active list does not show the reason');

                await browser.click(`.ban-row[data-community-id="${player}"] [data-ban-edit]`, { settle: 100 });
                await browser.modalShown('#ban-edit-modal');
                expect(await browser.eval(`$('#ban-edit-reason').val() === 'E2E wallhack' && $('#ban-edit-duration').val() === 'custom' && !!$('#ban-edit-ends').val()`), 'The edit window does not bring the reason and the end of the ban');
                await browser.fill({ '#ban-edit-duration': 'custom', '#ban-edit-ends': '2099-03-04T05:06', '#ban-edit-reason': 'E2E aimbot' });
                await browser.submit('#ban-edit-modal [type=submit]');
                await ok(browser, 'Editing a ban');

                const edited = await browser.eval(`document.querySelector('.ban-row[data-community-id="${player}"]').innerText`);

                expect(edited.includes('E2E aimbot') && edited.includes('2099'), `The edited ban shows "${edited}"`);

                await browser.click(`.ban-row[data-community-id="${player}"] [data-ban-lift]`, { settle: 100 });
                await browser.modalShown('#ban-lift-modal');
                await browser.fill({ '#ban-lift-reason': 'E2E mistake' });
                await browser.submit('#ban-lift-modal [type=submit]');
                await ok(browser, 'Lifting a ban');

                expect(!(await rows(browser, 'active')).includes(player), 'The lifted ban is still on the active list');
                await browser.click('.admin-tabs a[data-admin-tab="ended"]', { settle: 300 });
                expect(await browser.eval(`$('#tab-ended').is(':visible') && document.querySelector('#tab-ended .ban-ended-row[data-community-id="${player}"]').innerText.includes('E2E mistake')`), 'The Ended tab does not show the lifted ban with the reason');
                expect(await browser.eval(`location.search.includes('tab=ended')`), 'Opening the Ended tab does not keep it in the address');
                await shot(browser, context, 'bans-admin-ended');

                await browser.page.viewport(phone, 844);
                await browser.open('/admin/bans?tab=ended');
                await noHorizontalScroll(browser, 'The bans page of the administration on a phone');
                await shot(browser, context, 'bans-admin-phone');
            }
        },
        {
            name: 'bans-related-accounts',
            run: async browser => {
                const main = extra(1);
                const alternate = extra(2);

                await connect(context, main, sharedAddress, 'E2E main');
                await connect(context, alternate, sharedAddress, 'E2E alternate');

                await as(browser, fixtures.admin, desktop, '/admin/bans');
                await openAdd(browser);
                await browser.fill({ '#ban-player': main, '#ban-add-duration': '0', '#ban-add-reason': 'E2E alt accounts' });

                const loaded = browser.page.waitFor('Page.loadEventFired');

                await browser.click('#ban-add-modal [type=submit]', { settle: 0 });
                await browser.modalShown('#ra-modal');

                const offered = await browser.eval(`Array.from(document.querySelectorAll('#ra-modal .ra-list input:checked')).map(input => input.getAttribute('data-community-id'))`);

                expect(offered.length === 1 && offered[0] === alternate, `Banning ${main} offered ${JSON.stringify(offered)} instead of ${alternate}`);
                expect(await browser.eval(`document.querySelector('#ra-modal .ra-question').innerText.includes('ban these accounts')`), 'The related accounts window does not ask about banning');
                await browser.click('#ra-modal .ra-all', { settle: 0 });
                await loaded;
                await browser.page.idle();
                await sleep(300);
                await ok(browser, 'Banning with the related accounts');

                const active = await rows(browser, 'active');

                expect(active.includes(main) && active.includes(alternate), `Including the related account banned ${JSON.stringify(active)}`);
                expect((await successText(browser)).includes('Related accounts included: 1'), 'Banning with a related account does not say it was included');
            }
        },
        {
            name: 'bans-alerts',
            run: async browser => {
                const banned = extra(3);
                const first = extra(4);
                const second = extra(5);

                await connect(context, banned, alertAddress, 'E2E cheater');
                await api(context, '/api/bans', { player: banned, name: 'E2E cheater', minutes: 0, reason: 'E2E server ban', command: 'sm_ban' });
                await sleep(50);
                await connect(context, first, alertAddress, 'E2E new account');
                await connect(context, second, alertAddress, 'E2E friend');

                await as(browser, fixtures.admin, desktop, '/admin/bans?tab=alerts');
                await ok(browser, 'The alerts tab');
                expect(await browser.eval(`$('#tab-alerts').is(':visible')`), 'The address with tab=alerts does not open the Alerts tab');
                expect(await browser.eval(`document.querySelector('.ban-row[data-community-id="${banned}"]').innerText.includes('Server')`), 'The ban of the game server does not say it came from the server');

                const alerts = await rows(browser, 'alerts');

                expect(alerts.includes(first) && alerts.includes(second), `The alerts tab lists ${JSON.stringify(alerts)} instead of ${first} and ${second}`);
                expect(await browser.eval(`document.querySelector('.admin-tabs a[data-admin-tab="alerts"] .badge').innerText.trim()`) === '2', 'The Alerts tab does not count the two alerts');
                await shot(browser, context, 'bans-admin-alerts');

                await openAdd(browser, `.ban-alert-row[data-community-id="${first}"] [data-ban-add]`);
                expect(await browser.eval(`$('#ban-player').val() === ${quote(first)} && $('#ban-add-reason').val().startsWith('Related account of') && $('#ban-add-duration').val() === '0' && !!$('#ban-related-to').val()`), 'Banning from an alert does not fill the player, the reason, the duration and the banned account');
                const loaded = browser.page.waitFor('Page.loadEventFired');

                await browser.click('#ban-add-modal [type=submit]', { settle: 0 });
                await browser.modalShown('#ra-modal');

                const offered = await browser.eval(`Array.from(document.querySelectorAll('#ra-modal .ra-list input:not(:disabled)')).map(input => input.getAttribute('data-community-id'))`);
                const linked = await browser.eval(`Array.from(document.querySelectorAll('#ra-modal .ra-list input:disabled')).map(input => input.getAttribute('data-community-id'))`);

                expect(offered.length === 1 && offered[0] === second, `Banning ${first} offered ${JSON.stringify(offered)} instead of ${second}`);
                expect(linked.includes(banned), 'The related accounts window does not mark the banned account as already banned');
                await shot(browser, context, 'bans-admin-alert-related');
                await browser.click('#ra-modal .ra-only', { settle: 0 });
                await loaded;
                await browser.page.idle();
                await sleep(300);
                await ok(browser, 'Banning from an alert');
                expect((await rows(browser, 'active')).includes(first), 'The account banned from the alert is not on the active list');
                expect(!(await rows(browser, 'active')).includes(second), 'Leaving the related account out banned it anyway');

                await browser.open('/admin/bans?tab=alerts');
                expect(!(await rows(browser, 'alerts')).includes(first), 'The alert of the banned account is still on the list');
                await browser.submit(`.ban-alert-row[data-community-id="${second}"] form`);
                await ok(browser, 'Ignoring an alert');
                expect(await browser.eval(`location.search.includes('tab=alerts')`), 'Ignoring an alert did not go back to the Alerts tab');
                expect((await rows(browser, 'alerts')).length === 0, 'The ignored alert is still on the list');

                await browser.open('/notifications');
                expect(await browser.eval(`document.body.innerText.includes('shares an IP address with')`), 'The administrators were not told about the related account');
            }
        },
        {
            name: 'bans-import',
            run: async browser => {
                const imported = extra(6);
                const accountId = BigInt(imported) - 76561197960265728n;
                const steamId = `STEAM_1:${accountId % 2n}:${accountId / 2n}`;

                await as(browser, fixtures.admin, desktop, '/admin/bans');
                await browser.click('[data-target="#ban-import-modal"]', { settle: 100 });
                await browser.modalShown('#ban-import-modal');
                await browser.fill({ '#ban-import-modal textarea': `// Name: E2E imported\n// Ban date: 2026-08-24\n// Expire in: never\n// Ban reason: E2E from banned_user.cfg\nbanid 0 ${steamId}\nbanid 0 broken` });
                await browser.submit('#ban-import-modal [type=submit]');
                await ok(browser, 'Importing banned_user.cfg');

                expect((await successText(browser)).includes('1 added') || (await browser.eval(`document.querySelector('.alert-warning') && document.querySelector('.alert-warning').innerText.includes('1 added')`)), 'The import does not say one ban was added');
                expect(await browser.eval(`document.querySelector('.ban-row[data-community-id="${imported}"]').innerText.includes('Imported')`), 'The imported ban is not on the list as imported');
            }
        },
        {
            name: 'bans-public',
            run: async browser => {
                const player = fixtures.players[15];

                await api(context, '/api/bans', { player: player.id, name: player.name, minutes: 0, reason: 'E2E public reason', command: 'sm_addban' });

                await as(browser, null, desktop, '/bans');
                await ok(browser, 'The public bans page');

                const listed = await browser.eval(`Array.from(document.querySelectorAll('#table-bans .ban-row')).map(row => row.getAttribute('data-community-id'))`);

                expect(listed.includes(player.id) && listed.includes(extra(1)), `The public page lists ${JSON.stringify(listed)}`);
                expect(!listed.includes(extra(0)), 'The public page lists a lifted ban');
                expect(!(await browser.eval(`document.body.innerText.includes('${sharedAddress}')`)), 'The public page shows an IP address');
                expect(await browser.eval(`!!document.querySelector('#nav-more-menu a[href="/bans"]')`), 'The More menu has no Bans item');
                await shot(browser, context, 'bans-public-desktop');

                await browser.open('/bans?q=E2E%20public');
                expect((await browser.eval(`document.querySelectorAll('#table-bans .ban-row').length`)) === 1, 'Searching the public page does not find the one ban');

                await browser.open(`/player/${player.id}`);
                await ok(browser, 'The page of a banned player');
                expect(await browser.eval(`document.querySelector('.player-ban') && document.querySelector('.player-ban').innerText.includes('E2E public reason') && document.querySelector('.player-ban').innerText.includes('permanently')`), 'The player page does not show the ban with the reason');
                await shot(browser, context, 'bans-player-desktop');

                await browser.open(`/player/${fixtures.players[0].id}`);
                expect(!(await browser.eval(`!!document.querySelector('.player-ban')`)), 'A player who is not banned shows a ban');

                await detailsOnAPhone(browser, '/bans', '#table-bans', 3, 'The public bans');
                await browser.page.viewport(phone, 844);

                for (const address of ['/bans', `/player/${player.id}`]) {
                    await browser.open(address);
                    await noHorizontalScroll(browser, `${address} on a phone`);
                    await shot(browser, context, `bans-phone-${address === '/bans' ? 'list' : 'player'}`);
                }

                for (const language of languages) {
                    await browser.open(`/language?culture=${language.culture}&returnUrl=/bans`);
                    expect(await browser.eval(`document.querySelector('h4').innerText.trim()`) === language.title, `The bans page in ${language.culture} is not called "${language.title}"`);
                    await noHorizontalScroll(browser, `The bans page in ${language.culture}`);
                    await shot(browser, context, `bans-public-${language.culture}`);
                }

                try {
                    await browser.theme('light');
                    await browser.page.viewport(desktop, 800);
                    await browser.open('/bans');

                    const light = await browser.eval(`(() => { const color = getComputedStyle(document.body).backgroundColor.match(/\\d+/g).map(Number); return (color[0] * 299 + color[1] * 587 + color[2] * 114) / 1000 > 160; })()`);

                    expect(light, 'The bans page has a dark background in the light theme');
                    await shot(browser, context, 'bans-public-light');
                } finally {
                    await browser.theme('default');
                }

                await api(context, '/api/bans/lift', { player: player.id, command: 'sm_unban' });
            }
        }
    ];
}

module.exports = { banChecks };
