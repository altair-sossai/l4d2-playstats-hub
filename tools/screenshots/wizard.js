const { Runner } = require('./runner');
const live = require('./live');

const hideDevelopmentSignIn = `(() => { const field = document.querySelector('#developmentCommunityId'); if (field) field.closest('.form-group').style.display = 'none'; })()`;
const submit = selector => ({ eval: `document.querySelector(${JSON.stringify(selector)}).closest('form').querySelector('[type=submit]').click()`, navigates: true });

function shots(context) {
    return [
        { name: 'wizard-setup-code', user: null, path: '/setup', before: [hideDevelopmentSignIn], full: true, always: true },
        { name: 'wizard-setup-code-filled', user: null, before: [`document.querySelector('#token').value = ${JSON.stringify(context.setupCode)}`, hideDevelopmentSignIn], full: true, always: true },
        {
            name: 'wizard-site',
            user: null,
            before: [`document.querySelector('#developmentCommunityId').value = ${JSON.stringify(context.admin)}`, submit('#token'), { wait: '#Site_Name' }],
            full: true,
            always: true
        },
        {
            name: 'wizard-modules',
            user: null,
            before: [`document.querySelector('#Site_Name').value = 'L4D2 Competitive'; document.querySelector('#Site_DefaultCulture').value = ${JSON.stringify(context.culture)}`, submit('#Site_Name'), { sleep: 500 }],
            full: true,
            always: true
        },
        { name: 'wizard-server', user: null, before: [submit('form[action="/setup/modules"] [type=submit]'), { wait: '#Name' }], full: true, always: true },
        {
            name: 'wizard-server-token',
            user: null,
            before: [`document.querySelector('#Name').value = 'L4D2 Competitive #1'; document.querySelector('#Address').value = '203.0.113.10:27015'`, submit('#Name'), { wait: 'pre.server-config' }],
            full: true,
            always: true
        },
        {
            name: 'wizard-server-connected',
            user: null,
            before: [
                {
                    run: async runner => {
                        const text = await runner.page.evaluate(`document.querySelector('pre.server-config').innerText`);
                        const token = text.match(/playstats_token "([^"]+)"/)[1];

                        await live.heartbeat(runner.context.base, token, 0);
                    }
                },
                { wait: '.status-dot.status-Online' },
                { sleep: 800 }
            ],
            full: true,
            always: true
        },
        { name: 'wizard-finish', user: null, before: [{ eval: `location.href = '/setup?step=finish'`, navigates: true }], full: true, always: true },
        { name: 'wizard-done', user: null, before: [submit('form[action="/setup/finish"] [type=submit]'), { sleep: 800 }], full: true, maxHeight: 800, always: true }
    ];
}

async function captureWizard(page, context) {
    if (!context.setupCode)
        throw new Error('Pass the setup code with --setup-code.');

    const runner = new Runner(page, context);

    runner.user = null;
    await runner.run(shots(context));
}

module.exports = { captureWizard };
