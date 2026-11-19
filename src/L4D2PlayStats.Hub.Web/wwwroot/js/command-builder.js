function CommandBuilder(options) {
    this.commands = options.commands;
    this.dataSources = options.dataSources;
    this.texts = options.texts;
    this.modal = options.modal;
    this.list = options.list;
    this.preview = options.preview;
    this.previewContainer = options.previewContainer;
    this.apply = options.apply || function () {
        return true;
    };
    this.path = [];
}

CommandBuilder.prototype.show = function () {
    this.path = [];
    this.build();
    $(this.modal).modal();
};

CommandBuilder.prototype.close = function () {
    $(this.modal).modal('hide');
};

CommandBuilder.prototype.previous = function () {
    if (!this.path.length) {
        this.close();
        return;
    }

    this.path.pop();
    this.build();
};

CommandBuilder.prototype.next = function (value) {
    this.path.push(value);
    this.build();
};

CommandBuilder.prototype.build = function () {
    var self = this;
    var options = [];
    var input = null;

    this.clearOptions();

    if (this.path.length == 0) {
        this.addOption({ displayText: this.texts.selectCommand, header: true });

        options = $.map(Object.keys(this.commands), function (command) {
            return { value: command, description: self.commands[command].description };
        });
    } else {
        var command = this.commands[this.path[0]];

        if (this.path.length == command.args.length + 1) {
            this.updatePreview();
            this.flush();
            return;
        }

        var dataSource = this.dataSources[command.args[this.path.length - 1]];
        var items = dataSource.items();

        this.addOption({ displayText: dataSource.title, header: true });

        if (dataSource.input)
            input = dataSource.input();

        if (input)
            input = this.addInput(input);

        options = $.map(items, function (item) {
            return $.isPlainObject(item) ? item : { value: item };
        });
    }

    $.each(options, function (index, option) {
        self.addOption(option, function (e) {
            e.preventDefault();
            self.next($(this).data('value'));
        });
    });

    if (this.path.length) {
        this.addOption({ displayText: '← ' + this.texts.back }, function (e) {
            e.preventDefault();
            self.previous();
        });
    }

    this.updatePreview();

    $(this.list).scrollTop(0);

    if (input)
        input.trigger('focus');
};

CommandBuilder.prototype.clearOptions = function () {
    $(this.list).empty();
};

CommandBuilder.prototype.addOption = function (option, onClick) {
    var $item = onClick ? $('<a>', { href: '#' }) : $('<div>');

    $item.addClass('list-group-item');

    if (option.header)
        $item.addClass('active');

    if (option.value !== undefined && option.value !== null)
        $item.data('value', option.value);

    if (onClick)
        $item.on('click', onClick);

    $item.append($('<h5>', {
        class: 'list-group-item-heading',
        text: option.displayText || option.value
    }));

    if (option.description) {
        $item.append($('<small>', {
            class: 'list-group-item-text',
            text: option.description
        }));
    }

    $(this.list).append($item);
};

CommandBuilder.prototype.addInput = function (placeholder) {
    var self = this;
    var $input = $('<input>', { type: 'text', class: 'form-control', placeholder: placeholder });
    var $button = $('<button>', { type: 'button', class: 'btn btn-primary', text: this.texts.next });

    var submit = function () {
        var value = $.trim($input.val());

        if (value)
            self.next(value);
    };

    $button.on('click', submit);

    $input.on('keydown', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            submit();
        }
    });

    $('<div>', { class: 'list-group-item' })
        .append($('<div>', { class: 'input-group' }).append($input, $('<span>', { class: 'input-group-btn' }).append($button)))
        .appendTo($(this.list));

    return $input;
};

CommandBuilder.prototype.command = function () {
    var values = $.grep(this.path || [], function (value) {
        return value != null;
    });

    values = $.map(values, function (value) {
        return String(value).trim();
    });

    values = $.grep(values, function (value) {
        return value != '';
    });

    values = $.map(values, function (value) {
        return value.includes(' ') ? '"' + value + '"' : value;
    });

    return values.join(' ');
};

CommandBuilder.prototype.updatePreview = function () {
    var command = this.command();

    if (command) {
        $(this.previewContainer).show();
        $(this.preview).text(command);
    } else {
        $(this.previewContainer).hide();
    }
};

CommandBuilder.prototype.flush = function () {
    var command = this.command();

    if (!command)
        return;

    if (this.apply(command) === false) {
        this.previous();
        return;
    }

    this.close();
};
