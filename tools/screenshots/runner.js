const path = require('path');
const { sleep } = require('./cdp');

class Runner {
    constructor(page, context) {
        this.page = page;
        this.context = context;
        this.user = undefined;
        this.width = null;
        this.sizes = [];
    }

    url(address) {
        return /^https?:/.test(address) ? address : this.context.base + address;
    }

    async language() {
        await this.page.viewport(1280, 800);
        await this.page.goto(this.url(`/language?culture=${this.context.culture}&returnUrl=/`), 300);
    }

    async signIn(user) {
        if (user === this.user)
            return;

        await this.page.goto(this.url('/'), 200);

        if (await this.page.evaluate("!!document.getElementById('logout-form')"))
            await this.page.act("document.getElementById('logout-form').submit()", { navigates: true, settle: 200 });

        if (user)
            await this.page.goto(this.url(`/dev/sign-in/${user}?returnUrl=/`), 200);

        this.user = user;
    }

    async theme(name) {
        await this.page.goto(this.url(`/theme/${name}?returnUrl=/`), 200);
    }

    async step(action) {
        if (typeof action === 'string')
            return this.page.act(action);

        if (action.go)
            return this.page.goto(this.url(action.go), action.settle);

        if (action.wait)
            return this.page.waitForSelector(action.wait);

        if (action.sleep)
            return sleep(action.sleep);

        if (action.dialog !== undefined) {
            this.page.dialogAnswers.push(action.dialog);
            return;
        }

        if (action.run)
            return action.run(this);

        if (action.eval)
            return this.page.act(action.eval, action);
    }

    async shot(shot) {
        if (this.context.only && !this.context.only.includes(shot.name) && !shot.always)
            return;

        const width = shot.width || 1280;
        const height = shot.height || (width < 700 ? 844 : 800);

        await this.signIn(shot.user === undefined ? this.context.admin : shot.user);

        if (shot.theme)
            await this.theme(shot.theme);

        await this.page.viewport(width, height);

        if (shot.path)
            await this.page.goto(this.url(shot.path), shot.settle);

        for (const action of shot.before || [])
            await this.step(action);

        if (shot.name) {
            const file = path.join(this.context.output, shot.name + '.webp');
            const info = await this.page.capture(file, shot);

            this.sizes.push(info.bytes);
            console.log(`${file} ${info.width}x${info.height} ${Math.round(info.bytes / 1024)} KB`);
        }

        for (const action of shot.after || [])
            await this.step(action);

        if (shot.theme)
            await this.theme('default');
    }

    async run(shots) {
        await this.language();

        for (const shot of shots)
            await this.shot(shot);

        const total = this.sizes.reduce((sum, bytes) => sum + bytes, 0);

        console.log(`${this.sizes.length} images, ${(total / 1024 / 1024).toFixed(2)} MB`);
    }
}

module.exports = { Runner };
