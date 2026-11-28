const fs = require('fs');
const path = require('path');
const { launch } = require('../screenshots/cdp');
const { Browser, sleep } = require('./browser');
const host = require('./host');
const { seed } = require('./seed');
const { checks } = require('./checks');
const fixtures = require('./fixtures');

const defaultPort = 5199;
const defaultOutput = path.join(__dirname, '..', '..', 'e2e-output');
const checkTimeout = 120000;
const seedTimeout = 300000;
const closeTimeout = 15000;
const flags = ['keep', 'serve'];

function parse(argv) {
    const options = { port: defaultPort, out: defaultOutput };

    for (let index = 0; index < argv.length; index++) {
        const name = argv[index].replace(/^--/, '').replace(/-([a-z])/g, (_, letter) => letter.toUpperCase());

        if (flags.includes(name))
            options[name] = true;
        else
            options[name] = argv[++index];
    }

    options.out = path.resolve(options.out);
    options.only = options.only ? options.only.split(',') : null;

    return options;
}

function usage() {
    console.log(`Usage:
  node tools/e2e/run.js --site <published site folder> [--port ${defaultPort}] [--out e2e-output] [--only check,...] [--browser <path>] [--data <empty folder>] [--keep] [--serve] [--shots <folder>]`);
}

function withTimeout(promise, milliseconds, description) {
    let timer;

    return Promise.race([
        promise.finally(() => clearTimeout(timer)),
        new Promise((_, reject) => timer = setTimeout(() => reject(new Error(`${description} did not finish in ${milliseconds / 1000} seconds`)), milliseconds))
    ]);
}

async function evidence(browser, options, name) {
    const saved = {};

    fs.mkdirSync(options.out, { recursive: true });

    try {
        const shot = await withTimeout(browser.page.send('Page.captureScreenshot', { format: 'png' }), closeTimeout, 'The screenshot');

        saved.screenshot = path.join(options.out, `${name}.png`);
        fs.writeFileSync(saved.screenshot, Buffer.from(shot.data, 'base64'));
    } catch {
    }

    try {
        saved.html = path.join(options.out, `${name}.html`);
        fs.writeFileSync(saved.html, await withTimeout(browser.eval('document.documentElement.outerHTML'), closeTimeout, 'Reading the page'));
        saved.url = await withTimeout(browser.eval('location.href'), closeTimeout, 'Reading the address');
    } catch {
        delete saved.html;
    }

    return saved;
}

async function runCheck(browser, check, options) {
    const started = Date.now();

    browser.resetProblems();

    try {
        await withTimeout(check.run(browser), check.timeout || checkTimeout, `The check ${check.name}`);
        await sleep(200);

        const problems = [...browser.errors.map(error => `JavaScript error: ${error}`), ...browser.failedRequests.map(request => `Request failed: ${request}`)];

        if (problems.length > 0)
            throw new Error(problems.join('\n'));

        return { name: check.name, ok: true, seconds: (Date.now() - started) / 1000 };
    } catch (error) {
        return { name: check.name, ok: false, seconds: (Date.now() - started) / 1000, error: error.message, ...await evidence(browser, options, check.name) };
    }
}

async function close(page) {
    try {
        await withTimeout(page.close(), closeTimeout, 'Closing the browser');
    } catch {
    }
}

async function openBrowser(options, base) {
    const page = await withTimeout(launch({ browser: options.browser }), 60000, 'Starting the browser');

    return new Browser(page, { base, culture: 'en-US', admin: fixtures.admin });
}

function report(result) {
    console.log(`${result.ok ? 'PASS' : 'FAIL'} ${result.name} (${result.seconds.toFixed(1)}s)`);

    if (!result.ok) {
        const lines = [...result.error.split('\n'), result.url ? `at ${result.url}` : null, result.screenshot ? `screenshot: ${result.screenshot}` : null].filter(Boolean);

        console.log(lines.map(line => `     ${line}`).join('\n'));
    }
}

async function main() {
    const options = parse(process.argv.slice(2));

    if (!options.site) {
        usage();
        process.exit(2);
    }

    const started = Date.now();
    const results = [];

    fs.rmSync(options.out, { recursive: true, force: true });

    const site = await host.start(options);
    let browser = null;

    try {
        console.log(`Site at ${site.base}, data in ${site.data}`);
        browser = await openBrowser(options, site.base);

        let seeded;

        try {
            await browser.language();
            seeded = await withTimeout(seed(browser, site), seedTimeout, 'Seeding the site');
        } catch (error) {
            const saved = await evidence(browser, options, 'seed');

            results.push({ name: 'seed', ok: false, seconds: (Date.now() - started) / 1000, error: error.message, ...saved });
            report(results[0]);
            return;
        }

        console.log(`Seeded with ${seeded.files} statistics files in ${((Date.now() - started) / 1000).toFixed(0)}s.`);

        if (options.serve) {
            console.log(`Serving ${site.base} until Ctrl+C. Administrator: ${fixtures.admin}`);
            await new Promise(() => {});
        }

        const context = { token: seeded.token, base: site.base, out: options.shots ? path.resolve(options.shots) : null };

        for (const check of checks(context)) {
            if (options.only && !options.only.includes(check.name))
                continue;

            const result = await runCheck(browser, check, options);

            results.push(result);
            report(result);

            if (!result.ok && /did not finish/.test(result.error)) {
                await close(browser.page);
                browser = await openBrowser(options, site.base);
            }
        }
    } finally {
        if (browser)
            await close(browser.page);

        await site.stop();

        const failed = results.filter(result => !result.ok).length;

        console.log(`\n${results.length - failed} passed, ${failed} failed in ${((Date.now() - started) / 1000).toFixed(0)}s.`);

        if (failed > 0) {
            fs.mkdirSync(options.out, { recursive: true });
            fs.writeFileSync(path.join(options.out, 'site.log'), site.log.join('\n'));
            fs.writeFileSync(path.join(options.out, 'report.json'), JSON.stringify(results, null, 2));
            console.log(`Screenshots, pages and the site log are in ${options.out}`);
        }

        process.exitCode = failed > 0 || results.length === 0 ? 1 : 0;
    }
}

main().then(() => process.exit(process.exitCode)).catch(error => {
    console.error(error.stack || error.message);
    process.exit(1);
});
