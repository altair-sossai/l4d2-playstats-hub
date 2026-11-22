var adminGameModesConfig = JSON.parse(document.getElementById("admin-game-modes-config").textContent);

$(document).on('click', '[data-rule-edit]', function () {
    var $button = $(this);
    var id = $button.attr('data-rule-edit');
    var $title = $('#rule-edit-title');
    var editing = id !== '';

    $title.text(editing ? $title.attr('data-edit') : $title.attr('data-add'));
    $('#rule-id').val(editing ? id : '0');
    $('#rule-name').val(editing ? $button.attr('data-rule-name') : '');
    $('#rule-pattern').val(editing ? $button.attr('data-rule-pattern') : adminGameModesConfig.values["L4D2PlayStats.Hub.Core.AppConsts.AnyConfigurationPattern"]);
    $('#rule-team-size').val(editing ? $button.attr('data-rule-team-size') : '');
    $('#rule-minimum-maps').val(editing ? $button.attr('data-rule-minimum-maps') : adminGameModesConfig.values["L4D2PlayStats.Hub.Core.AppConsts.DefaultMinimumMaps"]);
    $('#rule-priority').val(editing ? $button.attr('data-rule-priority') : adminGameModesConfig.values["AppConsts.GameModeNewRulePriority"]);
    $('#rule-counts').val(editing ? $button.attr('data-rule-counts') : 'true');
    $('#rule-edit-modal').modal('show');
});

$('#rule-edit-modal').on('shown.bs.modal', function () {
    $('#rule-name').trigger('focus');
});
