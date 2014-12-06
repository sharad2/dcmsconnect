///Script used by BoxEditor Ui to perform  create dialog
/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

$(document).ready(function () {
    var count = 0;
    $('#divRemoveSkuDialog').dialog({
        title: 'Remove SKUs from Box #' + $('#hfUcc128Id').val(),
        autoOpen: false,
        width: 'auto',
        modal: true,
        closeOnEscape: false,
        open: function () {
            count = 0;
            $('input.input-validation-error').removeClass('input-validation-error');
            $('span.field-validation-error').removeClass('field-validation-error').addClass('field-validation-valid');
            $('#divAjaxErrors').removeClass('ui-state-error').removeClass('ui-state-hightlight').html('');
            $('#tbUpc').val('');
            $('#tbCoo').val('');
        },
        buttons: [
            {
                id: 'btnRemoveSku',
                text: 'Remove',
                click: function (event, ui) {
                    var $form = $('form', this);
                    if (!$form.valid()) {
                        return false;
                    }
                    var dialogData = $form.serializeArray();
                    $.ajax({
                        cache: false,
                        url: $form.attr('action'),
                        type: 'POST',
                        data: dialogData,
                        context: this,
                        statusCode: {
                            // Success
                            201: function (data, textStatus, jqXHR) {
                                count = count + 1;
                                $('#divAjaxErrors').html(count + ' pieces of box ' + $('#hfUcc128Id').val() + ' has been removed.').removeClass('ui-state-error').addClass('ui-state-highlight');
                                $('#tbUpc').val('').focus();
                                $('#tbCoo').val('');
                                $('#divCountryOfOrigin').hide();
                                $('#divUpdateBoxSku').html(data);
                                $('#spanWeight').html(jqXHR.getResponseHeader("BoxWeight"));
                            },
                            // Error
                            203: function (data, textStatus, jqXHR) {
                                $('#tbUpc').val('');
                                $('#tbCoo').val('');
                                $('#divCountryOfOrigin').hide();
                                $('#divAjaxErrors').html(data).removeClass('ui-state-highlight');
                            },
                            // SKU Contains Country of Origin.
                            250: function (data, textStatus, jqXHR) {
                                $('#skuRemoved').html('');
                                countryOfOrigin();
                                $('#divAjaxErrors').html(data).removeClass('ui-state-hightlight').addClass('ui-state-error');
                                $('#tbCoo').focus();
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            alert(jqXHR.responseText);
                        }
                    });
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
 
    //For handle keydown event on textbox of scan UPC.
    $('#tbUpc').keydown(function (e) {
        if (e.keyCode === $.ui.keyCode.ENTER) {
            $('#btnRemoveSku').click();
            return false;
        }
    });

    //For handle keydown event on textbox of scan Country of Origin.
    $('#tbCoo').keydown(function (e) {
        if (e.keyCode === $.ui.keyCode.ENTER) {
            $('#btnRemoveSku').click();
            return false;
        }
    });

    //make div as dialog for remove SKUs pieces.
    $('#btnRemoveSkuDialog').click(function () {
        $('#divRemoveSkuDialog').dialog('open');
        $('#divCountryOfOrigin').hide();
    });

    //function for check country of origin.
    function countryOfOrigin() {
        $('#divCountryOfOrigin').show();
        $('#tbCoo').each(function () {
            $(this).rules('add', {
                // Country of origin is required
                required: function (element) {
                    return $(element).is(':visible');
                },
                messages: {
                    required: "Country of Origin Required."
                }
            });
        });
    }


    // Script For cancel the Box.....

    $('#btnCancelBox').click(function () {
        $('#divCancelDialog').dialog('open');
    });

    $('#divCancelDialog').dialog({
        title: 'Cancel Box',
        autoOpen: false,
        width: 'auto',
        modal: true,
        closeOnEscape: false,
        buttons: [
        {
            text: 'Yes',
            click: function (event, ui) {
                $('#frmCancelBox').submit();
            }
        },
        {
            text: 'Cancel',
            click: function (event, ui) {
                $('#divCancelDialog').dialog('close');
            }
        }
        ]
    });

    // Script for sending box to green area

    $('#btnGreenBox').click(function () {
        $('#divGreenDialog').dialog('open');
    });

    $('#divGreenDialog').dialog({
        title: 'Send box to Green area',
        autoOpen: false,
        width: 'auto',
        modal: true,
        closeOnEscape: false,
        buttons: [
        {
            text: 'Yes',
            click: function (event, ui) {
                $('#frmGreenBox').submit();
            }
        },
        {
            text: 'Cancel',
            click: function (event, ui) {
                $('#divGreenDialog').dialog('close');
            }
        }
        ]
    });
    // Repitch Scripts

    $('#btnRepitchBox').click(function () {
        $('#divRepitchDialog').dialog('open');
    });

    $('#divRepitchDialog').dialog({
        title: 'Repitch Box',
        autoOpen: false,
        width: 'auto',
        modal: true,
        closeOnEscape: true,
        buttons: [
        {
            text: 'Yes',
            click: function (event, ui) {
                $('#frmRepitchBox').submit();
            }
        },
        {
            text: 'Cancel',
            click: function (event, ui) {
                $('#divRepitchDialog').dialog('close');
            }
        }
        ]
    });

    //Mark VAS Complete

    $('#btnMarkVasComplete').click(function () {
        $('#divMarkVasComplete').dialog('open');
    });

    $('#divMarkVasComplete').dialog({
        title: 'Mark VAS Complete',
        modal: true,
        autoOpen: false,
        width: 'auto',
        closeOnEscape: true,
        buttons: [
            {
                text: 'Yes',
                click: function (event, ui) {
                    $('#frmMarkVasComplete').submit();
                }
            },
            {
                text: 'Cancel',
                click: function (event, ui) {
                    $('#divMarkVasComplete').dialog('close');
                }
            }
        ]
    });
});