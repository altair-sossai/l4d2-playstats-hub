function banDuration($select, ends) {
    var $ends = $($select.attr('data-ban-ends'));

    $select.val(ends ? 'custom' : '0');
    $ends.val(ends || '');
    $select.trigger('change');
}

$(document).on('change', '.ban-duration', function () {
    var $select = $(this);
    var custom = $select.val() === 'custom';
    var $ends = $($select.attr('data-ban-ends'));

    $ends.prop('required', custom).closest('.ban-ends-group').toggle(custom);
});

$(document).on('click', '[data-ban-add]', function () {
    var $button = $(this);
    var player = $button.attr('data-ban-add');

    $('#ban-player').val(player);
    $('#ban-name').val(player ? $button.attr('data-ban-name') || '' : '');
    $('#ban-add-reason').val($button.attr('data-ban-reason') || '');
    $('#ban-related-to').val($button.attr('data-ban-related-to') || '');
    banDuration($('#ban-add-duration'), $button.attr('data-ban-ends') || '');
    $('#ban-add-modal').modal('show');
});

$(document).on('click', '[data-ban-edit]', function () {
    var $button = $(this);

    $('#ban-edit-id').val($button.attr('data-ban-edit'));
    $('#ban-edit-modal .ban-edit-name').text($button.attr('data-ban-name'));
    $('#ban-edit-reason').val($button.attr('data-ban-reason') || '');
    banDuration($('#ban-edit-duration'), $button.attr('data-ban-ends') || '');
    $('#ban-edit-modal').modal('show');
});

$(document).on('click', '[data-ban-lift]', function () {
    var $button = $(this);

    $('#ban-lift-id').val($button.attr('data-ban-lift'));
    $('#ban-lift-modal .ban-lift-name').text($button.attr('data-ban-name'));
    $('#ban-lift-reason').val('');
    $('#ban-lift-modal').modal('show');
});

$('#ban-add-modal').on('shown.bs.modal', function () {
    $($('#ban-player').val() ? '#ban-add-reason' : '#ban-player').trigger('focus');
});

$('.ban-duration').trigger('change');
