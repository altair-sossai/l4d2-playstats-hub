(function () {
    var editor = document.getElementById('campaign-map-editor');
    var rows = editor.querySelector('.cm-rows');
    var hidden = document.getElementById('campaign-maps');
    var template = editor.querySelector('.cm-template');
    var form = document.getElementById('campaign-form');

    function refresh() {
        var maps = [];
        var all = rows.querySelectorAll('.cm-row');

        all.forEach(function (row, index) {
            var map = row.querySelector('.cm-map').value.trim();

            row.querySelector('.cm-order').textContent = (index + 1) + '.';
            row.querySelector('.cm-up').disabled = index === 0;
            row.querySelector('.cm-down').disabled = index === all.length - 1;

            if (map)
                maps.push(map);
        });

        hidden.value = maps.join('\n');
        editor.classList.toggle('cm-none', all.length === 0);

        return maps;
    }

    function addRow(map) {
        var row = template.content.firstElementChild.cloneNode(true);

        row.querySelector('.cm-map').value = map || '';
        rows.appendChild(row);

        return row;
    }

    editor.querySelector('.cm-add').addEventListener('click', function () {
        addRow('').querySelector('.cm-map').focus();
        editor.classList.remove('cm-invalid');
        refresh();
    });

    rows.addEventListener('click', function (event) {
        var button = event.target.closest('button');

        if (!button)
            return;

        var row = button.closest('.cm-row');

        if (button.classList.contains('cm-remove'))
            row.remove();
        else if (button.classList.contains('cm-up') && row.previousElementSibling)
            rows.insertBefore(row, row.previousElementSibling);
        else if (button.classList.contains('cm-down') && row.nextElementSibling)
            rows.insertBefore(row.nextElementSibling, row);

        refresh();
    });

    rows.addEventListener('input', function () {
        editor.classList.remove('cm-invalid');
        refresh();
    });

    form.addEventListener('submit', function (event) {
        if (refresh().length > 0)
            return;

        event.preventDefault();
        editor.classList.add('cm-invalid');
    });

    $(document).on('click', '[data-campaign-edit]', function () {
        var id = $(this).attr('data-campaign-edit');
        var $title = $('#campaign-edit-title');
        var editing = id !== '';
        var maps = editing ? ($(this).attr('data-campaign-maps') || '').split('\n') : [''];

        $title.text(editing ? $title.attr('data-edit') : $title.attr('data-add'));
        $('#campaign-id').val(id);
        $('#campaign-name').val(editing ? $(this).attr('data-campaign-name') : '');
        rows.innerHTML = '';
        maps.forEach(addRow);
        editor.classList.remove('cm-invalid');
        refresh();
        $('#campaign-edit-modal').modal('show');
    });

    $('#campaign-edit-modal').on('shown.bs.modal', function () {
        $('#campaign-name').trigger('focus');
    });
})();
