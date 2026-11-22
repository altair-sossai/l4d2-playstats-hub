var layout2Config = JSON.parse(document.getElementById("layout-2-config").textContent);

(function () {
    var collapse = document.getElementById('bs-navbar-collapse');
    var main = document.getElementById('nav-main');
    var more = document.getElementById('nav-more');
    var menu = document.getElementById('nav-more-menu');
    var divider = document.getElementById('nav-fit-divider');
    var navbar = collapse.parentNode.parentNode;
    var header = navbar.querySelector('.navbar-header');
    var ownItems = more.getAttribute('data-own') === 'true';

    function wraps() {
        return collapse.offsetHeight > header.offsetHeight * 1.5;
    }

    function markActive() {
        if (menu.querySelector('li.active'))
            more.classList.add('active');
        else
            more.classList.remove('active');
    }

    function restore() {
        var moved = menu.querySelectorAll('li.nav-fit');

        for (var index = 0; index < moved.length; index++)
            main.insertBefore(moved[index], more);

        divider.style.display = 'none';
        more.style.display = ownItems ? '' : 'none';
        navbar.classList.remove('nav-compact');
    }

    function fit() {
        restore();

        if (window.innerWidth >= layout2Config.values.navbarDesktopWidth && wraps()) {
            navbar.classList.add('nav-compact');

            var items = main.querySelectorAll('li.nav-fit');

            for (var index = items.length - 1; index >= 0 && wraps(); index--) {
                more.style.display = '';
                divider.style.display = ownItems ? '' : 'none';
                menu.insertBefore(items[index], menu.firstChild);
            }
        }

        markActive();
    }

    window.fitNavbar = fit;
    window.addEventListener('resize', fit);
    window.addEventListener('load', fit);

    fit();
})();
