(function () {
    var outcomes = {
        Left: 'is-left',
        Right: 'is-right',
        SameLevel: 'is-same',
        UnknownLeft: 'is-unknown-left',
        UnknownRight: 'is-unknown-right',
        UnknownBoth: 'is-unknown-both',
        UnknownOne: 'is-unknown-one'
    };
    var keys = { ArrowLeft: 'Left', ArrowRight: 'Right', ArrowDown: 'SameLevel', ArrowUp: 'UnknownOne' };

    document.addEventListener('click', function (event) {
        var photo = event.target.closest('.ballot:not(.ballot-busy) .ballot-photo');

        if (photo) {
            photo.closest('.ballot-card').querySelector('.ballot-pick').click();
            return;
        }

        var button = event.target.closest('.ballot button[name="Outcome"]');

        if (!button)
            return;

        if (button.form.classList.contains('ballot-busy')) {
            event.preventDefault();
            return;
        }

        button.form.classList.add('ballot-busy', outcomes[button.value]);
    });

    document.addEventListener('keydown', function (event) {
        if (event.ctrlKey || event.altKey || event.metaKey || event.shiftKey)
            return;

        if (/^(INPUT|TEXTAREA|SELECT)$/.test(document.activeElement.tagName))
            return;

        var value = keys[event.key];
        var button = value && document.querySelector('.ballot:not(.ballot-busy) button[value="' + value + '"]');

        if (!button)
            return;

        event.preventDefault();
        button.click();
    });

    document.addEventListener('htmx:afterRequest', function (event) {
        var form = event.target.closest && event.target.closest('.ballot');

        if (!form || event.detail.successful)
            return;

        form.className = 'ballot';
    });
})();
