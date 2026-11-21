$(document).on('click', '[data-admin-edit]', function () {
    var id = $(this).attr('data-admin-edit');
    var $title = $('#admin-edit-title');
    var editing = id !== '';

    $title.text(editing ? $title.attr('data-edit') : $title.attr('data-add'));
    $('#communityId').val(id).prop('readonly', editing);
    $('#admin-edit-modal .admin-picker').prop('disabled', editing);
    $('#name').val(editing ? $(this).attr('data-admin-name') : '');
    $('#admin-edit-modal').modal('show');
});

$('#admin-edit-modal').on('shown.bs.modal', function () {
    $($('#communityId').prop('readonly') ? '#name' : '#communityId').trigger('focus');
});
