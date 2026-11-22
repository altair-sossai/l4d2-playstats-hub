var patentsAdminIndexConfig = JSON.parse(document.getElementById("patents-admin-index-config").textContent);

(function () {
    var input = document.getElementById('MaxExperience');
    var curveInput = document.getElementById('Curve');
    var levels = patentsAdminIndexConfig.values.patentLevels;
    var rounding = patentsAdminIndexConfig.values.patentExperienceRounding;
    var format = new Intl.NumberFormat(document.documentElement.lang || undefined);

    function refresh() {
        var max = parseInt(input.value, 10);
        var curve = parseFloat(String(curveInput.value).replace(',', '.'));

        if (isNaN(max) || max <= 0 || isNaN(curve) || curve <= 0)
            return;

        document.querySelectorAll('[data-pa-level]').forEach(function (cell) {
            var level = parseInt(cell.getAttribute('data-pa-level'), 10);
            var value = Math.floor(Math.pow((level - 1) / (levels - 1), curve) * max / rounding) * rounding;

            cell.textContent = format.format(value);
        });
    }

    input.addEventListener('input', refresh);
    curveInput.addEventListener('input', refresh);
})();
