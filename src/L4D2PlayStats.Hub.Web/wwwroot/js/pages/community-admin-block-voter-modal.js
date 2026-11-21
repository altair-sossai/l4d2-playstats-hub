document.addEventListener('DOMContentLoaded', function () {
    $(document).on('click', '[data-block-voter], [data-block-source]', function () {
        var $button = $(this);
        var source = $button.attr('data-block-source');
        var id = $.trim(source ? $(source).val() : $button.attr('data-block-voter') || '');

        if (!id) {
            if (source)
                $(source).focus();

            return;
        }

        var numeric = /^\d+$/.test(id);

        $('#ca-block-id').val(id);
        $('#ca-block-return').val($button.attr('data-block-return') || $('#ca-block-return').attr('data-default'));
        $('#ca-block-name').text($button.attr('data-block-name') || id);
        $('#ca-block-avatar').attr('src', numeric ? '/avatar/' + id : '').toggle(numeric);
        $('#ca-block-modal').modal('show');
    });
});
