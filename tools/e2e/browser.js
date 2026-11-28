const { Runner } = require('../screenshots/runner');
const { sleep } = require('../screenshots/cdp');

const keyCodes = { ArrowLeft: 37, ArrowUp: 38, ArrowRight: 39, ArrowDown: 40 };
const waitTimeout = 15000;
const pollInterval = 150;

class Browser extends Runner {
    constructor(page, context) {
        super(page, context);
        this.errors = [];
        this.failedRequests = [];
        this.requests = new Map();

        page.on('Runtime.exceptionThrown', params => {
            const details = params.exceptionDetails;

            this.errors.push(`${details.exception && details.exception.description ? details.exception.description.split('\n')[0] : details.text} (${details.url || 'inline'}:${details.lineNumber})`);
        });

        page.on('Network.requestWillBeSent', params => this.requests.set(params.requestId, params.request.url));
        page.on('Network.responseReceived', params => {
            if (params.response.status >= 500 && params.response.url.startsWith(context.base))
                this.failedRequests.push(`${params.response.status} ${params.response.url}`);
        });
        page.on('Network.loadingFailed', params => {
            const url = this.requests.get(params.requestId) || '';

            if (url.startsWith(context.base) && !params.canceled && params.errorText !== 'net::ERR_ABORTED')
                this.failedRequests.push(`${params.errorText} ${url}`);
        });
    }

    resetProblems() {
        this.errors = [];
        this.failedRequests = [];
    }

    async open(address, settle = 300) {
        await this.page.goto(this.url(address), settle);
    }

    eval(expression) {
        return this.page.evaluate(expression);
    }

    async until(expression, description, timeout = waitTimeout) {
        const started = Date.now();

        while (Date.now() - started < timeout) {
            if (await this.page.evaluate(`!!(${expression})`))
                return;

            await sleep(pollInterval);
        }

        throw new Error(`Timed out waiting for ${description || expression}`);
    }

    async click(selector, options = {}) {
        const found = await this.page.evaluate(`(() => { const element = document.querySelector(${JSON.stringify(selector)}); if (!element) return false; element.scrollIntoView({ block: 'center' }); const asks = element.matches('[data-confirm]') || (element.type === 'submit' && !!element.form && element.form.hasAttribute('data-confirm')); element.click(); return asks ? 'confirm' : true; })()`);

        if (!found)
            throw new Error(`Nothing to click at ${selector}`);

        if (found === 'confirm')
            await this.confirm();

        if (options.navigates) {
            await this.page.waitFor('Page.loadEventFired');
            await this.page.idle();
        }

        await sleep(options.settle === undefined ? 400 : options.settle);
    }

    async submit(selector) {
        const loaded = this.page.waitFor('Page.loadEventFired');

        const asks = await this.page.evaluate(`(() => { const form = document.querySelector(${JSON.stringify(selector)}).closest('form'); const button = form.querySelector('[type=submit]'); const asks = form.hasAttribute('data-confirm') || (!!button && button.hasAttribute('data-confirm')); if (button) button.click(); else form.requestSubmit ? form.requestSubmit() : form.submit(); return asks; })()`);

        if (asks)
            await this.confirm();

        await loaded;
        await this.page.idle();
        await sleep(300);
    }

    async confirm() {
        await this.until(`document.querySelector('#confirm-modal') && document.querySelector('#confirm-modal').classList.contains('in') && getComputedStyle(document.querySelector('#confirm-modal')).display === 'block'`, 'the confirmation window to open');
        await sleep(200);
        this.confirmations = (this.confirmations || 0) + 1;
        await this.page.evaluate(`document.querySelector('#confirm-modal [data-confirm-ok]').click()`);
    }

    async fill(values) {
        await this.page.evaluate(`(() => { const values = ${JSON.stringify(values)}; for (const [selector, value] of Object.entries(values)) { const element = document.querySelector(selector); if (!element) throw new Error('No field ' + selector); if (element.type === 'checkbox') element.checked = !!value; else element.value = value; element.dispatchEvent(new Event('input', { bubbles: true })); element.dispatchEvent(new Event('change', { bubbles: true })); } return true; })()`);
    }

    async press(key) {
        const code = keyCodes[key];
        const event = { key, code: key, windowsVirtualKeyCode: code, nativeVirtualKeyCode: code };

        await this.page.send('Input.dispatchKeyEvent', { type: 'rawKeyDown', ...event });
        await this.page.send('Input.dispatchKeyEvent', { type: 'keyUp', ...event });
    }

    async modalShown(selector) {
        await this.until(`document.querySelector(${JSON.stringify(selector)}) && document.querySelector(${JSON.stringify(selector)}).classList.contains('in') && getComputedStyle(document.querySelector(${JSON.stringify(selector)})).display === 'block'`, `the modal ${selector} to open`);
        await sleep(400);
    }

    async modalHidden(selector) {
        await this.until(`!document.querySelector(${JSON.stringify(selector)}).classList.contains('in') && !document.body.classList.contains('modal-open')`, `the modal ${selector} to close`);
        await sleep(200);
    }
}

module.exports = { Browser, sleep };
