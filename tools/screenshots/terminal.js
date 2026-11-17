const fs = require('fs');
const os = require('os');
const path = require('path');
const { pathToFileURL } = require('url');

const palette = {
    30: '#3b3b3b', 31: '#f14c4c', 32: '#23d18b', 33: '#e5c07b', 34: '#3b8eea', 35: '#d670d6', 36: '#29b8db', 37: '#e5e5e5',
    90: '#8a8a8a', 91: '#ff6b6b', 92: '#4be3a4', 93: '#f5f543', 94: '#61a8ff', 95: '#e48ae4', 96: '#4fd0ee', 97: '#ffffff'
};

function color256(index) {
    if (index < 8)
        return palette[30 + index];

    if (index < 16)
        return palette[90 + index - 8];

    if (index >= 232) {
        const level = 8 + (index - 232) * 10;

        return `rgb(${level},${level},${level})`;
    }

    const value = index - 16;
    const steps = [0, 95, 135, 175, 215, 255];

    return `rgb(${steps[Math.floor(value / 36)]},${steps[Math.floor(value / 6) % 6]},${steps[value % 6]})`;
}

function applySgr(style, parameters) {
    const codes = parameters.length === 0 ? [0] : parameters;
    const next = { ...style };

    for (let index = 0; index < codes.length; index++) {
        const code = codes[index];

        if (code === 0) {
            next.color = null;
            next.bold = false;
            next.dim = false;
        } else if (code === 1) {
            next.bold = true;
        } else if (code === 2) {
            next.dim = true;
        } else if (code === 22) {
            next.bold = false;
            next.dim = false;
        } else if (code === 39) {
            next.color = null;
        } else if (palette[code]) {
            next.color = palette[code];
        } else if (code === 38 && codes[index + 1] === 5) {
            next.color = color256(codes[index + 2]);
            index += 2;
        } else if (code === 38 && codes[index + 1] === 2) {
            next.color = `rgb(${codes[index + 2]},${codes[index + 3]},${codes[index + 4]})`;
            index += 4;
        } else if (code === 48 && (codes[index + 1] === 5 || codes[index + 1] === 2)) {
            index += codes[index + 1] === 5 ? 2 : 4;
        }
    }

    return next;
}

function emulate(text) {
    const lines = [[]];
    let row = 0;
    let column = 0;
    let style = { color: null, bold: false, dim: false };

    const line = index => {
        while (lines.length <= index)
            lines.push([]);

        return lines[index];
    };

    for (let index = 0; index < text.length; index++) {
        const character = text[index];

        if (character === '\x1b') {
            const kind = text[index + 1];

            if (kind === '[') {
                let end = index + 2;

                while (end < text.length && !/[@-~]/.test(text[end]))
                    end++;

                const body = text.slice(index + 2, end);
                const final = text[end];
                const numbers = body.replace(/^\?/, '').split(';').filter(part => part !== '').map(Number);
                const amount = numbers[0] || 1;

                if (final === 'm' && !body.startsWith('?'))
                    style = applySgr(style, numbers);
                else if (final === 'A')
                    row = Math.max(0, row - amount);
                else if (final === 'B')
                    row += amount;
                else if (final === 'C')
                    column += amount;
                else if (final === 'D')
                    column = Math.max(0, column - amount);
                else if (final === 'G')
                    column = Math.max(0, (numbers[0] || 1) - 1);
                else if (final === 'K') {
                    const current = line(row);

                    if (numbers[0] === 2)
                        current.length = 0;
                    else if (numbers[0] === 1)
                        current.fill({ character: ' ', style }, 0, column);
                    else
                        current.length = Math.min(current.length, column);
                } else if (final === 'J') {
                    line(row).length = Math.min(line(row).length, column);
                    lines.length = row + 1;
                }

                index = end;
            } else if (kind === ']') {
                let end = index + 2;

                while (end < text.length && text[end] !== '\x07' && !(text[end] === '\x1b' && text[end + 1] === '\\'))
                    end++;

                index = text[end] === '\x07' ? end : end + 1;
            } else {
                index++;
            }
        } else if (character === '\r') {
            column = 0;
        } else if (character === '\n') {
            row++;
            column = 0;
            line(row);
        } else if (character === '\t') {
            column = (Math.floor(column / 8) + 1) * 8;
        } else if (character >= ' ') {
            const current = line(row);

            while (current.length < column)
                current.push({ character: ' ', style: {} });

            current[column] = { character, style };
            column++;
        }
    }

    while (lines.length > 0 && lines[lines.length - 1].every(cell => cell.character === ' '))
        lines.pop();

    return lines;
}

function escape(text) {
    return text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function renderLine(cells) {
    let html = '';
    let open = null;

    for (const cell of cells) {
        const key = JSON.stringify(cell.style || {});

        if (key !== open) {
            if (open !== null)
                html += '</span>';

            const style = cell.style || {};
            const css = [style.color ? `color:${style.color}` : '', style.bold ? 'font-weight:bold' : '', style.dim ? 'opacity:.7' : ''].filter(Boolean).join(';');

            html += `<span style="${css}">`;
            open = key;
        }

        html += escape(cell.character);
    }

    if (open !== null)
        html += '</span>';

    return html.replace(/(\s|&nbsp;)+(<\/span>)?$/, '$2');
}

function prompt(directory) {
    return `<span class="user">root@vps</span>:<span class="dir">${escape(directory)}</span># `;
}

function renderSession(session, folder) {
    const directory = session.directory || '~';
    const body = [];

    for (const block of session.blocks) {
        body.push(prompt(directory) + escape(block.command));

        if (block.output) {
            const text = fs.readFileSync(path.join(folder, block.output), 'utf8');

            for (const cells of emulate(text))
                body.push(renderLine(cells));
        }
    }

    if (session.finalPrompt !== false)
        body.push(prompt(directory) + '<span class="cursor"> </span>');

    return `<!doctype html><html><head><meta charset="utf-8"><style>
        body { margin: 0; padding: 24px; background: #ffffff; font-family: 'Cascadia Mono', Consolas, 'DejaVu Sans Mono', monospace; }
        .window { display: inline-block; min-width: 760px; border-radius: 8px; overflow: hidden; background: #1e1e1e; box-shadow: 0 6px 20px rgba(0,0,0,.35); border: 1px solid #3c3c3c; }
        .bar { height: 30px; background: #2d2d2d; position: relative; color: #b8b8b8; font: 13px 'Segoe UI', Arial, sans-serif; line-height: 30px; text-align: center; }
        .dots { position: absolute; left: 12px; top: 9px; }
        .dots i { display: inline-block; width: 12px; height: 12px; border-radius: 6px; margin-right: 7px; }
        pre { margin: 0; padding: 12px 16px 14px; color: #d4d4d4; font: 14px/1.45 'Cascadia Mono', Consolas, 'DejaVu Sans Mono', monospace; white-space: pre; }
        .user { color: #23d18b; font-weight: bold; }
        .dir { color: #3b8eea; font-weight: bold; }
        .cursor { background: #d4d4d4; }
    </style></head><body><div class="window"><div class="bar"><span class="dots"><i style="background:#ff5f57"></i><i style="background:#febc2e"></i><i style="background:#28c840"></i></span>${escape(session.title || 'root@vps: ' + directory)}</div><pre>${body.join('\n')}</pre></div></body></html>`;
}

async function captureTerminals(page, sessionsFile, outputs, only) {
    const folder = path.dirname(sessionsFile);
    const sessions = JSON.parse(fs.readFileSync(sessionsFile, 'utf8'));
    const temporary = fs.mkdtempSync(path.join(os.tmpdir(), 'playstats-terminal-'));

    await page.viewport(1400, 900);

    for (const session of sessions) {
        if (only && !only.includes(session.name))
            continue;

        const file = path.join(temporary, session.name + '.html');

        fs.writeFileSync(file, renderSession(session, path.join(folder, 'terminal')));
        await page.goto(pathToFileURL(file).href, 300);

        for (const output of outputs) {
            const target = path.join(output, session.name + '.webp');
            const info = await page.capture(target, { clip: '.window', padding: 0, quality: 90 });

            console.log(`${target} ${info.width}x${info.height} ${Math.round(info.bytes / 1024)} KB`);
        }
    }

    fs.rmSync(temporary, { recursive: true, force: true });
}

module.exports = { captureTerminals, emulate };
