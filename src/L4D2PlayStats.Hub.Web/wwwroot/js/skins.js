(function () {
    var modal = document.getElementById('skin-shots-modal');

    if (!modal) {
        return;
    }

    var main = document.getElementById('skin-shots-main');
    var thumbs = document.getElementById('skin-shots-thumbs');
    var title = document.getElementById('skin-shots-title');
    var count = parseInt(modal.getAttribute('data-count'), 10);
    var extension = modal.getAttribute('data-extension');
    var thumbSuffix = modal.getAttribute('data-thumb-suffix');

    function url(folder, shot, suffix, version) {
        return folder + shot + suffix + extension + '?v=' + encodeURIComponent(version);
    }

    function select(folder, shot, version) {
        main.src = url(folder, shot, '', version);

        Array.prototype.forEach.call(thumbs.children, function (thumb) {
            thumb.classList.toggle('active', thumb.getAttribute('data-shot') === String(shot));
        });
    }

    function thumbnail(folder, shot, version) {
        var image = document.createElement('img');

        image.src = url(folder, shot, thumbSuffix, version);
        image.className = 'skin-shot-thumb';
        image.alt = '';
        image.loading = 'lazy';
        image.setAttribute('data-shot', shot);
        image.addEventListener('click', function () {
            select(folder, shot, version);
        });

        return image;
    }

    document.addEventListener('click', function (event) {
        var button = event.target.closest('[data-skin-shots]');

        if (!button) {
            return;
        }

        var folder = button.getAttribute('data-skin-shots');
        var version = button.getAttribute('data-skin-version');

        title.textContent = button.getAttribute('data-skin-title');
        thumbs.innerHTML = '';

        for (var shot = 1; shot <= count; shot++) {
            thumbs.appendChild(thumbnail(folder, shot, version));
        }

        select(folder, 1, version);
        bootstrap.Modal.getOrCreateInstance(modal).show();
    });
})();
