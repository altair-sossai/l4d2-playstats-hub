const desktop = 1280;
const phone = 390;

function expect(condition, message) {
    if (!condition)
        throw new Error(message);
}

async function detailsOnAPhone(browser, address, table, columns, label) {
    await browser.page.viewport(phone, 844);
    await browser.open(address);

    const state = await browser.eval(`(() => {
        const row = document.querySelector('${table} tbody tr');
        const visible = row ? Array.from(row.children).filter(cell => getComputedStyle(cell).display !== 'none') : [];
        return { columns: visible.length, button: !!(row && row.querySelector('[data-details]') && row.querySelector('[data-details]').getBoundingClientRect().width > 0), scroll: document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1 };
    })()`);

    expect(state.columns === columns, `${label} shows ${state.columns} columns on a phone instead of ${columns}`);
    expect(state.button, `${label} has no details button on a phone`);
    expect(state.scroll, `${label} scrolls sideways on a phone`);

    await browser.click(`${table} tbody tr [data-details]`, { settle: 100 });
    await browser.modalShown('#details-modal');

    const details = await browser.eval(`({ title: document.querySelector('#details-modal .modal-title').innerText.trim(), rows: document.querySelectorAll('#details-modal .details-list > div').length, empty: Array.from(document.querySelectorAll('#details-modal .details-list dd')).filter(dd => dd.innerText.trim() === '').length })`);

    expect(details.title && details.rows >= 4 && details.empty === 0, `The details window of ${label} shows ${JSON.stringify(details)}`);
    await browser.click('#details-modal .modal-footer [data-dismiss="modal"]', { settle: 100 });
    await browser.modalHidden('#details-modal');

    await browser.page.viewport(desktop, 800);
    await browser.open(address);
    expect(await browser.eval(`document.querySelector('${table} tbody tr [data-details]').getBoundingClientRect().width === 0`), `${label} shows the details button on a computer`);
}

module.exports = { detailsOnAPhone };
