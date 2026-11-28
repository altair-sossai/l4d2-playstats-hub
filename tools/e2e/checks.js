const fixtures = require('./fixtures');
const { championshipChecks } = require('./championships');
const { banChecks } = require('./bans');
const { sleep } = require('./browser');

const phone = 390;
const desktop = 1280;
const liveTimeout = 20000;
const outcomes = { ArrowLeft: 'is-left', ArrowRight: 'is-right', ArrowDown: 'is-same', ArrowUp: 'is-unknown-one', PhotoRight: 'is-right' };
const votesLeftAfter = { ArrowUp: 3, ArrowLeft: 2, PhotoRight: 1 };
const languages = [
    { culture: 'pt-BR', matches: 'Últimos jogos' },
    { culture: 'es-ES', matches: 'Últimas partidas' },
    { culture: 'en-US', matches: 'Last matches' }
];
const phonePages = ['/', '/championships', '/players', '/matches', '/matches/1', '/matches/1/statistics/1', `/player/${fixtures.players[0].id}`, `/player/${fixtures.unknownPlayer}`, '/community', '/community/votes', '/servers', '/patents', '/history', '/skins', '/anti-cheat', '/bans', '/commands'];
const adminPages = [
    { path: '/admin', help: false },
    { path: '/admin/servers' },
    { path: '/admin/servers/1' },
    { path: '/admin/modules' },
    { path: '/admin/settings' },
    { path: '/admin/game-modes' },
    { path: '/admin/campaigns' },
    { path: '/admin/administrators' },
    { path: '/admin/skins' },
    { path: '/admin/skins/catalog' },
    { path: '/admin/skins/settings' },
    { path: '/admin/skins/prices' },
    { path: '/admin/community' },
    { path: '/admin/chat-bans' },
    { path: '/admin/bans' },
    { path: '/admin/remote-commands' },
    { path: '/admin/player-connections' },
    { path: '/admin/anti-cheat' },
    { path: '/admin/championships' },
    { path: '/admin/backup' }
];

function expect(condition, message) {
    if (!condition)
        throw new Error(message);
}

const quote = value => JSON.stringify(value);
const commandOptions = `$('#commands a.list-group-item').filter((index, item) => $(item).data('value') !== undefined)`;

async function at(browser, user, width, address) {
    await browser.signIn(user);
    await browser.page.viewport(width, width < 700 ? 844 : 800);

    if (address)
        await browser.open(address);
}

async function noHorizontalScroll(browser, label) {
    const sizes = await browser.eval(`({ scroll: document.documentElement.scrollWidth, client: document.documentElement.clientWidth, wide: Array.from(document.querySelectorAll('body *')).filter(element => { const rect = element.getBoundingClientRect(); return rect.width > 0 && rect.right > document.documentElement.clientWidth + 1 && getComputedStyle(element).position !== 'fixed' && !element.closest('.table-responsive, .modal'); }).slice(0, 3).map(element => element.tagName.toLowerCase() + (element.id ? '#' + element.id : '') + (typeof element.className === 'string' && element.className.trim() ? '.' + element.className.trim().split(/\\s+/).join('.') : '')) })`);

    expect(sizes.scroll <= sizes.client + 1, `${label} scrolls sideways at ${sizes.client}px: the page is ${sizes.scroll}px wide (${sizes.wide.join(', ') || 'no element found'})`);
}

async function pageOk(browser, label) {
    const state = await browser.eval(`({ status: performance.getEntriesByType('navigation')[0] ? performance.getEntriesByType('navigation')[0].responseStatus : 200 })`);

    expect(!state.status || state.status < 400, `${label} answered ${state.status}`);
}

async function closeModal(browser, selector) {
    await browser.click(`${selector} [data-dismiss="modal"]`);
    await browser.modalHidden(selector);
}

async function ballot(browser) {
    return browser.eval(`(() => {
        const form = document.querySelector('.ballot');
        if (!form) return null;
        const card = side => { const element = form.querySelector('.ballot-' + side); const aka = element.querySelector('.ballot-aka'); return { id: form.querySelector('input[name=' + (side === 'left' ? 'Left' : 'Right') + ']').value, name: element.querySelector('.ballot-name').innerText.trim(), aka: aka ? aka.innerText : null }; };
        const label = document.querySelector('.ballot-votes-label strong');
        return { left: card('left'), right: card('right'), votesLeft: label ? Number(label.innerText) : null };
    })()`);
}

function checkAliases(card) {
    const player = fixtures.players.find(item => item.id === card.id);

    expect(player, `The ballot offered ${card.id}, a player the tests did not create`);

    if (player.aliases.length > 0) {
        expect(card.aka, `The ballot does not show the other names of ${card.name}`);
        expect([player.name, ...player.aliases].some(name => name !== card.name && card.aka.includes(name)), `The other names of ${card.name} on the ballot are "${card.aka}"`);
    } else {
        expect(!card.aka, `The ballot shows other names for ${card.name}, who only used one: "${card.aka}"`);
    }
}

async function pickFirstPlayer(browser) {
    await browser.modalShown('#pp-modal');
    await browser.until(`document.querySelector('#pp-modal .pp-player')`, 'the players of the search window');

    const picked = await browser.eval(`document.querySelector('#pp-modal .pp-player').getAttribute('data-community-id')`);

    await browser.click('#pp-modal .pp-player', { settle: 0 });

    return picked;
}

async function postLive(context, path, body, method = 'PUT') {
    const response = await fetch(context.base + path, {
        method,
        headers: { Authorization: context.token, 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });

    expect(response.ok, `${method} ${path} answered ${response.status}`);
}

const chatBanIds = `Array.from(document.querySelectorAll('.cb-ban')).map(item => item.getAttribute('data-community-id'))`;

function checks(context) {
    return [
        {
            name: 'home-ranking',
            run: async browser => {
                await at(browser, null, desktop, '/');
                await pageOk(browser, 'The home page');

                const rows = await browser.eval(`document.querySelectorAll('#table-ranking tbody tr a[href^="/player/"]').length`);

                expect(rows >= fixtures.players.length, `The ranking has ${rows} player links, expected at least ${fixtures.players.length}`);
                expect(await browser.eval(`!!document.querySelector('.period-selector')`), 'The period selector is missing');

                await browser.page.viewport(phone, 844);
                await browser.open('/');
                expect(await browser.eval(`document.querySelectorAll('#table-ranking tbody tr').length`) > 0, 'The ranking is empty on the phone');
                await noHorizontalScroll(browser, 'The home page');
                await browser.click('.navbar-toggle', { settle: 800 });
                expect(await browser.eval(`document.querySelector('.navbar-collapse').classList.contains('in')`), 'The phone menu does not open');
            }
        },
        {
            name: 'player-page',
            run: async browser => {
                const player = fixtures.players[0];

                await at(browser, null, desktop, `/player/${player.id}`);
                await pageOk(browser, 'The player page');

                const title = await browser.eval('document.title');

                expect([player.name, ...player.aliases].some(name => title.includes(name)), `The player page title "${title}" does not name ${player.name}`);

                for (const tab of ['campaigns', 'skins', 'relations', 'general']) {
                    await browser.click(`#player-tabs a[href="#${tab}"]`);
                    expect(await browser.eval(`document.querySelector('#${tab}').classList.contains('active') && document.querySelector('#${tab}').getClientRects().length > 0`), `The ${tab} tab of the player page does not show`);
                }

                await browser.open(`/player/${player.id}/${fixtures.players[1].id}`);
                await pageOk(browser, 'The player comparison');
            }
        },
        {
            name: 'unknown-player',
            run: async browser => {
                await at(browser, null, desktop, `/player/${fixtures.unknownPlayer}`);
                await pageOk(browser, 'The page of an unknown player');
                expect(await browser.eval(`!!document.querySelector('.alert-warning')`), 'The page of an unknown player does not say the player is not ranked');
                expect((await browser.eval(`(document.querySelector('.player-steam-profile') || {}).href || ''`)).includes(fixtures.unknownPlayer), 'The page of an unknown player has no Steam profile link');
                expect(await browser.eval(`!!document.querySelector('.player-summary-avatar')`), 'The page of an unknown player has no avatar');
            }
        },
        {
            name: 'matches',
            run: async browser => {
                await at(browser, null, desktop, '/matches');
                await pageOk(browser, 'The matches page');

                const matches = await browser.eval(`Array.from(new Set(Array.from(document.querySelectorAll('a[href^="/matches/"]')).map(a => a.getAttribute('href')).filter(href => /^\\/matches\\/\\d+$/.test(href))))`);

                expect(matches.length === fixtures.matches.length, `The matches page lists ${matches.length} matches, expected ${fixtures.matches.length}`);

                await browser.open(matches[matches.length - 1]);
                await pageOk(browser, 'The match page');
                expect(await browser.eval(`document.querySelectorAll('table').length`) >= 3, 'The match page has fewer than 3 tables');

                const maps = await browser.eval(`Array.from(new Set(Array.from(document.querySelectorAll('a[href*="/statistics/"]')).map(a => a.getAttribute('href'))))`);

                expect(maps.length === fixtures.matches[0].maps.length, `The first match links ${maps.length} maps, expected ${fixtures.matches[0].maps.length}`);

                await browser.open(maps[0]);
                await pageOk(browser, 'The map statistics page');
                expect(await browser.eval(`document.querySelectorAll('table tbody tr').length`) > 0, 'The map statistics page has no rows');
            }
        },
        {
            name: 'players',
            run: async browser => {
                await at(browser, null, desktop, '/players');
                await pageOk(browser, 'The players page');

                const rows = await browser.eval(`document.querySelectorAll('#table-players-summary tbody tr').length`);

                expect(rows === fixtures.players.length, `The players overview lists ${rows} players, expected ${fixtures.players.length}`);
                expect(await browser.eval(`document.querySelectorAll('#table-players-summary thead th').length`) === 10, 'The players overview does not have the history columns');

                await browser.click('#table-players-summary thead a[href="/players?sort=Games"]');
                await browser.until(`location.search === '?sort=Games'`, 'sorting the overview by games');
                expect(await browser.eval(`(() => { const games = Array.from(document.querySelectorAll('#table-players-summary tbody tr')).map(row => Number(row.cells[5].textContent.replace(/\D/g, ''))); return games.every((value, index) => index === 0 || games[index - 1] >= value); })()`), 'The overview is not ordered by games');

                await browser.open('/players?orderBy=MvpSiDamage&asc=False');
                await pageOk(browser, 'The players page with one statistic');
                expect(await browser.eval(`!document.querySelector('#table-players-summary') && document.querySelectorAll('table tbody tr').length > 0`), 'Choosing a statistic does not show its table');

                await browser.open('/history');
                expect(await browser.eval(`location.pathname === '/players' && !!document.querySelector('#table-players-summary')`), 'The old history address does not open the players overview');
            }
        },
        {
            name: 'phone-layout',
            run: async browser => {
                await at(browser, fixtures.players[0].id, phone);

                const problems = [];

                for (const address of phonePages) {
                    await browser.open(address);

                    try {
                        await pageOk(browser, address);
                        await noHorizontalScroll(browser, address);
                    } catch (error) {
                        problems.push(error.message);
                    }
                }

                expect(problems.length === 0, problems.join('\n'));
            }
        },
        {
            name: 'commands',
            run: async browser => {
                await at(browser, null, desktop, '/commands');
                await pageOk(browser, 'The commands page');

                const visible = `Array.from(document.querySelectorAll('#cmd-page .cmd-card')).filter(card => card.getClientRects().length > 0).length`;
                const total = await browser.eval(`document.querySelectorAll('#cmd-page .cmd-card').length`);

                expect(total > 50, `The commands page has only ${total} commands`);

                await browser.fill({ '#cmd-search': 'PAUSÉ' });
                await sleep(200);

                const found = await browser.eval(visible);

                expect(found > 0 && found < 15, `Searching "pausé" shows ${found} commands`);
                expect(await browser.eval(`document.querySelector('#cmd-count').innerText.trim().length > 0`), 'The search does not say how many commands it found');

                await browser.fill({ '#cmd-search': 'stopmix' });
                await sleep(600);
                expect(await browser.eval(`document.querySelector('#commands-admin .panel-collapse').classList.contains('in') && document.querySelector('#command-admin-stopmix').getClientRects().length > 0`), 'Searching an administrator command does not open that section');

                await browser.fill({ '#cmd-search': 'zzzz-nothing' });
                await sleep(200);
                expect(await browser.eval(`document.querySelector('#cmd-empty').getClientRects().length > 0`), 'A search with no result does not say so');

                await browser.fill({ '#cmd-search': '' });
                await sleep(200);
                expect(await browser.eval(visible) === total, 'Clearing the search does not show every command again');

                await browser.click('#cmd-chips a[data-section="commands-mix"]');
                expect(await browser.eval(`Math.abs(document.querySelector('#commands-mix').getBoundingClientRect().top) < 60`), 'The Mix chip does not jump to its section');

                await browser.page.viewport(phone, 844);
                await browser.open('/commands');
                await noHorizontalScroll(browser, 'The commands page');
            }
        },
        {
            name: 'community-ranking',
            run: async browser => {
                await at(browser, fixtures.voter, desktop, '/community');
                await pageOk(browser, 'The community ranking');
                expect(await browser.eval(`!!document.querySelector('.vote-invite') && document.querySelector('.vote-invite').getClientRects().length > 0`), 'The invitation to vote is not shown');
                expect(await browser.eval(`document.querySelector('.vote-invite-button').getAttribute('href')`) === '/community/vote', 'The invitation does not link to the voting page');

                await browser.click('.vote-invite-more');
                await browser.modalShown('#how-it-works-modal');
                expect(await browser.eval(`document.querySelector('#how-it-works-modal .modal-body').innerText.trim().length`) > 100, 'The "how it works" window is empty');
                await closeModal(browser, '#how-it-works-modal');

                const row = await browser.eval(`(() => { const row = document.querySelector('#table-community-ranking tbody tr[data-player]'); return row ? { id: row.dataset.player, name: row.dataset.name, position: row.dataset.position } : null; })()`);

                expect(row, 'The community ranking has no rows');
                expect(await browser.eval(`document.querySelectorAll('#table-community-ranking tbody tr[data-player]').length`) >= fixtures.players.length / 2, 'The community ranking lists too few players');
                await browser.click('#table-community-ranking tbody tr[data-player] td:last-child');
                await browser.modalShown('#community-player-modal');

                const details = await browser.eval(`({ name: document.querySelector('#community-player-name').innerText, position: document.querySelector('#community-player-modal .crm-position').innerText, votes: document.querySelector('#community-player-modal .crm-votes').getAttribute('href'), rating: document.querySelector('#community-player-modal .crm-rating').innerText })`);

                expect(details.name === row.name, `The details window shows "${details.name}" for ${row.name}`);
                expect(details.position.includes(row.position), `The details window shows the position "${details.position}"`);
                expect(details.votes.endsWith(`?player=${row.id}`), `The details window links the votes as ${details.votes}`);
                expect(details.rating.trim() !== '', 'The details window shows no rating');
                await closeModal(browser, '#community-player-modal');

                await browser.click('a[href="#community-columns-modal"]');
                await browser.modalShown('#community-columns-modal');
                expect(await browser.eval(`document.querySelector('#community-columns-modal .modal-body').innerText.trim().length`) > 50, 'The columns window is empty');
                await closeModal(browser, '#community-columns-modal');
            }
        },
        {
            name: 'community-vote',
            run: async browser => {
                await at(browser, fixtures.voter, desktop, '/community/vote');
                await pageOk(browser, 'The voting page');

                let current = await ballot(browser);

                expect(current, 'The voting page shows no ballot');
                expect(current.votesLeft === fixtures.community.VotesPerDay, `The ballot shows ${current.votesLeft} votes left, expected ${fixtures.community.VotesPerDay}`);

                expect(await browser.eval('document.querySelectorAll(".ballot button[name=Outcome]").length') === 4, 'The ballot does not have exactly four answers');

                for (const key of ['ArrowUp', 'ArrowLeft', 'PhotoRight', 'ArrowDown']) {
                    checkAliases(current.left);
                    checkAliases(current.right);

                    await browser.eval(`document.getElementById('community-ballot').dataset.old = '1'`);
                    if (key === 'PhotoRight')
                        await browser.click('.ballot-right .ballot-photo', { settle: 0 });
                    else
                        await browser.press(key);
                    expect(await browser.eval(`document.querySelector('.ballot').classList.contains(${quote(outcomes[key])})`), `The ${key} key did not mark the ballot as ${outcomes[key]}`);
                    await browser.until(`document.getElementById('community-ballot') && !document.getElementById('community-ballot').dataset.old`, `the ballot after the ${key} key`);
                    await sleep(300);

                    current = await ballot(browser);

                    if (key in votesLeftAfter) {
                        expect(current, `No ballot after the ${key} key`);
                        expect(current.votesLeft === votesLeftAfter[key], `After the ${key} key the ballot shows ${current.votesLeft} votes left, expected ${votesLeftAfter[key]}`);
                    }
                }

                expect(!current, 'The ballot is still shown after the last vote of the day');

                const limit = await browser.eval(`(() => { const button = document.querySelector('#community-ballot .jumbotron a.btn-success[href="/community"]'); return { text: document.querySelector('#community-ballot').innerText, button: !!button && button.getClientRects().length > 0 }; })()`);

                expect(limit.button, 'The limit reached state has no button to the community ranking');
                expect(limit.text.includes(String(fixtures.community.VotesPerDay)), 'The limit reached state does not show the votes per day');
                await browser.click('#community-ballot .jumbotron a.btn-success', { navigates: true });
                expect(await browser.eval('location.pathname') === '/community', 'The limit reached button does not open the community ranking');

                await browser.open(`/community/votes?voter=${fixtures.voter}`);
                expect(await browser.eval(`document.querySelectorAll('.vf-row').length`) >= fixtures.community.VotesPerDay - 1, 'The latest votes do not list the votes just made (every answer but "I do not know")');
            }
        },
        {
            name: 'community-votes',
            run: async browser => {
                await at(browser, null, desktop, '/community/votes');
                await pageOk(browser, 'The latest votes');
                expect(await browser.eval(`document.querySelectorAll('.vf-row').length`) > 0, 'The latest votes page is empty');

                await browser.click('[data-player-picker="#vf-voter"]', { settle: 0 });

                const loaded = browser.page.waitFor('Page.loadEventFired');
                const voter = await pickFirstPlayer(browser);

                await loaded;
                await browser.page.idle();
                expect(await browser.eval('new URLSearchParams(location.search).get("voter")') === voter, `Picking a voter in the search window did not filter by ${voter}`);

                const voters = await browser.eval(`Array.from(document.querySelectorAll('.vf-row .vf-voter')).map(a => new URLSearchParams(a.getAttribute('href').split('?')[1] || '').get('voter'))`);

                expect(voters.length > 0 && voters.every(id => id === voter), `The voter filter shows votes of other players: ${voters.join(', ')}`);
                expect(await browser.eval(`document.querySelectorAll('.vf-chip').length`) === 1, 'The voter filter has no chip');

                await browser.open('/community/votes');

                const option = await browser.eval(`(() => { const option = document.querySelector('#vf-players option'); return { name: option.value, id: option.getAttribute('data-id') }; })()`);
                const navigated = browser.page.waitFor('Page.loadEventFired');

                await browser.fill({ '#vf-player-name': option.name });
                await navigated;
                await browser.page.idle();
                expect(await browser.eval('new URLSearchParams(location.search).get("player")') === option.id, `Typing ${option.name} did not filter the votes about that player`);

                const pairs = await browser.eval(`Array.from(document.querySelectorAll('.vf-row')).map(row => Array.from(row.querySelectorAll('.vf-player')).map(a => new URLSearchParams(a.getAttribute('href').split('?')[1] || '').get('player')))`);

                expect(pairs.length > 0 && pairs.every(pair => pair.includes(option.id)), `The player filter shows comparisons without ${option.name}`);

                await browser.click('.vf-filters a.btn-default', { navigates: true });
                expect(await browser.eval('location.search') === '', 'The clear button does not remove the filters');
            }
        },
        {
            name: 'admin-pages',
            run: async browser => {
                await at(browser, fixtures.admin, desktop);

                const problems = [];

                for (const page of adminPages) {
                    await browser.open(page.path);

                    const state = await browser.eval(`({ path: location.pathname, heading: !!document.querySelector('h3, h4, .admin-lead'), help: document.querySelectorAll('.field-help').length, empty: Array.from(document.querySelectorAll('.field-help')).filter(help => !(help.getAttribute('title') || help.getAttribute('data-original-title') || '').trim()).length })`);

                    if (state.path !== page.path)
                        problems.push(`${page.path} went to ${state.path}`);
                    else if (!state.heading)
                        problems.push(`${page.path} has no heading`);
                    else if (page.help !== false && state.help === 0)
                        problems.push(`${page.path} has no help icons`);
                    else if (state.empty > 0)
                        problems.push(`${page.path} has ${state.empty} help icons without text`);
                }

                expect(problems.length === 0, problems.join('\n'));

                await browser.open('/admin/settings');
                await browser.click('.field-help-more');
                await browser.modalShown('#field-help-modal');
                expect(await browser.eval(`document.querySelector('#field-help-modal .field-help-summary').innerText.trim().length`) > 0, 'The help window has no summary');
                await closeModal(browser, '#field-help-modal');

                await browser.eval(`$('.field-help:not(.field-help-more)').first().tooltip('show')`);
                await browser.until(`document.querySelector('.tooltip.in .tooltip-inner') && document.querySelector('.tooltip.in .tooltip-inner').innerText.trim()`, 'the tooltip of a help icon');
            }
        },
        {
            name: 'admin-player-search',
            run: async browser => {
                await at(browser, fixtures.admin, desktop, '/admin/administrators');
                await browser.click('[data-admin-edit=""]', { settle: 0 });
                await browser.modalShown('#admin-edit-modal');

                const target = await browser.eval(`document.querySelector('#admin-edit-modal [data-player-picker]').getAttribute('data-player-picker')`);

                await browser.click('#admin-edit-modal [data-player-picker]', { settle: 0 });

                const picked = await pickFirstPlayer(browser);

                await browser.until(`!document.querySelector('#pp-modal').classList.contains('in') && document.querySelector('#admin-edit-modal').classList.contains('in')`, 'the search window to close over the add window');
                expect(await browser.eval(`document.querySelector(${quote(target)}).value`) === picked, `Picking ${picked} in the search window did not fill ${target}`);
                await closeModal(browser, '#admin-edit-modal');

                await browser.open('/admin/player-connections');
                await browser.click('[data-player-picker]', { settle: 0 });
                await browser.modalShown('#pp-modal');
                await browser.eval(`$('#pp-modal .pp-search').val(${quote(fixtures.players[2].name)}).trigger('input')`);
                await browser.until(`Array.from(document.querySelectorAll('#pp-modal .pp-player')).some(item => item.getAttribute('data-community-id') === ${quote(fixtures.players[2].id)})`, `${fixtures.players[2].name} in the search results`);
                await closeModal(browser, '#pp-modal');
            }
        },
        {
            name: 'admin-remote-commands',
            run: async browser => {
                await at(browser, fixtures.admin, desktop, '/admin/remote-commands');
                await browser.click('button[onclick="terminal.show()"]', { settle: 0 });
                await browser.modalShown('#terminal');

                expect(await browser.eval(`${commandOptions}.length`) > 10, 'The command builder lists no commands');
                await browser.eval(`${commandOptions}.filter((index, item) => $(item).data('value') === '!aac').first().click()`);
                await sleep(400);

                const options = await browser.eval(`${commandOptions}.map((index, item) => String($(item).data('value'))).get()`);

                expect(options.length > 0 && !options.includes('!aac'), `The command builder shows no options for !aac: ${options.join(', ')}`);
                await browser.eval(`${commandOptions}.first().click()`);
                await browser.modalHidden('#terminal');

                const command = await browser.eval(`document.querySelector('#command').value`);

                expect(command.startsWith('!aac '), `The command builder wrote "${command}"`);
                await browser.submit('#command');
                expect(await browser.eval(`document.body.innerText.includes(${quote(command.replace(/^!/, 'sm_'))}) || document.body.innerText.includes(${quote(command)})`), `The command ${command} is not in the audit log after sending`);
            }
        },
        {
            name: 'admin-chat-limits',
            run: async browser => {
                await at(browser, fixtures.admin, desktop, '/admin/chat-bans');

                const values = { '#GlobalCooldownInSeconds': '3', '#UserCooldownInSeconds': '7', '#MaxMessageLength': '150' };

                await browser.fill(values);
                await browser.submit('#GlobalCooldownInSeconds');
                expect(await browser.eval('location.pathname') === '/admin/chat-bans', 'Saving the chat limits did not return to the chat bans');
                expect(await browser.eval(`!!document.querySelector('.alert-success')`), 'Saving the chat limits shows no confirmation');

                const saved = await browser.eval(`Object.fromEntries(${quote(Object.keys(values))}.map(selector => [selector, document.querySelector(selector).value]))`);

                expect(Object.keys(values).every(selector => saved[selector] === values[selector]), `The chat limits were not saved: ${JSON.stringify(saved)}`);
            }
        },
        {
            name: 'admin-related-accounts',
            run: async browser => {
                const main = fixtures.players[13];
                const alternate = fixtures.players[14];

                for (const player of [main, alternate])
                    await postLive(context, '/api/game-info/player-connection-info', { communityId: player.id, ipAddress: '198.51.100.77', name: player.name }, 'POST');

                await at(browser, fixtures.admin, desktop, '/admin/chat-bans');
                await browser.click('.cb-add', { settle: 0 });
                await browser.modalShown('#cb-add-modal');
                await browser.fill({ '#cb-add-player': main.id });
                await browser.click('.cb-add-save', { settle: 0 });
                await browser.modalShown('#ra-modal');

                const offered = await browser.eval(`Array.from(document.querySelectorAll('#ra-modal .ra-list input:checked')).map(input => input.getAttribute('data-community-id'))`);

                expect(offered.length === 1 && offered[0] === alternate.id, `Banning ${main.name} offered the related accounts ${JSON.stringify(offered)} instead of ${alternate.name}`);
                await browser.click('#ra-modal [data-dismiss="modal"]', { settle: 0 });
                await browser.modalShown('#cb-add-modal');
                expect(await browser.eval(`!document.querySelector('#cb-add-modal .cb-add-save').disabled && ${chatBanIds}.length === 0`), 'Closing the related accounts window did not go back to the ban window without saving');

                const loaded = browser.page.waitFor('Page.loadEventFired');

                await browser.click('.cb-add-save', { settle: 0 });
                await browser.modalShown('#ra-modal');
                await browser.click('#ra-modal .ra-all', { settle: 0 });
                await loaded;
                await browser.page.idle();

                const banned = await browser.eval(chatBanIds);

                expect(banned.includes(main.id) && banned.includes(alternate.id), `Including the related account banned ${JSON.stringify(banned)} instead of ${main.name} and ${alternate.name}`);

                await browser.eval(`Promise.all(${quote([main.id, alternate.id])}.map(id => fetch('/api/chat-bans/' + id, { method: 'DELETE', headers: { RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value } }))).then(() => true)`);
            }
        },
        {
            name: 'admin-live-settings',
            run: async browser => {
                await at(browser, fixtures.admin, desktop, '/admin/servers');

                const values = { '#RefreshInMilliseconds': '2000', '#OnlineWindowInSeconds': '120', '#RemoteCommandExpirationInMinutes': '6' };

                await browser.fill(values);
                await browser.submit('#RefreshInMilliseconds');
                expect(await browser.eval(`!!document.querySelector('.alert-success')`), 'Saving the live settings shows no confirmation');

                const saved = await browser.eval(`Object.fromEntries(${quote(Object.keys(values))}.map(selector => [selector, document.querySelector(selector).value]))`);

                expect(Object.keys(values).every(selector => saved[selector] === values[selector]), `The live settings were not saved: ${JSON.stringify(saved)}`);
                expect(await browser.eval(`document.querySelector('#DelayInSeconds').value`) === '0', 'Saving the live settings changed the delay');
            }
        },
        {
            name: 'live-server',
            run: async browser => {
                await at(browser, null, desktop, '/servers');
                await pageOk(browser, 'The live page');
                await browser.until(`document.querySelector('#game-info-players') && document.querySelector('#game-info-players').innerText.includes(${quote(fixtures.players[0].name)})`, 'the players on the live page');

                const stamp = Date.now().toString(36);
                const message = `e2e live ${stamp}`;
                const renamed = `Live${stamp}`;
                const player = fixtures.players[0];
                const newcomer = '76561198099999991';

                await postLive(context, '/api/game-info/messages', { public: true, team: 2, communityId: player.id, name: player.name, isAdmin: false, message });
                await browser.until(`document.body.innerText.includes(${quote(message)})`, 'the chat message on the live page', liveTimeout);

                const survivor = { isAdmin: false, latency: 0.04, character: 0, permanentHealth: 100, temporaryHealth: 0, incapacitated: false, isPlayerAlive: true, blackAndWhite: false, primaryWeapon: 2, secondaryWeapon: 1, meleeWeapon: 0, progress: 0.5 };

                await postLive(context, '/api/game-info/players', {
                    survivors: [{ ...survivor, communityId: player.id, name: `${renamed} known` }, { ...survivor, communityId: newcomer, name: renamed }],
                    infecteds: [],
                    spectators: []
                });
                await browser.until(`document.querySelector('#game-info-players').innerText.includes(${quote(renamed)})`, 'the new players on the live page', liveTimeout);
                expect(await browser.eval(`document.querySelector('#game-info-players').innerText.includes(${quote(player.name)})`), 'The live page does not show the site name of a known player');
                expect(!(await browser.eval(`document.querySelector('#game-info-players').innerText.includes(${quote(`${renamed} known`)})`)), 'The live page shows the in-game name of a known player instead of the site name');
            }
        },
        {
            name: 'admin-replace-player',
            run: async browser => {
                await at(browser, fixtures.admin, desktop, '/matches/1');

                const map = await browser.eval(`document.querySelector('a[href*="/statistics/"]').getAttribute('href')`);

                await browser.open(map);
                await pageOk(browser, 'The map page of an administrator');
                const line = await browser.eval(`(() => { const button = document.querySelector('#tab-round-half-1 .ms-replace[data-survivor="true"]'); return button && { half: button.getAttribute('data-half'), player: button.getAttribute('data-player') }; })()`);
                const onMap = await browser.eval(`Array.from(new Set(Array.from(document.querySelectorAll('.ms-replace')).map(button => button.getAttribute('data-player'))))`);
                const pencils = await browser.eval(`document.querySelectorAll('.ms-replace').length`);

                expect(line, 'The map page has no pencil next to the survivors of the first round half');
                expect(pencils === onMap.length * 2, `The map page has ${pencils} pencils for ${onMap.length} players; each player should have one per round half`);

                await browser.click('#tab-round-half-1 .ms-replace[data-survivor="true"]', { settle: 0 });
                await browser.modalShown('#pp-modal');
                await browser.until(`document.querySelector('#pp-modal .pp-player')`, 'the players of the search window');

                const picked = await browser.eval(`(() => { const outside = Array.from(document.querySelectorAll('#pp-modal .pp-player')).find(item => !${quote(onMap)}.includes(item.getAttribute('data-community-id'))); if (!outside) return null; outside.setAttribute('data-e2e-pick', '1'); return outside.getAttribute('data-community-id'); })()`);

                expect(picked, 'The search window has no player who is outside the map');

                const loaded = browser.page.waitFor('Page.loadEventFired');

                await browser.click('#pp-modal [data-e2e-pick]', { settle: 0 });
                await browser.confirm();
                await loaded;
                await browser.page.idle();
                await pageOk(browser, 'The map page after replacing a player');

                const after = await browser.eval(`Array.from(document.querySelectorAll('#tab-round-half-1 .ms-replace[data-survivor="true"]')).map(button => button.getAttribute('data-player'))`);

                expect(after.includes(picked) && !after.includes(line.player), `After the replacement the survivors of the first round half are ${JSON.stringify(after)}: ${picked} should be there in the place of ${line.player}`);

                const back = await browser.eval(`fetch(matchesStatisticsConfig.values.replacePlayerUrl, { method: 'POST', headers: Object.assign({ 'Content-Type': 'application/json' }, matchesStatisticsConfig.values.antiforgeryHeaders), body: JSON.stringify({ half: ${Number(line.half)}, survivor: true, replaced: ${quote(picked)}, replacement: ${quote(line.player)} }) }).then(response => response.status)`);

                expect(back === 200, `Replacing the players back answered ${back}`);
            }
        },
        {
            name: 'languages',
            run: async browser => {
                await at(browser, null, phone);

                for (const language of languages) {
                    await browser.open(`/language?culture=${language.culture}&returnUrl=/matches`);
                    expect(await browser.eval('document.documentElement.lang') === language.culture, `The language ${language.culture} was not applied`);
                    expect(await browser.eval(`document.body.innerText.includes(${quote(language.matches)})`), `The ${language.culture} page does not say "${language.matches}"`);
                    await noHorizontalScroll(browser, `The matches page in ${language.culture}`);
                    await browser.open('/community');
                    await noHorizontalScroll(browser, `The community ranking in ${language.culture}`);
                }
            }
        },
        {
            name: 'light-theme',
            run: async browser => {
                await at(browser, null, desktop);

                try {
                    for (const address of ['/', '/matches/1', '/community']) {
                        await browser.theme('light');
                        await browser.open(address);

                        const state = await browser.eval(`(() => { const color = getComputedStyle(document.body).backgroundColor.match(/\\d+/g).map(Number); return { sheet: !!document.querySelector('link[href*="bootstrap.light"]'), light: (color[0] * 299 + color[1] * 587 + color[2] * 114) / 1000 > 160 }; })()`);

                        expect(state.sheet, `${address} does not load the light theme`);
                        expect(state.light, `${address} has a dark background in the light theme`);
                    }
                } finally {
                    await browser.theme('default');
                }
            }
        },
        ...championshipChecks(context),
        ...banChecks(context)
    ];
}

module.exports = { checks };
