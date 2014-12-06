﻿/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />
///Script used by Carton Editor.

$(document).ready(function () {
    $('#btnUpdate,#btnReset,#btnGo,#btnRemove,#btnRefresh').button();
    $('#tbIrregular').val('');
    $('#tbSample').val('');
    $('#tbScan').focus();
    $('#cbRemovePallet').removeAttr('checked');




    // Check box to delete empty carton is checked by default.
    $('#cbEmptyCarton').attr('checked', 'checked');

    // Created tabs for editing carton properties and removing irregular, sample pieces.
    $('#divTabs').tabs({
        create: function (event, ui) {
            $(this).tabs('option', 'selected', parseInt($(this).attr('data-selected-index')));
        }
    });


    //Assign value of selected printer id in hidden fileds existing in both forms.
    $('#ddlPrinter').change(function (e) {
        var printerId = $('#ddlPrinter').val();
        $('#hfPrinter, #hfrPrinter').val(printerId);
    });
    // Enable disable the associated radio buttons
    $('#cbqUpdateReworkStatus').change(function (e) {
        var checked = $(this).is(':checked');
        if (checked) {
            $('#rbuCompleteRework,#rbuNotSet').removeAttr('disabled').eq(0).attr('checked', 'checked');
        } else {
            $('#rbuCompleteRework,#rbuNotSet').attr('disabled', 'disabled').removeAttr('checked');
        }
    });

    $('#tbSetPieces').change(function () {
        $('#spPieces').show();
    });
    $('#tbUpdateSKU').change(function () {
        $('#spSku').show();
        // Clear the style,color... info being displayed next to the input box.
        var upc = $(this).val();
        $(this).autocompleteEx('clear');
        $(this).val(upc);
    }).bind('autocompleteselect', function (event, ui) {
        $('#hfSkuDescription').val(ui.item.label);
    });
    $('#ddlUpdateQuality').change(function () {
        $('#spQualityCode').show();
    });
    $('#ddlUpdateVwhId').change(function () {
        $('#spVwh').show();
    });
    $('#ddlPriceSeasonCode').change(function () {
        $('#spSeasonCode').show();
    });

    //Support eneter button on tbScan
    $('#tbScan').keydown(function (e) {
        if (e.keyCode === $.ui.keyCode.ENTER) {
            $(this).closest('form').submit();
        }
    });

    $('#btnReset').click(function () {
        $('#spQualityCode').hide();
        $('#spPieces').hide();
        $('#spVwh').hide();
        $('#spSeasonCode').hide();
        $('#spSku').hide();
        $('#cbRemovePallet').removeAttr('checked');
        $('#tbPallet').val('');
        $('#spnPalletInfo').html('');
    });

    // Allow enter press on tbPallet. Id tbPallet is generated by editor template it self.
    $('#tbPallet').keydown(function (e) {
        if (e.keyCode === $.ui.keyCode.ENTER && $(this).val()) {
            $('#tbUpdateSKU').focus();
            return false;
        }
    });

    $('#cbRemovePallet').change(function (e) {
        var checked = $(this).is(':checked');
        if (checked) {
            $('#tbPallet').attr('disabled', 'disabled');
        }
        else {
            $('#tbPallet').removeAttr('disabled');

        }

    });

}).keydown(function (e) {
    // Don't submit the page when enter is pressed
    if (e.keyCode === $.ui.keyCode.ENTER) {
        e.preventDefault();
    }
});
function PlaySound(file) {
    // alert(file);
    try {
        $('#sound_' + file)[0].play();
    }
    catch (e) {
        // No plugin available? Browser does not support HTML5? Ignore the error
    }
    //    var $embed = $('embed', $sound).removeAttr('autostart').attr('autostart', true);
    //    $sound.children('span:first').html($embed[0].outerHTML);
}


//$Id$
