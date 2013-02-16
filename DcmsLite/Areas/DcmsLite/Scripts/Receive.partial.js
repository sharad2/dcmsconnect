$(document).ready(function () {
    $('button[data-icon]').each(function () {
        $(this).button({ icons: { primary: $(this).attr('data-icon') } });
    }).click(function (e) {
        $('#divReceivingDialog').dialog("option", "currentForm", $(e.target).closest('form')).dialog('open');
        return false;
    });

    $('#divReceivingDialog').dialog({
        title: 'Receive Cartons',
        autoOpen: false,
        width: 'auto',
        modal: true,
        closeOnEscape: false,
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close").hide();
        },
        buttons: [
        {
            text: 'OK',
            click: function (event, ui) {
                var $form = $(this).dialog('option', 'currentForm');
                $(event.target).button({ disabled: true });
                $('#btnCancel').button({ disabled: true });
                $form.submit();
            }
        },
        {
            text: 'Cancel',
            id: 'btnCancel',
            click: function (event, ui) {
                $('#divReceivingDialog').dialog('close');
            }
        }
        ]
    });
});