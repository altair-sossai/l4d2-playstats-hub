$(document).ready(function () {
    $('#btn-show-more').click(function () {
        $('#table-ranking tbody tr').removeClass('hidden');
        $('#btn-show-more').hide();
    });
});
