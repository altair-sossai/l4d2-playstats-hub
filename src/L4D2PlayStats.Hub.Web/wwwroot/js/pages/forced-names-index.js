$(document).on('click', '[data-fn-edit]', function () {
    var id = $(this).attr('data-fn-edit');
    var $title = $('#fn-edit-title');
    var editing = id !== '';

    $title.text(editing ? $title.attr('data-edit') : $title.attr('data-add'));
    $('#fn-player').val(id).prop('readonly', editing);
    $('#fn-edit-modal .fn-picker').prop('disabled', editing);
    $('#fn-name').val(editing ? $(this).attr('data-fn-name') : '');
    $('#fn-edit-modal').modal('show');
});

$('#fn-edit-modal').on('shown.bs.modal', function () {
    $($('#fn-player').prop('readonly') ? '#fn-name' : '#fn-player').trigger('focus');
});
