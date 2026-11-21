(function () {
    var form = document.getElementById('vf-filters');

    function resolve(nameId, listId, hiddenId) {
        var input = document.getElementById(nameId);
        var hidden = document.getElementById(hiddenId);

        if (!input || !hidden)
            return;

        if (hidden.dataset.picked) {
            hidden.disabled = !hidden.value;
            return;
        }

        var text = input.value.trim().toLowerCase();
        var match = Array.prototype.find.call(document.getElementById(listId).options, function (option) {
            return option.value.toLowerCase() === text;
        });

        hidden.value = match ? match.getAttribute('data-id') : '';
        hidden.disabled = !hidden.value;
    }

    form.addEventListener('submit', function () {
        resolve('vf-voter-name', 'vf-voters', 'vf-voter');
        resolve('vf-player-name', 'vf-players', 'vf-player');
    });

    document.addEventListener('DOMContentLoaded', function () {
        $('#vf-voter, #vf-player').on('change', function () {
            this.dataset.picked = '1';
        });
    });

    ['vf-voter-name', 'vf-player-name'].forEach(function (id) {
        var input = document.getElementById(id);

        if (input)
            input.addEventListener('change', function () {
                form.requestSubmit ? form.requestSubmit() : form.submit();
            });
    });
})();
