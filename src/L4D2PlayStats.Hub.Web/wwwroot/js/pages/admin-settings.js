(function () {
    var editor = document.getElementById('footer-links');

    if (!editor)
        return;

    var rows = editor.querySelector('.fl-rows');
    var hidden = document.getElementById('Site_FooterLinks');
    var template = editor.querySelector('.fl-template');

    function refresh() {
        var lines = [];

        rows.querySelectorAll('.fl-row').forEach(function (row, index, all) {
            var text = row.querySelector('.fl-text').value.replace(/\|/g, '/').trim();
            var url = row.querySelector('.fl-url').value.trim();

            row.querySelector('.fl-up').disabled = index === 0;
            row.querySelector('.fl-down').disabled = index === all.length - 1;

            if (text || url)
                lines.push(text + ' | ' + url);
        });

        hidden.value = lines.join('\n');
        editor.classList.toggle('fl-none', rows.querySelectorAll('.fl-row').length === 0);
    }

    editor.querySelector('.fl-add').addEventListener('click', function () {
        rows.appendChild(template.content.firstElementChild.cloneNode(true));
        refresh();
        rows.lastElementChild.querySelector('.fl-text').focus();
    });

    rows.addEventListener('click', function (event) {
        var button = event.target.closest('button');

        if (!button)
            return;

        var row = button.closest('.fl-row');

        if (button.classList.contains('fl-remove'))
            row.remove();
        else if (button.classList.contains('fl-up') && row.previousElementSibling)
            rows.insertBefore(row, row.previousElementSibling);
        else if (button.classList.contains('fl-down') && row.nextElementSibling)
            rows.insertBefore(row.nextElementSibling, row);

        refresh();
    });

    rows.addEventListener('input', refresh);
    editor.closest('form').addEventListener('submit', refresh);
    refresh();
})();
