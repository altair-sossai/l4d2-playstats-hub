document.addEventListener('DOMContentLoaded', function () {
    $(document).on('click', '[data-details]', function () {
        var $button = $(this);
        var $modal = $('#details-modal');

        $modal.find('.modal-title').text($button.attr('data-details-title') || '');
        $modal.find('.modal-body').html($($button.attr('data-details')).html());
        $modal.modal('show');
    });
});
