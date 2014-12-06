$(document).ready(function () {
    // The dialog must be passed $tr (current row which has been clicked) in the custom option currentRow

    $('#divUpdateAreaDialog').dialog({
        autoOpen: false,
        width: 'auto',
        modal: true,
        closeOnEscape: true,
        open: function (event, ui) {
            var $tr = $(this).dialog('option', 'currentRow');
            $('#hfAreaId').val($tr.find('span.data-area-id').html());
            $('div[data-valmsg-summary]', this)
                .addClass('validation-summary-valid')
                .removeClass('validation-summary-errors');
            //For location numbering flag.
            if ($tr.find('span.data-numbered-location').length == 1) {
                $('#cbLocationNumberingFlag').attr('CHECKED', 'checked');
            } else {
                $('#cbLocationNumberingFlag').removeAttr('CHECKED');
            }
            //For pallet required.
            if ($tr.find('span.data-Pallet-Required').length == 1) {
                $('#cbPalletRequired').attr('CHECKED', 'checked');
            } else {
                $('#cbPalletRequired').removeAttr('CHECKED');
            }
            // For Unusable Inventory
            if ($tr.find('span.data-Unusable-Inventory').length == 1) {
                $('#cbUnusableInventory').attr('CHECKED', 'checked');
            } else {
                $('#cbUnusableInventory').removeAttr('CHECKED');
            }
            $('#tbDescription').val($tr.find('span.data-Description').text());
            $(this).dialog({ title: $tr.find('span.ui-icon-pencil').attr('title') });
            $("#btnUpdate").button({ icons: { primary: "ui-icon-disk"} });
        },
        buttons: [
            {
                id: 'btnUpdate',
                text: 'Update',
                click: function (event, ui) {
                    var $form = $('form', this);
                    if (!$form.valid()) {
                        return false;
                    }
                    $form.submit();
                }
            },
            {
                text: 'Cancel',
                click: function (event, ui) {
                    $(this).dialog('close');
                }
            }
            ]
    });

    // When the pencil icon is clicked, show the dialog
    $('#divAreaList').on('click', 'span.ui-icon-pencil', function (e) {
            var $tr = $(this).closest('tr');
            $dlg = $('#divUpdateAreaDialog');
            var $oldtr = $dlg.dialog('option', 'currentRow');
            $oldtr && $oldtr.removeClass('ui-state-active');
            $tr.addClass('ui-state-active');
            $dlg.dialog('option', 'currentRow', $tr)
            .dialog('open');
        $('#divUpdateAreaDialog').dialog('open');

    });
});  