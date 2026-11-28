const fs = require('fs');
const path = require('path');
const fixtures = require('./fixtures');
const { sleep } = require('./browser');

const desktop = 1280;
const phone = 390;
const admin = fixtures.admin;
const players = fixtures.players.map(player => player.id);
const quote = value => JSON.stringify(value);

function expect(condition, message) {
    if (!condition)
        throw new Error(message);
}

async function as(browser, user, width = desktop) {
    await browser.signIn(user);
    await browser.page.viewport(width, width < 700 ? 844 : 800);
}

async function problem(browser) {
    return browser.eval(`(() => { const alert = document.querySelector('.main-container > .alert-danger'); return alert ? alert.innerText.trim() : null; })()`);
}

async function ok(browser, label) {
    const status = await browser.eval(`performance.getEntriesByType('navigation')[0] ? performance.getEntriesByType('navigation')[0].responseStatus : 200`);
    const error = await problem(browser);

    expect(!status || status < 400, `${label} answered ${status}`);
    expect(!error, `${label} showed the error: ${error}`);
}

async function post(browser, form, values = {}, label = form) {
    const found = await browser.eval(`(() => {
        const form = document.querySelector(${quote(form)});
        if (!form) return false;
        const values = ${quote(values)};
        for (const [name, value] of Object.entries(values)) {
            const fields = form.querySelectorAll('[name="' + name + '"]');
            if (fields.length === 0) throw new Error('No field ' + name + ' in ' + ${quote(form)});
            const field = Array.from(fields).find(item => item.type !== 'hidden') || fields[0];
            if (field.type === 'checkbox') field.checked = !!value; else field.value = value;
        }
        form.removeAttribute('data-confirm');
        return true;
    })()`);

    expect(found, `There is no form ${form}`);
    await browser.submit(form);
    await ok(browser, label);
}

async function modal(browser, opener, selector) {
    await browser.click(opener, { settle: 100 });
    await browser.modalShown(selector);
}

async function save(browser, selector, label) {
    await browser.submit(`${selector} [type=submit]`);
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

async function tab(browser, url, name, query = '') {
    await browser.open(`${url}?tab=${name}${query}`);
    await ok(browser, `The ${name} tab of ${url}`);
    expect(await browser.eval(`!!document.querySelector('.admin-tabs li.active a[data-admin-tab="${name}"]')`), `The ${name} tab is not active`);
}

async function create(browser, name, template) {
    await browser.open('/admin/championships');
    await ok(browser, 'The championships administration');
    await modal(browser, '[data-target="#championship-add-modal"]', '#championship-add-modal');
    await browser.fill({ '#championship-name': name, '#championship-template': template });
    await save(browser, '#championship-add-modal', `Creating ${name}`);

    const url = await browser.eval('location.pathname');

    expect(/^\/admin\/championships\/\d+$/.test(url), `Creating ${name} went to ${url}`);

    return url;
}

async function settings(browser, url, values) {
    await tab(browser, url, 'settings');
    await browser.fill(Object.fromEntries(Object.entries(values).map(([name, value]) => [`#${name}`, value])));
    await browser.submit('#Name');
    await ok(browser, `The settings of ${url}`);
}

async function images(browser, url, art) {
    await tab(browser, url, 'settings');

    const set = await browser.eval(`(async () => {
        const art = ${quote(art)};
        const draw = (width, height, hue, seed, text, type) => new Promise(resolve => {
            const canvas = document.createElement('canvas');
            const g = canvas.getContext('2d');

            canvas.width = width;
            canvas.height = height;

            const background = g.createLinearGradient(0, 0, width, height);

            background.addColorStop(0, 'hsl(' + hue + ', 65%, 32%)');
            background.addColorStop(1, 'hsl(' + (hue + 70) + ', 60%, 10%)');
            g.fillStyle = background;
            g.fillRect(0, 0, width, height);

            for (let index = 0; index < 18; index++) {
                g.fillStyle = 'hsla(' + (hue + index * 23) + ', 85%, 60%, ' + (0.06 + (index % 4) * 0.03) + ')';
                g.beginPath();
                g.arc((index * 131 + seed * 57) % width, (index * 71 + seed * 29) % height, height * (0.08 + (index % 5) * 0.06), 0, Math.PI * 2);
                g.fill();
            }

            g.strokeStyle = 'rgba(255, 255, 255, 0.07)';
            g.lineWidth = Math.max(2, width / 300);

            for (let offset = -height; offset < width; offset += width / 14) {
                g.beginPath();
                g.moveTo(offset, height);
                g.lineTo(offset + height, 0);
                g.stroke();
            }

            if (text) {
                g.fillStyle = 'rgba(255, 255, 255, 0.92)';
                g.textAlign = 'center';
                g.textBaseline = 'middle';
                g.font = 'bold ' + Math.round(Math.min(width / (text.length * 0.65), height * 0.42)) + 'px sans-serif';
                g.fillText(text, width / 2, height / 2);
            }

            canvas.toBlob(resolve, type, 0.86);
        });
        const files = (selector, list) => {
            const transfer = new DataTransfer();

            for (const [blob, name] of list)
                transfer.items.add(new File([blob], name, { type: blob.type }));

            document.querySelector(selector).files = transfer.files;
        };

        if (art.banner)
            files('#Banner', [[await draw(1600, 500, art.hue, 1, '', 'image/jpeg'), 'banner.jpg']]);

        if (art.logo)
            files('#Logo', [[await draw(512, 512, art.hue + 30, 2, art.logo, 'image/png'), 'logo.png']]);

        if (art.gallery) {
            const list = [];

            for (let index = 0; index < art.gallery; index++)
                list.push([await draw(960, 640, art.hue + index * 40, index + 3, '#' + (index + 1), 'image/jpeg'), 'photo-' + (index + 1) + '.jpg']);

            files('#Gallery', list);
        }

        return true;
    })()`);

    expect(set, 'The images were not drawn');
    await browser.submit('#Name');
    await ok(browser, `The images of ${url}`);

    if (art.banner)
        expect(await browser.eval(`!!document.querySelector('.championship-image-banner img')`), 'The settings do not show the banner');

    if (art.logo)
        expect(await browser.eval(`!!document.querySelector('.championship-image-logo img')`), 'The settings do not show the logo');

    if (art.gallery)
        expect(await browser.eval(`document.querySelectorAll('.championship-image-gallery img').length`) >= art.gallery, 'The settings do not show the gallery');
}

async function landing(browser, context, slug, expected, suffix = '') {
    for (const width of [desktop, phone]) {
        await browser.page.viewport(width, width < 700 ? 844 : 800);
        await browser.open(`/championships/${slug}`);
        await ok(browser, `The landing of ${slug} at ${width}px`);

        const page = await browser.eval(`(() => {
            const loaded = image => !!image && image.complete && image.naturalWidth > 0;
            return {
                scroll: document.documentElement.scrollWidth,
                client: document.documentElement.clientWidth,
                hero: !!document.querySelector('.championship-hero.championship-hero-full h1.championship-hero-title'),
                banner: loaded(document.querySelector('.championship-hero-banner')),
                logo: loaded(document.querySelector('img.championship-hero-logo')),
                watch: document.querySelectorAll('#watch .championship-watch-link').length,
                teams: document.querySelectorAll('#teams .championship-team-card').length,
                captains: document.querySelectorAll('#teams .championship-team-card .label-warning').length,
                participants: document.querySelectorAll('#participants .championship-person').length,
                stages: document.querySelectorAll('#format .championship-stage').length,
                podium: !!document.querySelector('#podium .podium-place.first'),
                gallery: Array.from(document.querySelectorAll('#gallery a[data-lity] img')).filter(loaded).length,
                countdown: !!document.querySelector('.championship-next [data-countdown]'),
                join: !!document.querySelector('#join'),
                order: Array.from(document.querySelectorAll('.championship-landing > section')).map(section => section.id)
            };
        })()`);

        expect(page.scroll <= page.client + 1, `The landing of ${slug} scrolls sideways at ${width}px: ${page.scroll}px for ${page.client}px`);
        expect(page.hero, `The landing of ${slug} has no hero`);

        for (const [name, value] of Object.entries(expected))
            expect(page[name] === value, `The landing of ${slug} at ${width}px has ${name} ${JSON.stringify(page[name])}, expected ${JSON.stringify(value)}`);

        const known = ['about', 'join', 'watch','my-series', 'games', 'teams', 'participants', 'format', 'podium', 'gallery'];
        const positions = page.order.map(id => known.indexOf(id));

        expect(positions.every((position, index) => position >= 0 && (index === 0 || position > positions[index - 1])), `The sections of ${slug} are out of order: ${page.order.join(', ')}`);
        await shot(browser, context, `championship-landing-${slug}${suffix}-${width < 700 ? 'phone' : 'desktop'}`);
    }

    await browser.page.viewport(desktop, 800);
}

async function participants(browser, url, ids) {
    await tab(browser, url, 'participants');
    await modal(browser, '[data-target="#participants-add-modal"]', '#participants-add-modal');
    await browser.fill({ '#participants-players': ids.join('\n') });
    await save(browser, '#participants-add-modal', 'Adding participants');

    const count = await browser.eval(`Number(document.querySelector('.admin-tabs a[data-admin-tab="participants"] .badge').innerText)`);

    expect(count >= ids.length, `The championship has ${count} participants, expected ${ids.length}`);
}

async function teams(browser, url, count) {
    await tab(browser, url, 'teams');
    await modal(browser, '[data-target="#teams-add-modal"]', '#teams-add-modal');
    await browser.fill({ '#teams-add-count': count });
    await save(browser, '#teams-add-modal', 'Creating the teams');

    return browser.eval(`Array.from(document.querySelectorAll('form[action*="/teams/"][action$="/delete"]')).map(form => Number(form.getAttribute('action').split('/').slice(-2)[0]))`);
}

async function member(browser, url, team, player) {
    await post(browser, `form[action="${url}/teams/${team}/members"]`, { player }, `Adding ${player} to ${team}`);
}

async function leaders(browser, url, team, captain, vice) {
    await post(browser, `form[action="${url}/teams/${team}"]`, { captain, viceCaptain: vice || '' }, `The leaders of ${team}`);
}

async function stages(browser, url) {
    await tab(browser, url, 'stages');

    return browser.eval(`Array.from(document.querySelectorAll('form[action*="/stages/"]')).map(form => form.getAttribute('action').match(/stages\\/(\\d+)$/)).filter(Boolean).map(match => Number(match[1]))`);
}

async function stageForm(browser, url, stage, values) {
    await tab(browser, url, 'stages');
    await modal(browser, `[data-target="#stage-edit-modal-${stage}"]`, `#stage-edit-modal-${stage}`);

    for (const [name, value] of Object.entries(values))
        await browser.eval(`(() => { const field = Array.from(document.querySelectorAll('#stage-edit-modal-${stage} [name="${name}"]')).find(item => item.type !== 'hidden'); if (!field) throw new Error('No field ${name}'); if (field.type === 'checkbox') field.checked = ${quote(!!value)}; else field.value = ${quote(value)}; })()`);

    await save(browser, `#stage-edit-modal-${stage}`, `Saving the stage ${stage}`);
}

async function build(browser, url, stage, teams) {
    await tab(browser, url, 'stages');
    await modal(browser, `[data-target="#stage-build-modal-${stage}"]`, `#stage-build-modal-${stage}`);

    const entrants = await browser.eval(`document.querySelectorAll('#stage-build-modal-${stage} .championship-entrants li').length`);

    expect(entrants === teams, `The build window lists ${entrants} teams, expected ${teams}`);
    await save(browser, `#stage-build-modal-${stage}`, `Building the stage ${stage}`);
}

async function games(browser, url) {
    const all = [];

    for (let number = 1; number < 20; number++) {
        await tab(browser, url, 'games', `&gamesPage=${number}`);

        const rows = await browser.eval(`Array.from(document.querySelectorAll('.championship-games-table tbody tr')).map(row => {
            const buttons = Array.from(row.querySelectorAll('[data-game-edit]'));
            return {
                series: buttons.length ? Number(buttons[0].getAttribute('data-game-edit').match(/series\\/(\\d+)/)[1]) : 0,
                a: row.children[1].innerText.trim(),
                b: row.children[3].innerText.trim(),
                pending: buttons.filter(button => !button.disabled && button.classList.contains('btn-default')).map(button => button.getAttribute('data-game-edit')),
                notNeeded: buttons.filter(button => button.classList.contains('btn-link')).length,
                waiting: buttons.length > 0 && buttons.every(button => button.disabled)
            };
        })`);

        all.push(...rows);

        const last = await browser.eval(`!document.querySelector('.tab-pane.active .admin-pagination .pagination li:last-child:not(.disabled) a')`);

        if (last)
            break;
    }

    return all;
}

async function openGame(browser, url, action) {
    const page = await browser.eval(`!!document.querySelector('[data-game-edit="${action}"]')`);

    expect(page, `The game ${action} is not on the page`);
    await browser.click(`[data-game-edit="${action}"]`, { settle: 100 });
    await browser.modalShown('#game-edit-modal');
    await browser.until(`document.querySelector('#game-match option') && document.querySelector('#game-match option').innerText !== ${quote('Loading...')}`, 'the matches of the server');
}

async function typeResult(browser, url, series, action, winnerA, scores = [700, 400]) {
    await gamesPageOf(browser, url, series);
    await openGame(browser, url, action);
    await browser.eval(`document.querySelector('#game-edit-modal input[name="result"][value="typed"]').click()`);
    await browser.fill({ '#game-score-a': winnerA ? scores[0] : scores[1], '#game-score-b': winnerA ? scores[1] : scores[0] });
    await save(browser, '#game-edit-modal', `Typing a result in ${action}`);
}

async function gamesPageOf(browser, url, series) {
    for (let number = 1; number < 20; number++) {
        await tab(browser, url, 'games', `&gamesPage=${number}`);

        if (await browser.eval(`!!document.querySelector('[data-game-edit*="/series/${series}/"]')`))
            return;
    }

    throw new Error(`The series ${series} is not in the games tab`);
}

async function playSeries(browser, url, series, winnerA, scores) {
    for (let guard = 0; guard < 8; guard++) {
        const row = (await games(browser, url)).find(item => item.series === series);

        if (!row || row.pending.length === 0)
            return;

        await typeResult(browser, url, series, row.pending[0], winnerA, scores);
    }

    throw new Error(`The series ${series} did not finish`);
}

async function playAll(browser, url, decide) {
    for (let guard = 0; guard < 100; guard++) {
        const row = (await games(browser, url)).find(item => item.pending.length > 0);

        if (!row)
            return;

        await typeResult(browser, url, row.series, row.pending[0], decide(row));
    }

    throw new Error('The series did not finish');
}

async function finish(browser, url, label) {
    await settings(browser, url, { status: 'Finished' });
    expect(await browser.eval(`document.querySelector('.championship-status').innerText.trim()`) === 'Finished', `${label} is not finished`);
}

async function draftPicks(browser, url, picks) {
    for (let index = 0; index < picks; index++) {
        await tab(browser, url, 'draft');
        expect(await browser.eval(`!!document.querySelector('.championship-free-list form[action$="/draft/pick"] button:not([disabled])')`), `The draft has no player to pick at pick ${index + 1}`);
        await browser.submit('.championship-free-list form[action$="/draft/pick"] button');
        await ok(browser, `Draft pick ${index + 1}`);
    }
}

async function publicPages(browser, context, slug, tabs, label) {
    for (const name of tabs) {
        const address = name ? `/championships/${slug}/${name}` : `/championships/${slug}`;

        await browser.open(address);
        await ok(browser, `${label} ${address}`);
        expect(await browser.eval(`!!document.querySelector('.championship-tabs li.active')`), `${address} has no active tab`);
        await shot(browser, context, `championship-${slug}-${name || 'overview'}`);
    }
}

function championshipChecks(context) {
    return [
        {
            name: 'championships-league',
            timeout: 600000,
            run: async browser => {
                await as(browser, admin);

                const url = await create(browser, 'Liga E2E 2026', 'RoundRobin');

                await settings(browser, url, {
                    Slug: 'liga-e2e-2026',
                    Description: 'Every team plays every other team once, with one campaign drawn for each round.',
                    PlayersPerGame: 4,
                    RosterSize: 4,
                    GameConfiguration: fixtures.configuration,
                    StreamUrl: 'https://www.twitch.tv/example',
                    Rules: '# Format\n- Every team plays every other team **once**.\n- A win gives 3 points, a draw 1.\n\n## Schedule\nThe captains agree on the dates.',
                    RulesPortuguese: '# Formato\n- Todas as equipes se enfrentam **uma vez**.\n- Vitória vale 3 pontos, empate 1.'
                });
                await shot(browser, context, 'championships-admin-settings');
                await images(browser, url, { hue: 210, banner: true, logo: 'LE', gallery: 4 });
                await shot(browser, context, 'championships-admin-settings-images');

                await participants(browser, url, players);
                await shot(browser, context, 'championships-admin-participants');

                const ids = await teams(browser, url, 4);

                expect(ids.length === 4, `Expected 4 teams, got ${ids.length}`);

                for (let team = 0; team < 4; team++) {
                    for (let player = 0; player < 4; player++)
                        await member(browser, url, ids[team], players[team * 4 + player]);

                    await leaders(browser, url, ids[team], players[team * 4], players[team * 4 + 1]);
                }

                await tab(browser, url, 'teams');
                await shot(browser, context, 'championships-admin-teams');
                await modal(browser, `[data-target="#team-players-modal-${ids[0]}"]`, `#team-players-modal-${ids[0]}`);
                expect(await browser.eval(`document.querySelectorAll('#team-players-modal-${ids[0]} .championship-members tr').length`) === 4, 'The players window of a team does not list its 4 players');
                await shot(browser, context, 'championships-admin-team-players');

                const [stage] = await stages(browser, url);

                await build(browser, url, stage, 4);

                const all = await games(browser, url);

                expect(all.length === 6, `The league has ${all.length} series, expected 6`);

                const linked = [['Team A', 'Team B', 1], ['Team C', 'Team D', 2], ['Team A', 'Team C', 5]];

                for (const [teamA, teamB, match] of linked) {
                    const item = all.find(entry => entry.a === teamA && entry.b === teamB || entry.a === teamB && entry.b === teamA);

                    expect(item, `There is no series ${teamA} x ${teamB}`);
                    await gamesPageOf(browser, url, item.series);
                    await openGame(browser, url, item.pending[0]);

                    const available = await browser.eval(`Array.from(document.querySelectorAll('#game-match option')).map(option => option.value)`);

                    expect(available.includes(String(match)), `The match #${match} is not offered for ${teamA} x ${teamB}: ${available.join(', ')}`);
                    await browser.eval(`document.querySelector('#game-edit-modal input[name="result"][value="match"]').click()`);
                    await browser.fill({ '#game-match': String(match) });

                    if (match === 5)
                        await shot(browser, context, 'championships-admin-game');

                    await save(browser, '#game-edit-modal', `Linking the match #${match}`);
                }

                await tab(browser, url, 'games');
                expect(await browser.eval(`document.querySelectorAll('.championship-game-buttons .btn-success').length`) === 3, 'The three linked games are not shown as played');
                await shot(browser, context, 'championships-admin-games');

                const firstSeries = all[0].series;

                await modal(browser, `[data-target="#series-edit-modal-${firstSeries}"]`, `#series-edit-modal-${firstSeries}`);
                await shot(browser, context, 'championships-admin-series');
                await browser.eval(`$('#series-edit-modal-${firstSeries}').modal('hide')`);
                await sleep(400);

                await tab(browser, url, 'stages');
                await shot(browser, context, 'championships-admin-stages');
                await modal(browser, `[data-target="#stage-edit-modal-${stage}"]`, `#stage-edit-modal-${stage}`);
                await shot(browser, context, 'championships-admin-stage-edit');

                await playAll(browser, url, item => item.a < item.b);
                await finish(browser, url, 'The league');

                await tab(browser, url, 'history');
                expect(await browser.eval(`document.querySelectorAll('.championship-history tbody tr').length`) === 20, 'The history does not show a full page');
                expect(await browser.eval(`!!document.querySelector('.tab-pane.active .admin-pagination .pagination')`), 'The history has no pagination');
                await shot(browser, context, 'championships-admin-history');

                await as(browser, null);
                await publicPages(browser, context, 'liga-e2e-2026', ['', 'rules', 'teams', 'standings', 'games', 'statistics'], 'The league');

                await landing(browser, context, 'liga-e2e-2026', { banner: true, logo: true, watch: 1, teams: 4, captains: 4, participants: 16, stages: 1, podium: true, gallery: 4, countdown: false });

                await browser.open('/championships/liga-e2e-2026/statistics');
                const rows = await browser.eval(`document.querySelectorAll('.championship-statistics tbody tr').length`);

                expect(rows >= 12, `The statistics list ${rows} players, expected at least 12 from the linked matches`);

                await browser.open('/championships/liga-e2e-2026/standings');
                expect(await browser.eval(`document.querySelectorAll('.championship-standings tbody tr').length`) === 4, 'The standings do not have 4 teams');

                await browser.open('/matches/1');
                expect(await browser.eval(`!!document.querySelector('.match-championship a[href^="/championships/liga-e2e-2026/series/"]')`), 'The match page does not link to its championship');

                await browser.open('/language?culture=pt-BR&returnUrl=/championships/liga-e2e-2026/rules');
                expect(await browser.eval(`document.querySelector('.championship-rules h3').innerText.trim()`) === 'Formato', 'The rules are not shown in Portuguese');
                await browser.open('/language?culture=en-US&returnUrl=/');
            }
        },
        {
            name: 'championships-snake-knockout',
            timeout: 600000,
            run: async browser => {
                await as(browser, admin);

                const url = await create(browser, 'Copa Mata-Mata E2E', 'Knockout');

                await settings(browser, url, { Slug: 'copa-mata-mata-e2e', PlayersPerGame: 4, RosterSize: 4, DraftMethod: 'Snake', Description: 'Four captains draft their teams and play a knockout to the final, with a match for the third place.', StreamUrl: 'https://www.twitch.tv/example', VideosUrl: 'https://www.youtube.com/@example' });
                await images(browser, url, { hue: 0, banner: true, logo: 'MM', gallery: 3 });
                await participants(browser, url, players);

                const ids = await teams(browser, url, 4);

                for (let team = 0; team < 4; team++)
                    await leaders(browser, url, ids[team], players[team * 4], null);

                await tab(browser, url, 'draft');
                await post(browser, `form[action="${url}/draft/order"]`, {}, 'Drawing the order of the captains');
                await draftPicks(browser, url, 6);
                await tab(browser, url, 'draft');
                await shot(browser, context, 'championships-admin-draft');
                await draftPicks(browser, url, 6);

                await tab(browser, url, 'draft');
                expect(await browser.eval(`!document.querySelector('.championship-free-list')`), 'The draft is not complete after 12 picks');

                const [stage] = await stages(browser, url);

                await stageForm(browser, url, stage, { Name: 'Playoffs', BestOf: 3, FinalBestOf: 3, ThirdPlace: true, CampaignRule: 'Picks', Veto: true, OncePerTeam: true, MinimumMaps: 0, Seeding: 'Seeds' });
                await build(browser, url, stage, 4);

                await as(browser, null);
                await browser.open('/championships/copa-mata-mata-e2e/draft');
                await ok(browser, 'The public draft');
                expect(await browser.eval(`document.querySelectorAll('table tbody tr').length`) === 12, 'The public draft does not list 12 picks');
                await shot(browser, context, 'championship-copa-mata-mata-e2e-draft');

                await browser.open('/championships/copa-mata-mata-e2e/bracket');
                const semifinal = await browser.eval(`(() => { const link = document.querySelector('.bracket-round:first-child a.championship-series'); return { url: link.getAttribute('href'), id: Number(link.getAttribute('href').split('/').pop()), a: link.querySelector('.team-a').innerText.trim(), b: link.querySelector('.team-b').innerText.trim() }; })()`);

                await as(browser, admin);
                await tab(browser, url, 'teams');

                const captains = await browser.eval(`(() => { const result = {}; document.querySelectorAll('[id^="team-edit-modal-"]').forEach(modal => { const name = modal.querySelector('input[name=name]').value; result[name] = modal.querySelector('select[name=captain]').value; }); return result; })()`);
                const captainA = captains[semifinal.a];
                const captainB = captains[semifinal.b];

                expect(captainA && captainB, `The captains of ${semifinal.a} and ${semifinal.b} were not found`);

                await as(browser, captainA);
                await browser.open(semifinal.url);
                await ok(browser, 'The series page of a captain');
                expect(await browser.eval(`!!document.querySelector('.championship-captain')`), 'The captain does not see the panel of the team');
                await post(browser, `form[action="${semifinal.url}/veto"]`, {}, 'The veto of team A');

                await as(browser, captainB);
                await browser.open(semifinal.url);
                await post(browser, `form[action="${semifinal.url}/veto"]`, {}, 'The veto of team B');

                const pickB = await browser.eval(`(() => { const form = document.querySelector('form[action$="/pick"]'); return form ? form.getAttribute('action') : null; })()`);

                expect(pickB, 'Team B has no game to choose');
                await post(browser, `form[action="${pickB}"]`, {}, 'The pick of team B');

                const date = new Date(Date.now() + 3 * 86400000);
                const local = `${date.getUTCFullYear()}-${String(date.getUTCMonth() + 1).padStart(2, '0')}-${String(date.getUTCDate()).padStart(2, '0')}T21:00`;

                await post(browser, `form[action="${semifinal.url}/proposals"]`, { date: local }, 'Proposing a date');

                await as(browser, captainA);
                await browser.open(semifinal.url);

                const pickA = await browser.eval(`(() => { const form = document.querySelector('form[action$="/pick"]'); return form ? form.getAttribute('action') : null; })()`);

                await post(browser, `form[action="${pickA}"]`, {}, 'The pick of team A');

                const answer = await browser.eval(`document.querySelector('form[action*="/proposals/"][action$="/answer"]').getAttribute('action')`);

                await post(browser, `form[action="${answer}"]`, { accept: 'true' }, 'Accepting the date');
                expect(await browser.eval(`!!document.querySelector('.championship-series-dates a[href*="calendar.google.com"]')`), 'The confirmed date has no calendar link');
                await shot(browser, context, 'championship-copa-mata-mata-e2e-series-captain');

                await as(browser, admin);
                await playSeries(browser, url, semifinal.id, true, [800, 300]);

                const played = (await games(browser, url)).find(item => item.series === semifinal.id);

                expect(played.notNeeded === 1, 'The third game of a 2-0 is not marked as not needed');

                await playAll(browser, url, item => item.a > item.b);
                await finish(browser, url, 'The cup');

                await as(browser, null);
                await publicPages(browser, context, 'copa-mata-mata-e2e', ['', 'teams', 'bracket', 'games'], 'The cup');
                await browser.open('/championships/copa-mata-mata-e2e');
                expect(await browser.eval(`document.querySelectorAll('.championship-podium .podium-place').length`) === 3, 'The cup does not show the three places');
                await landing(browser, context, 'copa-mata-mata-e2e', { banner: true, logo: true, watch: 2, teams: 4, captains: 4, participants: 16, stages: 1, podium: true, gallery: 3 });
            }
        },
        {
            name: 'championships-groups-roulette',
            timeout: 600000,
            run: async browser => {
                await as(browser, admin);

                const url = await create(browser, 'Copa dos Grupos E2E', 'GroupsAndKnockout');

                await settings(browser, url, { Slug: 'copa-dos-grupos-e2e', PlayersPerGame: 2, RosterSize: 2, DraftMethod: 'Roulette' });
                await images(browser, url, { hue: 130, banner: true, logo: 'CG' });
                await participants(browser, url, players);
                await teams(browser, url, 8);

                for (let spin = 0; spin < 16; spin++) {
                    await tab(browser, url, 'draft');
                    await post(browser, `form[action="${url}/draft/spin"]`, {}, `Spin ${spin + 1}`);
                }

                await tab(browser, url, 'draft');
                expect(await browser.eval(`document.querySelectorAll('.championship-picks tbody tr').length`) === 16, 'The roulette did not place 16 players');

                const [groups, knockout] = await stages(browser, url);

                await stageForm(browser, url, knockout, { Name: 'Playoffs', BestOf: 1, FinalBestOf: 3, CampaignRule: 'Picks', Seeding: 'Standings', MinimumMaps: 4 });
                await build(browser, url, groups, 8);

                const all = await games(browser, url);

                expect(all.length === 12, `The groups have ${all.length} series, expected 12`);
                await playAll(browser, url, item => item.a < item.b);
                await build(browser, url, knockout, 4);
                await playAll(browser, url, item => item.a < item.b);
                await finish(browser, url, 'The groups');

                await as(browser, null);
                await publicPages(browser, context, 'copa-dos-grupos-e2e', ['', 'standings', 'bracket', 'draft', 'games'], 'The groups');
                await browser.open('/championships/copa-dos-grupos-e2e/standings');
                expect(await browser.eval(`document.querySelectorAll('.championship-standings').length`) === 2, 'The groups standings do not show two groups');
                expect(await browser.eval(`document.querySelectorAll('.championship-qualified').length`) === 4, 'The groups do not mark two qualified teams each');
            }
        },
        {
            name: 'championships-byes-pots',
            timeout: 600000,
            run: async browser => {
                await as(browser, admin);

                const url = await create(browser, 'Liga com Mata-Mata E2E', 'RoundRobinAndKnockout');

                await settings(browser, url, { Slug: 'liga-com-mata-mata-e2e', PlayersPerGame: 2, RosterSize: 2, DraftMethod: 'Pots', Description: 'A league of six pairs drawn from two pots, then a knockout where the two best seeds wait for the semifinals.', startsOn: '2026-11-20', endsOn: '2027-01-31' });
                await images(browser, url, { hue: 280, logo: 'LM' });
                await participants(browser, url, players.slice(0, 12));

                for (let index = 0; index < 12; index++) {
                    await tab(browser, url, 'participants');
                    await browser.click(`[data-participant-pot="${url}/participants/${players[index]}/pot"]`, { settle: 100 });
                    await browser.modalShown('#participant-pot-modal');
                    await browser.fill({ '#participant-pot': index < 6 ? 1 : 2 });
                    await save(browser, '#participant-pot-modal', `The pot of ${players[index]}`);
                }

                await teams(browser, url, 6);
                await tab(browser, url, 'draft');
                await post(browser, `form[action="${url}/draft/pots"]`, {}, 'Drawing the pots');

                const [league, knockout] = await stages(browser, url);

                await stageForm(browser, url, league, { Name: 'League', Qualifiers: 6, PointsForWin: 3, PointsForDraw: 1, PointsForLoss: 0, GroupCount: 1, Legs: 1, BestOf: 1, CampaignRule: 'RoundDraw', MinimumMaps: 4 });
                await stageForm(browser, url, knockout, { Name: 'Playoffs', BestOf: 3, FinalBestOf: 5, CampaignRule: 'Picks', Seeding: 'Standings', MinimumMaps: 0 });
                await build(browser, url, league, 6);
                await playAll(browser, url, item => item.a < item.b);

                await tab(browser, url, 'stages');
                await modal(browser, `[data-target="#stage-penalties-modal-${league}"]`, `#stage-penalties-modal-${league}`);
                await post(browser, `form[action="${url}/stages/${league}/penalty"]`, { points: 1, reason: 'Late for round 2' }, 'Adding a penalty');
                await build(browser, url, knockout, 6);

                await tab(browser, url, 'games');
                await modal(browser, '[data-target="#games-deadline-modal"]', '#games-deadline-modal');
                await browser.fill({ '#deadline-stage': String(knockout), '#deadline-round': 1, '#deadline-date': '2027-01-10T23:59' });
                await save(browser, '#games-deadline-modal', 'Setting a deadline');
                expect(await browser.eval(`document.body.innerText.includes('Until')`), 'The deadline does not show in the games');

                const scheduled = (await games(browser, url)).filter(item => item.pending.length > 0).slice(0, 2);

                expect(scheduled.length === 2, `The knockout has ${scheduled.length} series to schedule, expected 2`);

                for (const [index, item] of scheduled.entries()) {
                    const date = new Date(Date.now() + (index * 3 + 2) * 86400000);
                    const local = `${date.getUTCFullYear()}-${String(date.getUTCMonth() + 1).padStart(2, '0')}-${String(date.getUTCDate()).padStart(2, '0')}T20:30`;

                    await gamesPageOf(browser, url, item.series);
                    await post(browser, `form[action="${url}/series/${item.series}"]`, { scheduledAt: local }, `Scheduling the series ${item.series}`);
                }

                await as(browser, null);
                await browser.open('/championships/liga-com-mata-mata-e2e/bracket');
                await ok(browser, 'The bracket with byes');
                expect(await browser.eval(`document.querySelectorAll('.bracket-round').length`) === 3, 'The bracket of six teams does not have three rounds');
                expect(await browser.eval(`document.querySelectorAll('.bracket-round:first-child a.championship-series.status-bye').length`) === 2, 'The two best seeds do not have a bye');
                await publicPages(browser, context, 'liga-com-mata-mata-e2e', ['', 'standings', 'bracket', 'draft'], 'The league with knockout');
                await landing(browser, context, 'liga-com-mata-mata-e2e', { banner: false, logo: true, watch: 0, teams: 6, participants: 12, stages: 2, podium: false, gallery: 0, countdown: true });
                expect(await browser.eval(`/^\\d{2}$/.test(document.querySelector('[data-countdown-seconds]').innerText.trim())`), 'The countdown does not show the seconds');

                await as(browser, null, phone);

                for (const address of ['/championships', '/championships/liga-com-mata-mata-e2e', '/championships/liga-com-mata-mata-e2e/bracket', '/championships/liga-com-mata-mata-e2e/standings']) {
                    await browser.open(address);

                    const sizes = await browser.eval(`({ scroll: document.documentElement.scrollWidth, client: document.documentElement.clientWidth })`);

                    expect(sizes.scroll <= sizes.client + 1, `${address} scrolls sideways on a phone: ${sizes.scroll}px for ${sizes.client}px`);
                    await shot(browser, context, `championship-phone${address.replace(/\//g, '-')}`);
                }
            }
        },
        {
            name: 'championships-list',
            timeout: 600000,
            run: async browser => {
                await as(browser, admin);

                const cancelled = await create(browser, 'Copa Cancelada E2E', 'Knockout');

                await settings(browser, cancelled, { status: 'Cancelled', reason: 'Not enough teams' });
                await create(browser, 'Rascunho Secreto E2E', 'RoundRobin');

                const open = await create(browser, 'Copa Aberta E2E', 'Knockout');

                await settings(browser, open, { Slug: 'copa-aberta-e2e', Description: 'Registrations are open: ask for a place and the administration approves each request.', PlayersPerGame: 2, RosterSize: 2, MaxParticipants: 8, JoinRequests: true, status: 'Participants', StreamUrl: 'https://www.twitch.tv/example', startsOn: '2027-02-01' });
                await images(browser, open, { hue: 40, banner: true, logo: 'CA' });
                await participants(browser, open, players.slice(10, 13));

                await as(browser, players[0]);
                await landing(browser, context, 'copa-aberta-e2e', { banner: true, logo: true, join: true, participants: 3 }, '-join');
                expect(await browser.eval(`!!document.querySelector('.championship-hero-cta[href="#join"]')`), 'The open championship has no button to ask to join in the hero');
                await post(browser, 'form[action="/championships/copa-aberta-e2e/join"]', { message: 'I can play every night after 9 PM.' }, 'Asking to join');
                expect(await browser.eval(`!!document.querySelector('form[action="/championships/copa-aberta-e2e/join/cancel"]')`), 'The pending request cannot be cancelled');

                await as(browser, players[1]);
                await browser.open('/championships/copa-aberta-e2e');
                await post(browser, 'form[action="/championships/copa-aberta-e2e/join"]', {}, 'Asking to join again');

                await as(browser, admin);
                await tab(browser, open, 'participants');
                expect(await browser.eval(`document.querySelectorAll('.championship-requests tbody tr').length`) === 2, 'The participants tab does not list the two requests');
                expect(await browser.eval(`document.querySelector('.championship-requests').innerText.includes('I can play every night after 9 PM.')`), 'The request does not show its message');
                await shot(browser, context, 'championships-admin-requests');

                const [approve, refuse] = await browser.eval(`(() => { const rows = document.querySelectorAll('.championship-requests tbody tr'); return [rows[0].querySelector('form[action$="/approve"]').getAttribute('action'), rows[1].querySelector('form[action$="/refuse"]').getAttribute('action')]; })()`);

                await post(browser, `form[action="${approve}"]`, {}, 'Approving a request');
                await tab(browser, open, 'participants');
                await post(browser, `form[action="${refuse}"]`, {}, 'Refusing a request');
                await tab(browser, open, 'participants');
                expect(await browser.eval(`Number(document.querySelector('.admin-tabs a[data-admin-tab="participants"] .badge').innerText)`) === 4, 'The approved player is not a participant');

                await as(browser, players[0]);
                await browser.open('/championships/copa-aberta-e2e');
                expect(await browser.eval(`!!document.querySelector('#join .text-success')`), 'The approved player is not told that the request was approved');

                await as(browser, players[1], phone);
                await browser.open('/championships/copa-aberta-e2e');
                expect(await browser.eval(`!!document.querySelector('#join .text-danger')`), 'The refused player is not told that the request was refused');
                await shot(browser, context, 'championship-join-refused-phone');
                await as(browser, admin);

                await browser.open('/admin/championships');
                expect(await browser.eval(`document.querySelectorAll('.admin-table tbody tr').length`) >= 6, 'The administration does not list every championship');
                expect(await browser.eval(`document.querySelectorAll('.admin-table tbody tr .admin-row-actions a, .admin-table tbody tr .admin-row-actions button').length`) >= 12, 'The rows have no Edit and Delete buttons');
                await shot(browser, context, 'championships-admin-list');

                for (const name of ['settings', 'participants', 'teams', 'draft', 'stages', 'games', 'history'])
                    await tab(browser, '/admin/championships/2', name);

                await browser.open('/admin/championships/1?tab=teams');
                await browser.click('.admin-tabs a[data-admin-tab="stages"]', { settle: 300 });
                expect(await browser.eval(`location.search.includes('tab=stages')`), 'Switching tabs does not keep the tab in the address');

                await as(browser, null);
                await browser.open('/championships');
                await ok(browser, 'The championships page');

                const text = await browser.eval('document.body.innerText');

                expect(!text.includes('Rascunho Secreto E2E'), 'A draft is listed to visitors');
                expect(text.includes('Copa Cancelada E2E'), 'The cancelled championship is not in the history');
                expect(await browser.eval(`document.querySelectorAll('.championship-history .championship-card').length`) >= 4, 'The history does not list the finished and cancelled championships');
                expect(await browser.eval(`document.querySelectorAll('.championship-cards-current .championship-card').length`) >= 1, 'The championships in progress are not shown as cards');
                expect(await browser.eval(`Array.from(document.querySelectorAll('.championship-card-banner img')).filter(image => image.complete && image.naturalWidth > 0).length`) >= 3, 'The cards do not show the banners');
                expect(await browser.eval(`!!document.querySelector('.championship-history .championship-card-champion')`), 'The finished cards do not show the champion');
                await shot(browser, context, 'championships-list');

                const hidden = await fetch(`${context.base}/championships/rascunho-secreto-e2e`);

                expect(hidden.status === 404, `A draft answered ${hidden.status} to visitors`);

                for (const culture of ['pt-BR', 'es-ES']) {
                    await browser.open(`/language?culture=${culture}&returnUrl=/championships/copa-mata-mata-e2e/bracket`);
                    await ok(browser, `The bracket in ${culture}`);
                    await shot(browser, context, `championship-bracket-${culture}`);
                }

                await as(browser, admin);
                await browser.open('/language?culture=pt-BR&returnUrl=/admin/championships/1?tab=games');
                await ok(browser, 'The games tab in Portuguese');
                await shot(browser, context, 'championships-admin-games-pt-BR');
                await browser.open('/language?culture=en-US&returnUrl=/');
                await browser.theme('light');
                await browser.open('/championships/liga-e2e-2026/standings');
                await ok(browser, 'The standings in the light theme');
                await shot(browser, context, 'championship-standings-light');
                await landing(browser, context, 'liga-e2e-2026', { banner: true, podium: true }, '-light');
                await browser.open('/championships');
                await shot(browser, context, 'championships-list-light');
                await browser.theme('default');
            }
        }
    ];
}

module.exports = { championshipChecks };
