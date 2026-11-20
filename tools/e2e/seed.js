const live = require('../screenshots/live');
const { generate } = require('./statistics');
const fixtures = require('./fixtures');

const liveDelayInSeconds = 0;

async function wizard(browser, setupCode) {
    browser.user = null;
    await browser.open('/setup');
    await browser.fill({ '#token': setupCode, '#developmentCommunityId': fixtures.admin });
    await browser.submit('#token');
    await browser.until(`document.querySelector('#Site_Name')`, 'the site step of the wizard');
    await browser.fill({ '#Site_Name': fixtures.siteName, '#Site_TimeZone': 'UTC', '#Site_DefaultCulture': 'en-US' });
    await browser.submit('#Site_Name');
    await browser.until(`document.querySelector('form[action="/setup/modules"]')`, 'the modules step of the wizard');
    await browser.eval(`document.querySelectorAll('form[action="/setup/modules"] input[type=checkbox]').forEach(box => box.checked = true)`);
    await browser.submit('form[action="/setup/modules"] [type=submit]');
    await browser.until(`document.querySelector('#Name')`, 'the server step of the wizard');
    await browser.fill({ '#Name': fixtures.serverName, '#Address': fixtures.serverAddress });
    await browser.submit('#Name');
    await browser.until(`document.querySelector('pre.server-config')`, 'the instructions of the new server');

    const config = await browser.eval(`document.querySelector('pre.server-config').innerText`);
    const token = config.match(/playstats_token "([^"]+)"/)[1];

    await browser.open('/setup?step=finish');
    await browser.submit('form[action="/setup/finish"] [type=submit]');
    browser.user = fixtures.admin;

    return token;
}

async function upload(base, token) {
    const files = generate(fixtures.players, fixtures.matches, new Date(), fixtures.configuration);

    for (const file of files) {
        const response = await fetch(`${base}/api/statistics`, {
            method: 'POST',
            headers: { Authorization: token, 'Content-Type': 'application/json' },
            body: JSON.stringify(file)
        });

        if (!response.ok)
            throw new Error(`Uploading ${file.fileName} answered ${response.status}: ${await response.text()}`);
    }

    return files.length;
}

async function settings(browser) {
    await browser.signIn(fixtures.admin);
    await browser.open('/admin/community');
    await browser.fill(Object.fromEntries(Object.entries(fixtures.community).map(([name, value]) => [`#${name}`, value])));
    await browser.submit('#MinimumMatchesToVote');
    await browser.open('/admin/servers');
    await browser.fill({ '#DelayInSeconds': liveDelayInSeconds });
    await browser.submit('#DelayInSeconds');
}

async function vote(browser, outcome) {
    await browser.eval(`document.getElementById('community-ballot').dataset.old = '1'`);
    await browser.click(`.ballot button[value="${outcome}"]`, { settle: 0 });
    await browser.until(`document.getElementById('community-ballot') && !document.getElementById('community-ballot').dataset.old && !document.querySelector('.ballot-busy')`, 'the next ballot');
}

async function votes(browser) {
    const outcomes = ['Left', 'Right', 'SameLevel'];

    for (const voter of fixtures.seedVoters) {
        await browser.signIn(voter);
        await browser.open('/community/vote');

        for (let index = 0; index < fixtures.seedVotesPerVoter; index++) {
            if (!await browser.eval(`!!document.querySelector('.ballot')`))
                break;

            await vote(browser, outcomes[(index + fixtures.seedVoters.indexOf(voter)) % outcomes.length]);
        }
    }
}

function step(name) {
    console.log(`Seed: ${name}`);
}

async function seed(browser, host) {
    step('setup wizard');
    const token = await wizard(browser, host.setupCode);

    step('statistics files');
    const files = await upload(host.base, token);

    step('community and live settings');
    await settings(browser);
    step('live data');
    await live.feed(host.base, token, 0, true, fixtures.players.map(player => [player.id, player.name]));
    step('community votes');
    await votes(browser);

    return { token, files };
}

module.exports = { seed };
