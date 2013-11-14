$(document).ready(function (e) {
    var $tablePallets = $('#tblPallets');
    var $tbPallet = $('#tbPalletId');
    var $createDialogForm = $('#frmCreate').keypress(function (e) {
        if (e.which == $.ui.keyCode.ENTER) {
            $('#btnCreatePallet').click();
            e.preventDefault();
        }
    });

    $('button').button();

    var $divCreatedlg = $('#divCreateDialog');
    // Open the create pallet dialog
    $('#btnNonExpeditedBox').click(function (e) {
        $tbPallet.val('');
        $divCreatedlg
            .dialog('option', 'currentRow', '')
        .dialog('option', 'title', 'Create Pallet')
            .dialog('open');
        $('#btnCreatePallet').button({ label: 'Create Pallet' }, { icons: { primary: 'ui-icon-disk'} });
    }).button();

    $divCreatedlg.dialog({
        title: 'Create Pallet',
        autoOpen: false,
        width: 380,
        closeOnEscape: false,
        modal: true,
        open: function (event, ui) {
            var $currentrow = $(this).dialog('option', 'currentRow');
            if ($currentrow != '') {
                $tbPallet.val($currentrow.find('td a.data-palletId').text().replace(/\s+/g, ''));
                $('#divInfo').show();
            }
            else {
                $('#divInfo').hide();
            }
            $createDialogForm.validate().resetForm();
            $divCreatedlg.find('span.field-validation-error').removeClass('field-validation-error').addClass('field-validation-valid');
        },
        buttons: [
            {
                id: 'btnCreatePallet',
                click: function (event, ui) {
                    if (!$createDialogForm.valid()) {
                        return false;
                    }
                    $createDialogForm.submit();
                }
            },
            {
                text: 'Cancel',
                click: function (event, ui) {
                    $tbPallet.val('');
                    $(this).dialog('close');
                }
            }
            ]
    });

    $tablePallets.find('button').each(function (e) {
        $(this).button({ text: false }, { icons: { primary: $(this).attr('data-icon')} });
    });

    $tablePallets.find('button.btnExpediteBoxes')
            .end()
            .click(function (e) {
                if ($(e.target).is('button.btnExpediteBoxes')) {
                    $('#btnCreatePallet').button({ label: 'Add Cartons' }, { icons: { primary: 'ui-icon-disk'} });
                    $divCreatedlg
                    .dialog('option', 'title', 'Add Cartons to Pallet')
                        .dialog('option', 'currentRow', $(e.target).closest('tr'))
                        .dialog('open');

                }
            });

    $tablePallets.find('button.btnRemoveUnpickedBoxes')
            .end()
            .click(function (e) {
                if ($(e.target).is('button.btnRemoveUnpickedBoxes')) {
                    var msg = "This will remove " + $(e.target).attr('data-unpicked-boxes') + " unpicked boxes from the pallet. Press OK to continue.";
                    return confirm(msg);
                }
                else if ($(e.target).is('a.ui-icon-print')) {
                    msg = "This will print " + $(e.target).attr('data-total-boxes') + " UCC Labels. To print UCC labels for specific boxes, you should click the pallet instead. Press OK to continue.";
                    return confirm(msg);
                }
            });
});

