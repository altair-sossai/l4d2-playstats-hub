const text = {
    en: {
        cup: 'Autumn Cup 2026',
        cupDescription: 'Four teams drafted by their captains play a league, and then a knockout until the final.',
        league: 'Winter League 2026',
        leagueDescription: 'Every team plays every other team once, with one campaign drawn for each round.',
        open: 'Summer Cup 2027',
        openDescription: 'Registrations are open: ask for a place and the administration approves each request.',
        rules: '# Format\n- Every team plays every other team **once**.\n- The four teams go to the semifinals, in best of 3.\n\n# Schedule\n- The captains agree on the dates in the captain area.\n- A team that does not show up loses by walkover.',
        message: 'I can play every evening.',
        teams: ['Night Shift', 'Safe Room', 'Tank Rush', 'Pills First']
    },
    pt: {
        cup: 'Copa de Outono 2026',
        cupDescription: 'Quatro equipes formadas no draft pelos capitães jogam uma liga e depois um mata-mata até a final.',
        league: 'Liga de Inverno 2026',
        leagueDescription: 'Todas as equipes se enfrentam uma vez, com uma campanha sorteada para cada rodada.',
        open: 'Copa de Verão 2027',
        openDescription: 'As inscrições estão abertas: peça uma vaga e a administração aprova cada pedido.',
        rules: '# Formato\n- Todas as equipes se enfrentam **uma vez**.\n- As quatro equipes vão para as semifinais, em melhor de 3.\n\n# Datas\n- Os capitães combinam as datas na área do capitão.\n- A equipe que não comparecer perde por W.O.',
        message: 'Posso jogar todas as noites.',
        teams: ['Turno da Noite', 'Abrigo', 'Tank na Rampa', 'Pílula Primeiro']
    },
    es: {
        cup: 'Copa de Otoño 2026',
        cupDescription: 'Cuatro equipos formados en el draft por sus capitanes juegan una liga y después una eliminatoria hasta la final.',
        league: 'Liga de Invierno 2026',
        leagueDescription: 'Todos los equipos se enfrentan una vez, con una campaña sorteada para cada ronda.',
        open: 'Copa de Verano 2027',
        openDescription: 'Las inscripciones están abiertas: pide un lugar y la administración aprueba cada pedido.',
        rules: '# Formato\n- Todos los equipos se enfrentan **una vez**.\n- Los cuatro equipos van a las semifinales, al mejor de 3.\n\n# Fechas\n- Los capitanes acuerdan las fechas en el área del capitán.\n- El equipo que no se presenta pierde por walkover.',
        message: 'Puedo jugar todas las noches.',
        teams: ['Turno de Noche', 'Refugio', 'Tank en la Rampa', 'Pastillas Primero']
    }
};

const slugs = { cup: 'autumn-cup-2026', league: 'winter-league-2026', open: 'summer-cup-2027' };
const tags = ['NS', 'SR', 'TR', 'PF'];
const hues = [260, 130, 0, 45];
const teamSize = 4;
const picksBeforeTheShot = 6;
const dayInMilliseconds = 24 * 60 * 60 * 1000;
const quote = value => JSON.stringify(value);

const helpers = `window.shots = window.shots || {
    draw(width, height, hue, seed, label, type) {
        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');

        canvas.width = width;
        canvas.height = height;

        const background = context.createLinearGradient(0, 0, width, height);

        background.addColorStop(0, 'hsl(' + hue + ', 60%, 34%)');
        background.addColorStop(1, 'hsl(' + (hue + 60) + ', 55%, 12%)');
        context.fillStyle = background;
        context.fillRect(0, 0, width, height);

        for (let index = 0; index < 16; index++) {
            context.fillStyle = 'hsla(' + (hue + index * 19) + ', 80%, 60%, ' + (0.05 + (index % 4) * 0.03) + ')';
            context.beginPath();
            context.arc((index * 137 + seed * 59) % width, (index * 73 + seed * 31) % height, height * (0.08 + (index % 5) * 0.06), 0, Math.PI * 2);
            context.fill();
        }

        if (label) {
            context.fillStyle = 'rgba(255, 255, 255, 0.92)';
            context.textAlign = 'center';
            context.textBaseline = 'middle';
            context.font = 'bold ' + Math.round(height * 0.42) + 'px sans-serif';
            context.fillText(label, width / 2, height / 2 + height * 0.03);
        }

        return new Promise(resolve => canvas.toBlob(resolve, type, 0.86));
    },
    async post(selector, values, files) {
        const form = document.querySelector(selector);

        if (!form)
            throw new Error('There is no form ' + selector);

        const data = new FormData(form);

        for (const [name, value] of Object.entries(values || {})) {
            data.delete(name);
            [].concat(value).forEach(item => data.append(name, item));
        }

        for (const file of files || []) {
            data.delete(file.name);
            data.append(file.name, await this.draw(...file.draw), file.name + (file.draw[5] === 'image/png' ? '.png' : '.jpg'));
        }

        const multipart = form.enctype === 'multipart/form-data';
        const response = await fetch(form.getAttribute('action'), { method: 'POST', body: multipart ? data : new URLSearchParams(data) });
        const page = new DOMParser().parseFromString(await response.text(), 'text/html');
        const error = page.querySelector('.main-container > .alert-danger:not(.player-ban)');

        if (!response.ok || error)
            throw new Error(form.getAttribute('action') + ' answered ' + response.status + (error ? ': ' + error.textContent.trim() : ''));

        return new URL(response.url).pathname;
    }
}`;

class Championship {
    constructor(runner) {
        this.runner = runner;
        this.url = null;
    }

    async open(address) {
        await this.runner.page.goto(this.runner.url(address), 100);
        await this.runner.page.evaluate(`${helpers}; true`);
    }

    tab(name) {
        return this.open(`${this.url}?tab=${name}`);
    }

    post(selector, values = {}, files = []) {
        return this.runner.page.evaluate(`window.shots.post(${quote(selector)}, ${quote(values)}, ${quote(files)})`);
    }

    eval(expression) {
        return this.runner.page.evaluate(expression);
    }

    async create(name, template) {
        await this.open('/admin/championships');
        this.url = await this.post('#championship-add-modal form', { name, template });
    }

    async settings(values, files = []) {
        await this.tab('settings');
        await this.post(`form[action="${this.url}"]`, values, files);
    }

    async participants(ids) {
        await this.tab('participants');
        await this.post('#participants-add-modal form', { players: ids.join('\n') });
    }

    async teams(names, captains) {
        await this.tab('teams');
        await this.post('#teams-add-modal form', { count: names.length });
        await this.tab('teams');

        const ids = await this.eval(`Array.from(document.querySelectorAll('[id^="team-edit-modal-"]')).map(modal => Number(modal.id.split('-').pop())).sort((a, b) => a - b)`);

        for (let index = 0; index < ids.length; index++)
            await this.post(`#team-edit-modal-${ids[index]} form`, { name: names[index], tag: tags[index], captain: captains[index] }, [{ name: 'flag', draw: [512, 512, hues[index], index + 1, tags[index], 'image/png'] }]);

        return ids;
    }

    async member(team, player) {
        await this.tab('teams');
        await this.post(`form[action="${this.url}/teams/${team}/members"]`, { player });
    }

    async pick() {
        await this.tab('draft');
        await this.post('.championship-free-list form[action$="/draft/pick"]');
    }

    async stages() {
        await this.tab('stages');

        return this.eval(`Array.from(document.querySelectorAll('[id^="stage-edit-modal-"]')).map(modal => Number(modal.id.split('-').pop())).sort((a, b) => a - b)`);
    }

    async stage(id, values) {
        await this.tab('stages');
        await this.post(`#stage-edit-modal-${id} form`, values);
    }

    async build(id) {
        await this.tab('stages');
        await this.post(`#stage-build-modal-${id} form`);
    }

    async pending() {
        await this.tab('games');

        return this.eval(`Array.from(document.querySelectorAll('.championship-games-table tbody tr')).map(row => {
            const games = Array.from(row.querySelectorAll('[data-game-edit]')).filter(button => !button.disabled && button.classList.contains('btn-default')).map(button => button.getAttribute('data-game-edit'));

            return { games, series: games.length ? Number(games[0].match(/series\\/(\\d+)/)[1]) : 0, a: row.children[1].innerText.trim(), b: row.children[3].innerText.trim() };
        }).filter(row => row.games.length > 0)`);
    }

    async result(game, scoreA, scoreB) {
        await this.eval(`document.querySelector('#game-form').setAttribute('action', ${quote(game)}); true`);
        await this.post('#game-form', { campaignId: '', originalCampaignId: '', result: 'typed', teamAScore: scoreA, teamBScore: scoreB });
    }

    async schedule(series, date) {
        await this.tab('games');
        await this.post(`#series-form-${series}`, { scheduledAt: date });
    }
}

function day(offset, time = '') {
    const date = new Date(Date.now() + offset * dayInMilliseconds).toISOString().slice(0, 10);

    return time ? `${date}T${time}` : date;
}

async function playRoundRobin(championship, strength) {
    for (let game = 0; ; game++) {
        const [series] = await championship.pending();

        if (!series)
            return;

        const winnerA = strength.indexOf(series.a) < strength.indexOf(series.b);
        const scores = [2450 + (game * 137) % 600, 1980 + (game * 91) % 400];

        await championship.result(series.games[0], winnerA ? scores[0] : scores[1], winnerA ? scores[1] : scores[0]);
    }
}

async function finishedLeague(runner, words, players) {
    const championship = new Championship(runner);

    await championship.create(words.league, 'RoundRobin');
    await championship.settings({ Slug: slugs.league, Description: words.leagueDescription, startsOn: day(-120), endsOn: day(-60) }, [
        { name: 'Banner', draw: [1600, 500, 200, 4, '', 'image/jpeg'] },
        { name: 'Logo', draw: [512, 512, 220, 5, 'WL', 'image/png'] }
    ]);
    await championship.participants(players.map(([id]) => id));

    const teams = await championship.teams(words.teams, players.slice(0, words.teams.length).map(([id]) => id));

    for (let index = words.teams.length; index < players.length; index++)
        await championship.member(teams[index % teams.length], players[index][0]);

    const [stage] = await championship.stages();

    await championship.build(stage);
    await playRoundRobin(championship, words.teams);
    await championship.settings({ status: 'Finished' });
}

async function runningCup(runner, words, players, shot) {
    const championship = new Championship(runner);
    const captains = players.slice(0, words.teams.length).map(([id]) => id);

    await championship.create(words.cup, 'RoundRobinAndKnockout');
    await championship.settings({
        Slug: slugs.cup,
        Description: words.cupDescription,
        startsOn: day(-12),
        endsOn: day(16),
        GameConfiguration: 'ZoneMod v2.9.2b',
        DraftMethod: 'Snake',
        status: 'TeamFormation',
        SuggestedWeekday: 'Saturday',
        suggestedTime: '20:00',
        AntiCheatRequired: 'true'
    }, [
        { name: 'Banner', draw: [1600, 500, 15, 1, '', 'image/jpeg'] },
        { name: 'Logo', draw: [512, 512, 25, 2, 'AC', 'image/png'] }
    ]);
    await championship.tab('rules');
    await championship.post(`form[action="${championship.url}/rules"]`, { Rules: words.rules, RulesPortuguese: words.rules, RulesSpanish: words.rules });
    await championship.participants(players.map(([id]) => id));
    await championship.teams(words.teams, captains);
    await championship.tab('draft');
    await championship.post(`form[action="${championship.url}/draft/order"]`);

    for (let pick = 0; pick < players.length - captains.length; pick++) {
        if (pick === picksBeforeTheShot)
            await shot(championship.url);

        await championship.pick();
    }

    const [league, knockout] = await championship.stages();

    await championship.build(league);
    await playRoundRobin(championship, words.teams);
    await championship.stage(knockout, { BestOf: 3, FinalBestOf: 3, ThirdPlace: 'true', CampaignRule: 'Picks', Veto: 'true' });
    await championship.build(knockout);

    const [first, second] = await championship.pending();

    await championship.schedule(second.series, day(3, '20:00'));

    for (const score of [[2810, 2240], [2395, 2060]]) {
        const series = (await championship.pending()).find(item => item.series === first.series);

        await championship.result(series.games[0], ...score);
    }

    return { url: championship.url, captain: captains[words.teams.indexOf(second.a)], team: second.a, series: second.series };
}

async function openCup(runner, words, players) {
    const championship = new Championship(runner);

    await championship.create(words.open, 'Knockout');
    await championship.settings({ Slug: slugs.open, Description: words.openDescription, startsOn: day(45), MaxParticipants: 32, JoinRequests: 'true', status: 'Participants' }, [
        { name: 'Banner', draw: [1600, 500, 40, 7, '', 'image/jpeg'] },
        { name: 'Logo', draw: [512, 512, 45, 8, 'SC', 'image/png'] }
    ]);
    await championship.participants(players.slice(0, 10).map(([id]) => id));

    for (const [id] of players.slice(10, 13)) {
        await runner.signIn(id);
        await championship.open(`/championships/${slugs.open}`);
        await championship.post(`form[action="/championships/${slugs.open}/join"]`, { message: words.message });
    }

    await runner.signIn(runner.context.admin);

    return championship.url;
}

async function renameTeam(runner, captain, name) {
    const championship = new Championship(runner);

    await runner.signIn(captain);
    await championship.open(`/championships/${slugs.cup}/captain?tab=team`);
    await championship.post('form[action$="/captain/identity"]', { name, tag: 'ESP' });
    await runner.signIn(runner.context.admin);
}

async function fillChampionships(runner, shot) {
    const words = text[runner.context.lang];
    const players = runner.context.livePlayers.slice(0, words.teams.length * teamSize);

    await runner.signIn(runner.context.admin);
    await finishedLeague(runner, words, players);

    const cup = await runningCup(runner, words, players, shot);
    const open = await openCup(runner, words, players);

    await renameTeam(runner, cup.captain, `${cup.team} E-Sports`);

    return { cup: cup.url, open, captain: cup.captain, series: cup.series };
}

module.exports = { fillChampionships, slugs };
