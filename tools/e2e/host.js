const { spawn } = require('child_process');
const fs = require('fs');
const os = require('os');
const path = require('path');
const { sleep } = require('../screenshots/cdp');

const assembly = 'L4D2PlayStats.Hub.Web.dll';
const setupCodeFile = 'setup-code.txt';
const startTimeout = 90000;

function resolveAssembly(site) {
    const candidate = fs.existsSync(site) && fs.statSync(site).isDirectory() ? path.join(site, assembly) : site;

    if (!fs.existsSync(candidate))
        throw new Error(`The published site was not found at ${candidate}. Run dotnet publish src/L4D2PlayStats.Hub.Web -c Release -o <folder> and pass the folder with --site.`);

    return path.resolve(candidate);
}

async function start(options) {
    const dll = resolveAssembly(options.site);
    const data = options.data ? path.resolve(options.data) : fs.mkdtempSync(path.join(os.tmpdir(), 'playstats-e2e-'));
    const base = `http://127.0.0.1:${options.port}`;
    const log = [];

    fs.mkdirSync(data, { recursive: true });

    if (fs.readdirSync(data).length > 0)
        throw new Error(`The data folder ${data} must be empty, since the tests run the setup wizard.`);

    const child = spawn('dotnet', [dll, '--urls', base], {
        cwd: path.dirname(dll),
        env: {
            ...process.env,
            ASPNETCORE_ENVIRONMENT: 'Development',
            Development__AllowTestSignIn: 'true',
            Hub__DataDirectory: data,
            DOTNET_NOLOGO: '1'
        },
        stdio: ['ignore', 'pipe', 'pipe']
    });

    const remember = chunk => log.push(...chunk.toString().split(/\r?\n/).filter(Boolean));

    child.stdout.on('data', remember);
    child.stderr.on('data', remember);

    let exited = null;

    child.once('exit', code => exited = code);
    process.once('exit', () => {
        if (exited === null)
            child.kill();
    });
    process.once('SIGINT', () => process.exit(130));

    const started = Date.now();
    const codeFile = path.join(data, setupCodeFile);

    while (Date.now() - started < startTimeout) {
        if (exited !== null)
            throw new Error(`The site stopped with code ${exited}:\n${log.slice(-40).join('\n')}`);

        if (fs.existsSync(codeFile) && fs.readFileSync(codeFile, 'utf8').trim()) {
            try {
                const response = await fetch(`${base}/setup`);

                if (response.ok)
                    break;
            } catch {
            }
        }

        await sleep(500);
    }

    if (!fs.existsSync(codeFile))
        throw new Error(`The site did not start in ${startTimeout / 1000} seconds:\n${log.slice(-40).join('\n')}`);

    return {
        base,
        data,
        log,
        setupCode: fs.readFileSync(codeFile, 'utf8').trim(),
        stop: async () => {
            if (exited === null) {
                const gone = new Promise(resolve => child.once('exit', resolve));

                child.kill();
                await Promise.race([gone, sleep(10000)]);
            }

            if (!options.keep && !options.data) {
                try {
                    fs.rmSync(data, { recursive: true, force: true });
                } catch {
                }
            }
        }
    };
}

module.exports = { start };
