/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

///Script used by advance Ui to perform validation and also to create dialog
$(document).ready(function () {
    //Supports the Enter button on Dialog to submit the Carton or Pallet for update
    $('#tbScan').handlescan({ formMain: '#frmMain' });
    $('#btnUpdate').button().click(function (e) {
        if (!$('#frmMain').valid()) {
            return;
        }
        $('span.field-validation-error').removeClass('field-validation-error').addClass('field-validation-valid');
        $('#divUpdate').dialog('open');
    });

    // Setup the dialog
    $('#divUpdate').dialog({
        title: 'Scan Cartons/Pallet',
        autoOpen: false,
        width: 400,
        modal: true,
        closeOnEscape: false,
        // Clear existing values
        open: function (event, ui) {
            $('#tbScan').handlescan('clearErrors');
            $('#tbConfirm').val('');
        },
        buttons: [
            {
                text: 'Go', click: function (event, ui) {
                    var $tbScan = $('#tbScan');
                    if ($tbScan.val()) {
                        $tbScan.handlescan('scan');
                    }
                    else {
                        $('#ajaxErrors').html('Please scan carton/pallet.');
                        $('#ajaxErrors').show().removeClass('success-display').addClass('field-validation-error');
                        return false;
                    }
                }
            },
            {
                text: 'Close',
                click: function (event, ui) {
                    $(this).dialog('close');
                }
            }
            ]
    });

    // Enable disable the associated radio buttons
    $('#cbqReworkStatus').change(function (e) {
        var checked = $(this).is(':checked');
        if (checked) {
            $('#rbqDoesNotNeedRework,#rbqNeedsRework').removeAttr('disabled').eq(0).attr('checked', 'checked');
        } else {
            $('#rbqDoesNotNeedRework,#rbqNeedsRework').attr('disabled', 'disabled').removeAttr('checked');
        }
    });


    // Enable disable the associated radio buttons
    // Assign value to check box on check uncheck. This is done to implement logic of validating updating rules against their values.
    $('#cbqUpdateReworkStatus').change(function (e) {
        var checked = $(this).is(':checked');
        if (checked) {
            $(this).val('Y');
            $('#rbuCompleteRework,#rbuNotSet').removeAttr('disabled').eq(0).attr('checked', 'checked');
        } else {
            $('#rbuCompleteRework,#rbuNotSet').attr('disabled', 'disabled').removeAttr('checked');
            $(this).val('');
        }
    });

    // Enable disable Pallet TextBox 
    $('#cbremoveFromPallet').change(function (e) {
        var checked = $(this).is(':checked');
        if (checked) {
            $(this).val('true');
            $('#tbPallet').attr('disabled', 'disabled');
        } else {
            $('#tbPallet').removeAttr('disabled');
            $(this).val('');
        }
    });
});

$(document).ready(function () {

    $('form').submit(function (e) {
        // None of the forms should ever be submitted. This page never posts back. It only issues ajax calls.
        // Occasionally, pressing enter causes the form to get submitted.
        return false;
    });
    $('#tbMultiField').rules("add", {
        updatable: true
    });
    $('#tbUpdatePieces').rules("add", {
        notequal: '#tbQualifyPieces',
        messages: {
            notequal: 'Qualifying pieces cannot be same as pieces to update'
        }
    });
    $('#ddlUpdateQuality').rules("add", {
        notequal: '#ddlQualifyingQuality',
        messages: {
            notequal: 'Qualifying quality code cannot be same as quality code to update'
        }
    });
    $('#ddlUpdateVwhId').rules("add", {
        notequal: '#ddlQualifyingVwhId',
        messages: {
            notequal: 'Qualifying virtual warehouse cannot be same as virtual warehouse to update'
        }
    });
    $('#tbUpdateSKU').rules("add", {
        notequal: '#tbQualifyingSKU',
        messages: {
            notequal: 'Qualifying SKU cannot be same as SKU to update'
        }
    });
    $('#ddlPriceSeasonCode').rules("add", {
        notequal: '#ddlQualifyingPriceSeasonCode',
        messages: {
            notequal: 'Qualifying season code cannot be same as season code to update'
        }
    });
    $('#tbScan').rules("add", {
        notequal: '#tbPallet',
        messages: {
            notequal: 'Pallet to move cannot be same as pallet scanned'
        }
    });
});

// Make sure that user specifies something to update
$.validator.addMethod("updatable", function (value, element) {
    var allnull = true;
    $('.update-rule').each(function () {
        if ($(this).val()) {
            allnull = false;
            return false;    // exit the each loop
        }
    });
    return !allnull;
}, "Please specify something to update or move");

$.validator.addMethod("notequal", function (value, element, param) {
    var other = $(param).val();
    if (other && value && other == value) {
        return false;
    }
    return true;
});



//$Id$
