const fs = require('fs');
const path = require('path');
const { Runner } = require('./runner');

const community = {
    name: 'L4D2 Competitive',
    culture: 'pt-BR',
    timeZone: 'America/Sao_Paulo',
    seasonMonths: 2,
    server: 'l4d2.com.br | Competitive',
    address: '124.198.128.93:27015',
    alternativeAddress: 'server.l4d2.com.br',
    keywords: 'L4D2 Competitive, Left 4 Dead 2, estatísticas de jogadores, ranking, servidor L4D2, desempenho em jogos, análise de jogadores, melhores jogadores, gaming, leaderboard',
    googleAnalytics: 'G-4QMNBJLQST',
    footer: ['Server repository | https://github.com/altair-sossai/l4d2-zone-server', 'GitHub | https://github.com/altair-sossai/l4d2-playstats-hub', 'Steam | https://steamcommunity.com/id/altairsossai'],
    antiCheatService: 'https://aac.l4d2.com.br',
    antiCheatDownload: 'https://l4d2playstats.blob.core.windows.net/anti-cheat/L4D2AntiCheat.exe',
    modules: { Ranking: true, LiveServer: true, SiteChat: true, RemoteCommands: true, SkinShop: true, CommunityRanking: false, AntiCheat: true },
    rules: [
        { Name: 'ZoneMod', ConfigurationPattern: 'ZoneMod v*', Priority: 10 },
        { Name: 'NeoMod Tourney', ConfigurationPattern: 'NeoMod Tourney *', Priority: 20 },
        { Name: 'VanillaMod', ConfigurationPattern: 'VanillaMod *', Priority: 30 }
    ],
    otherRule: { 'en-US': 'Other configurations', 'pt-BR': 'Outras configurações', 'es-ES': 'Otras configuraciones' }
};

const json = value => JSON.stringify(value);
const hideDevelopmentSignIn = `(() => { const field = document.querySelector('#developmentCommunityId'); if (field) field.closest('.form-group').style.display = 'none'; })()`;
const submit = selector => ({ eval: `document.querySelector(${json(selector)}).closest('form').querySelector('[type=submit]').click()`, navigates: true });
const fill = values => Object.entries(values).map(([selector, value]) => `document.querySelector(${json(selector)}).value = ${json(String(value))}`).join('; ');
const newRuleForm = `[...document.querySelectorAll('form[action="/admin/game-modes"]')].find(form => !form.querySelector('[name=Id]'))`;
const hideKey = `document.querySelectorAll('pre.server-config').forEach(pre => pre.innerHTML = pre.innerHTML.replace(/(playstats_token "[^:"]+:)[^"]+/, '$1YOUR-KEY'))`;
const showAddress = address => `document.querySelectorAll('pre.server-config').forEach(pre => pre.innerHTML = pre.innerHTML.split(location.origin).join(${json(address)}))`;
const ruleField = (form, name) => `document.querySelector('[form="' + ${form}.id + '"][name=${name}]')`;

async function upload(runner, selector, file) {
    const { root } = await runner.page.send('DOM.getDocument', { depth: -1 });
    const { nodeId } = await runner.page.send('DOM.querySelector', { nodeId: root.nodeId, selector });

    await runner.page.send('DOM.setFileInputFiles', { nodeId, files: [file] });
}

async function readToken(runner) {
    const text = await runner.page.evaluate(`document.querySelector('pre.server-config').innerText`);
    const token = text.match(/playstats_token "([^"]+)"/)[1];

    if (runner.context.tokenFile)
        fs.writeFileSync(runner.context.tokenFile, token, { mode: 0o600 });
}

function addRule(rule) {
    const values = { Name: rule.Name, ConfigurationPattern: rule.ConfigurationPattern, TeamSize: 4, MinimumMaps: 2, Priority: rule.Priority, CountsForRanking: 'true' };
    const assignments = Object.entries(values).map(([name, value]) => `${ruleField('form', name)}.value = ${json(String(value))}`).join('; ');

    return { eval: `(() => { const form = ${newRuleForm}; ${assignments}; form.querySelector('[type=submit]').click(); })()`, navigates: true };
}

function otherRule(name) {
    const form = `[...document.querySelectorAll('form[action="/admin/game-modes"]')].find(form => form.querySelector('[name=Id]') && document.querySelector('[form="' + form.id + '"][name=ConfigurationPattern]').value === '*')`;

    return { eval: `(() => { const form = ${form}; ${ruleField('form', 'Name')}.value = ${json(name)}; ${ruleField('form', 'CountsForRanking')}.value = 'false'; ${ruleField('form', 'Priority')}.value = '100'; form.querySelector('[type=submit]').click(); })()`, navigates: true };
}

function shots(context) {
    const steamKey = context.steamKeyFile ? fs.readFileSync(context.steamKeyFile, 'utf8').trim() : '';
    const modules = Object.entries(community.modules).map(([name, on]) => `document.querySelector('#module-${name}').checked = ${on}`).join('; ');
    const administrators = (context.admins || '').split(',').filter(Boolean);
    const logo = path.resolve(context.logo);

    return [
        { name: 'switch-setup-code', user: null, path: '/setup', before: [`document.querySelector('#token').value = ${json(context.setupCode)}`, hideDevelopmentSignIn], full: true, always: true },
        {
            name: 'switch-site',
            user: null,
            before: [
                `document.querySelector('#developmentCommunityId').value = ${json(context.admin)}`,
                submit('#token'),
                { wait: '#Site_Name' },
                fill({ '#Site_Name': community.name, '#Site_DefaultCulture': community.culture, '#Site_TimeZone': community.timeZone, '#Season_LengthInMonths': community.seasonMonths }),
                { run: runner => upload(runner, '#Logo', logo) }
            ],
            full: true,
            always: true
        },
        {
            name: 'switch-modules',
            user: null,
            before: [fill({ '#Site_SteamApiKey': steamKey }), submit('#Site_Name'), { wait: '#module-AntiCheat' }, modules],
            full: true,
            always: true
        },
        {
            name: 'switch-server',
            user: null,
            before: [submit('form[action="/setup/modules"] [type=submit]'), { wait: '#Name' }, fill({ '#Name': community.server, '#Address': community.address })],
            full: true,
            always: true
        },
        {
            name: 'switch-server-token',
            user: null,
            before: [submit('#Name'), { wait: 'pre.server-config' }, { run: readToken }, showAddress(context.siteAddress), hideKey],
            settle: 0,
            full: true,
            always: true
        },
        {
            name: 'switch-done',
            user: null,
            before: [{ eval: `location.href = '/setup?step=finish'`, navigates: true }, submit('form[action="/setup/finish"] [type=submit]'), { sleep: 800 }],
            full: true,
            maxHeight: 800,
            always: true
        },
        {
            name: 'switch-settings',
            user: null,
            before: [
                { go: '/admin/settings' },
                fill({ '#Site_FooterLinks': community.footer.join('\n'), '#Site_MetaKeywords': community.keywords, '#Site_GoogleAnalyticsId': community.googleAnalytics }),
                submit('#Site_Name'),
                { wait: '#Site_Name' }
            ],
            full: true,
            always: true
        },
        {
            user: null,
            before: administrators.flatMap(id => [{ go: '/admin/administrators' }, fill({ '#communityId': id }), submit('#communityId')]),
            always: true
        },
        {
            name: 'switch-server-page',
            user: null,
            before: [
                { go: '/admin/servers' },
                { eval: `location.href = document.querySelector('a[href^="/admin/servers/"]').getAttribute('href')`, navigates: true },
                fill({ '#AlternativeAddress': community.alternativeAddress }),
                submit('#AlternativeAddress'),
                { wait: '#AlternativeAddress' },
                showAddress(context.siteAddress)
            ],
            full: true,
            always: true
        },
        {
            name: 'switch-game-modes',
            user: null,
            before: [{ go: '/admin/game-modes' }, ...community.rules.map(addRule), otherRule(community.otherRule[context.culture])],
            full: true,
            always: true
        },
        {
            name: 'switch-anti-cheat-settings',
            user: null,
            before: [{ go: '/admin/anti-cheat' }, fill({ '#serviceUrl': community.antiCheatService, '#downloadUrl': community.antiCheatDownload }), submit('#serviceUrl'), { wait: '#serviceUrl' }],
            full: true,
            always: true
        }
    ];
}

function overview() {
    return [{ name: 'switch-overview', path: '/admin', full: true, always: true }];
}

async function captureSwitch(page, context) {
    const runner = new Runner(page, context);

    if (context.step === 'overview') {
        await runner.run(overview());
        return;
    }

    if (!context.setupCode || !context.logo)
        throw new Error('Pass the setup code with --setup-code and the logo with --logo.');

    context.siteAddress = context.siteAddress || context.base;

    runner.user = null;
    await runner.run(shots(context));
}

module.exports = { captureSwitch };
