(function () {
    var input = document.getElementById('cmd-search');
    var count = document.getElementById('cmd-count');
    var empty = document.getElementById('cmd-empty');
    var cards = Array.prototype.slice.call(document.querySelectorAll('#cmd-page .cmd-card'));
    var sections = Array.prototype.slice.call(document.querySelectorAll('#cmd-page .cmd-section'));
    var chips = Array.prototype.slice.call(document.querySelectorAll('#cmd-chips a'));

    function normalize(text) {
        return (text || '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().replace(/\s+/g, ' ').trim();
    }

    cards.forEach(function (card) {
        card.setAttribute('data-search', normalize(card.textContent));
    });

    function openAdmin(section) {
        var body = section.querySelector('.panel-collapse');

        if (body && !body.classList.contains('in'))
            $(body).collapse('show');
    }

    function filter() {
        var terms = normalize(input.value).split(' ').filter(function (term) { return term.length > 0; });
        var total = 0;

        cards.forEach(function (card) {
            var text = card.getAttribute('data-search');
            var match = terms.every(function (term) { return text.indexOf(term) >= 0; });

            card.classList.toggle('cmd-hidden', !match);

            if (match)
                total++;
        });

        sections.forEach(function (section) {
            var visible = section.querySelector('.cmd-card:not(.cmd-hidden)') !== null;
            var chip = document.querySelector('#cmd-chips a[data-section="' + section.id + '"]');

            section.classList.toggle('cmd-hidden', !visible);

            if (chip)
                chip.classList.toggle('cmd-hidden', !visible);

            if (visible && terms.length > 0 && section.classList.contains('cmd-admin'))
                openAdmin(section);
        });

        empty.classList.toggle('cmd-hidden', terms.length === 0 || total > 0);
        count.textContent = terms.length === 0 ? '' : count.getAttribute('data-found').replace('{0}', total);
    }

    input.addEventListener('input', filter);

    chips.forEach(function (chip) {
        chip.addEventListener('click', function () {
            var section = document.getElementById(chip.getAttribute('data-section'));

            if (section && section.classList.contains('cmd-admin'))
                openAdmin(section);
        });
    });

    if (window.location.hash) {
        var target = document.getElementById(window.location.hash.substring(1));

        if (target && target.classList.contains('cmd-admin'))
            openAdmin(target);
    }
})();
