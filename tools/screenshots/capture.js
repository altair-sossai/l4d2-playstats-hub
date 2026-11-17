const fs = require('fs');
const path = require('path');
const { launch } = require('./cdp');
const { captureTerminals } = require('./terminal');
const { captureSite } = require('./site');
const { captureWizard } = require('./wizard');

const cultures = { en: 'en-US', pt: 'pt-BR', es: 'es-ES' };

function parse(argv) {
    const options = { command: argv[0], tokens: [] };

    for (let index = 1; index < argv.length; index++) {
        const name = argv[index].replace(/^--/, '');
        const value = argv[index + 1];

        if (name === 'token')
            options.tokens.push(value);
        else
            options[name.replace(/-([a-z])/g, (_, letter) => letter.toUpperCase())] = value;

        index++;
    }

    options.out = path.resolve(options.out || path.join(__dirname, '..', '..', 'docs', 'images'));
    options.only = options.only ? options.only.split(',') : null;

    return options;
}

function usage() {
    console.log(`Usage:
  node capture.js terminal [--langs en,pt,es] [--out docs/images] [--only name,...]
  node capture.js site     --base http://localhost:5103 --lang en --admin <SteamID64> --token <id:key> [--token <id:key>] [--player <SteamID64>] [--out docs/images] [--only name,...]
  node capture.js wizard   --base http://localhost:5103 --lang en --admin <SteamID64> --setup-code <code> [--out docs/images] [--only name,...]

Common options: --browser <path to Edge or Chrome>.`);
}

async function main() {
    const options = parse(process.argv.slice(2));

    if (!['terminal', 'site', 'wizard'].includes(options.command)) {
        usage();
        process.exit(1);
    }

    const page = await launch({ browser: options.browser });

    try {
        if (options.command === 'terminal') {
            const outputs = (options.langs || 'en,pt,es').split(',').map(lang => path.join(options.out, lang));

            await captureTerminals(page, path.join(__dirname, 'terminal-shots.json'), outputs, options.only);
            return;
        }

        if (!options.base || !cultures[options.lang] || !options.admin) {
            usage();
            process.exit(1);
        }

        const context = {
            ...options,
            base: options.base.replace(/\/$/, ''),
            culture: cultures[options.lang],
            output: path.join(options.out, options.lang)
        };

        fs.mkdirSync(context.output, { recursive: true });

        if (options.command === 'site')
            await captureSite(page, context);
        else
            await captureWizard(page, context);
    } finally {
        await page.close();
    }
}

main().then(() => process.exit(0)).catch(error => {
    console.error(error);
    process.exit(1);
});
