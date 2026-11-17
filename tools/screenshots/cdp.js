const { spawn } = require('child_process');
const fs = require('fs');
const os = require('os');
const path = require('path');

const browserCandidates = [
    'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
    'C:/Program Files/Microsoft/Edge/Application/msedge.exe',
    'C:/Program Files/Google/Chrome/Application/chrome.exe',
    '/usr/bin/microsoft-edge',
    '/usr/bin/google-chrome',
    '/usr/bin/chromium',
    '/usr/bin/chromium-browser',
    '/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge',
    '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'
];

const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));

function findBrowser(explicit) {
    const found = [explicit, process.env.BROWSER, ...browserCandidates].find(candidate => candidate && fs.existsSync(candidate));

    if (!found)
        throw new Error('No Edge or Chrome found. Pass its path with --browser.');

    return found;
}

async function launch(options = {}) {
    const executable = findBrowser(options.browser);
    const port = 9400 + Math.floor(Math.random() * 500);
    const profile = fs.mkdtempSync(path.join(os.tmpdir(), 'playstats-shots-'));
    const process_ = spawn(executable, ['--headless=new', '--disable-gpu', '--hide-scrollbars', '--no-first-run', '--no-default-browser-check', '--force-color-profile=srgb', `--remote-debugging-port=${port}`, `--user-data-dir=${profile}`, 'about:blank'], { stdio: 'ignore' });

    let target = null;

    for (let attempt = 0; attempt < 80 && !target; attempt++) {
        await sleep(250);

        try {
            const list = await (await fetch(`http://127.0.0.1:${port}/json`)).json();

            target = list.find(item => item.type === 'page');
        } catch {
        }
    }

    if (!target)
        throw new Error('The browser did not start.');

    const socket = new WebSocket(target.webSocketDebuggerUrl);

    await new Promise((resolve, reject) => {
        socket.addEventListener('open', resolve);
        socket.addEventListener('error', reject);
    });

    const page = new Page(socket);

    await page.send('Page.enable');
    await page.send('Runtime.enable');
    await page.send('Network.enable');

    page.on('Page.javascriptDialogOpening', message => {
        const answer = page.dialogAnswers.length > 0 ? page.dialogAnswers.shift() : null;

        page.send('Page.handleJavaScriptDialog', answer === false ? { accept: false } : { accept: true, promptText: answer === null || answer === true ? '' : String(answer) });
    });

    page.close = async () => {
        const exited = new Promise(resolve => {
            process_.once('exit', resolve);
            setTimeout(resolve, 10000);
        });

        try {
            await page.send('Browser.close');
        } catch {
        }

        await exited;
        socket.close();
        process_.kill();
        await sleep(1000);

        try {
            fs.rmSync(profile, { recursive: true, force: true });
        } catch {
        }
    };

    return page;
}

class Page {
    constructor(socket) {
        this.socket = socket;
        this.sequence = 0;
        this.pending = new Map();
        this.listeners = [];
        this.dialogAnswers = [];
        this.width = 1280;
        this.height = 800;

        socket.addEventListener('message', event => {
            const message = JSON.parse(event.data);

            if (message.id && this.pending.has(message.id)) {
                const { resolve, reject } = this.pending.get(message.id);

                this.pending.delete(message.id);

                if (message.error)
                    reject(new Error(message.error.message));
                else
                    resolve(message.result);
            } else if (message.method) {
                this.listeners.filter(listener => listener.method === message.method).forEach(listener => listener.callback(message.params));
            }
        });
    }

    send(method, params = {}) {
        return new Promise((resolve, reject) => {
            const id = ++this.sequence;

            this.pending.set(id, { resolve, reject });
            this.socket.send(JSON.stringify({ id, method, params }));
        });
    }

    on(method, callback) {
        const listener = { method, callback };

        this.listeners.push(listener);

        return () => this.listeners.splice(this.listeners.indexOf(listener), 1);
    }

    waitFor(method, timeout = 20000) {
        return new Promise(resolve => {
            const timer = setTimeout(() => {
                off();
                resolve(null);
            }, timeout);
            const off = this.on(method, params => {
                clearTimeout(timer);
                off();
                resolve(params);
            });
        });
    }

    async viewport(width, height) {
        this.width = width;
        this.height = height;

        await this.send('Emulation.setDeviceMetricsOverride', { width, height, deviceScaleFactor: 1, mobile: width < 700 });
        await this.send('Emulation.setTouchEmulationEnabled', { enabled: width < 700 });
    }

    async colorScheme(dark) {
        await this.send('Emulation.setEmulatedMedia', { features: [{ name: 'prefers-color-scheme', value: dark ? 'dark' : 'light' }] });
    }

    async goto(url, settle = 1200) {
        const loaded = this.waitFor('Page.loadEventFired');

        await this.send('Page.navigate', { url });
        await loaded;
        await this.idle();
        await sleep(settle);
    }

    async idle() {
        await this.evaluate('document.fonts ? document.fonts.ready.then(() => true) : true');
        await this.evaluate(`Promise.all(Array.from(document.images).filter(image => !image.complete).map(image => new Promise(resolve => { image.onload = image.onerror = resolve; setTimeout(resolve, 8000); }))).then(() => true)`);
    }

    async evaluate(expression) {
        const result = await this.send('Runtime.evaluate', { expression, awaitPromise: true, returnByValue: true });

        if (result.exceptionDetails)
            throw new Error(`Script failed: ${result.exceptionDetails.exception ? result.exceptionDetails.exception.description : result.exceptionDetails.text}\n${expression}`);

        return result.result ? result.result.value : undefined;
    }

    async act(expression, options = {}) {
        const loaded = options.navigates ? this.waitFor('Page.loadEventFired') : null;

        await this.evaluate(expression);

        if (loaded) {
            await loaded;
            await this.idle();
        }

        await sleep(options.settle === undefined ? 900 : options.settle);
    }

    async waitForSelector(selector, timeout = 20000) {
        const started = Date.now();

        while (Date.now() - started < timeout) {
            if (await this.evaluate(`!!document.querySelector(${JSON.stringify(selector)})`))
                return true;

            await sleep(250);
        }

        return false;
    }

    async box(selector, padding, nth) {
        return this.evaluate(`(() => {
            const visible = Array.from(document.querySelectorAll(${JSON.stringify(selector)})).filter(element => element.getClientRects().length > 0);
            const elements = ${nth === undefined ? 'visible' : `visible.slice(${nth}, ${nth + 1})`};
            if (elements.length === 0) return null;
            const rects = elements.map(element => element.getBoundingClientRect());
            const left = Math.min(...rects.map(rect => rect.left)) + window.scrollX;
            const top = Math.min(...rects.map(rect => rect.top)) + window.scrollY;
            const right = Math.max(...rects.map(rect => rect.right)) + window.scrollX;
            const bottom = Math.max(...rects.map(rect => rect.bottom)) + window.scrollY;
            const pad = ${padding};
            const x = Math.max(0, Math.floor(left - pad));
            const y = Math.max(0, Math.floor(top - pad));
            return { x, y, width: Math.min(document.documentElement.scrollWidth, Math.ceil(right + pad)) - x, height: Math.ceil(bottom + pad) - y };
        })()`);
    }

    async capture(file, options = {}) {
        const metrics = await this.send('Page.getLayoutMetrics');
        const contentHeight = Math.ceil(metrics.cssContentSize.height);
        let clip;

        if (options.clip) {
            clip = await this.box(options.clip, options.padding === undefined ? 10 : options.padding, options.nth);

            if (!clip)
                throw new Error(`Nothing matches ${options.clip}`);
        } else if (options.full) {
            const used = await this.evaluate(`Math.ceil(Math.max(0, ...Array.from(document.body.children).filter(element => getComputedStyle(element).position !== 'fixed' && element.getClientRects().length > 0).map(element => element.getBoundingClientRect().bottom + window.scrollY)))`);

            clip = { x: 0, y: 0, width: this.width, height: Math.min(contentHeight, used > 0 ? used + 16 : contentHeight) };
        } else {
            const scrollY = await this.evaluate('window.scrollY');

            clip = { x: 0, y: scrollY, width: this.width, height: this.height };
        }

        if (options.from) {
            const start = await this.box(options.from, options.padding === undefined ? 10 : options.padding);

            if (start)
                options = { ...options, top: start.y };
        }

        if (options.top !== undefined) {
            clip.height = clip.y + clip.height - options.top;
            clip.y = options.top;
        }

        if (options.maxHeight && clip.height > options.maxHeight)
            clip.height = options.maxHeight;

        const shot = await this.send('Page.captureScreenshot', {
            format: 'webp',
            quality: options.quality || 80,
            captureBeyondViewport: true,
            clip: { ...clip, scale: options.scale || 1 }
        });

        fs.mkdirSync(path.dirname(file), { recursive: true });
        fs.writeFileSync(file, Buffer.from(shot.data, 'base64'));

        return { width: Math.round(clip.width), height: Math.round(clip.height), bytes: fs.statSync(file).size };
    }
}

module.exports = { launch, sleep };
