terminal.apply = function (command) {
    $('#command').val(command);

    $('#terminal').one('hidden.bs.modal', function () {
        $('#command').trigger('focus');
    });

    return true;
};
