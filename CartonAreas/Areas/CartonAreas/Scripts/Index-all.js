$(document).ready(function () {
    $('div.boxContent').on('click', 'a.editPalletLimit', function () {
        var $item = $(this).closest('div');
        $item.find('strong.palletLimit').hide();
        $(this).hide();
        $item.find('input.tbPalletLimit').show();
        $item.find('button.btPalletLimit').show().button();
        $item.find('a.cancelEdit').show();
    });

    $('div.boxContent').on('click', 'a.cancelEdit', function () {
        var $item = $(this).closest('div');
        $('.cancelEdit').hide();
        $item.find('input.tbPalletLimit').hide();
        $item.find('button.btPalletLimit').hide();
        $item.find('strong.palletLimit').show();
        $item.find('a.editPalletLimit').show();

    });






    $('a.editAddressDialogOpen').click(function (e) {
        var $item = $(this).closest('div');
        $('#hfGetBuildingId').val($item.find('.hfPassBuildingId').val());
        $('#tbAddress').val($item.find('.hfPassAddress').val().split(',').join('\n'));
        $('#tbCity').val($item.find('.hfPassCity').val());
        $('#tbState').val($item.find('.hfPassState').val());
        $('#tbZipCode').val($item.find('.hfPassZipCode').val());
        $('#dglEditAddress').dialog('open');
    });
    $('#dglEditAddress').dialog({
        title: 'Edit Address',
        width: 'auto',
        modal: true,
        autoOpen: false,
        closeOnEscape: true,
        buttons: [
        {
            text: 'Update',
            click: function (event, ui) {
                $('#frmEditAddress').submit();
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
    var textArea = $('#tbAddress');
    var maxRows = textArea.attr('rows');
    var maxChars = textArea.attr('cols');
    textArea.keypress(function (e) {
        var text = textArea.val();
        var lines = text.split('\n');
        if (e.keyCode == 13) {
            return lines.length < maxRows;
        }
        else { //Should check for backspace/del/etc.
            var caret = textArea.get(0).selectionStart;
            var line = 0;
            var charCount = 0;
            $.each(lines, function (i, e) {
                charCount += e.length;
                if (caret <= charCount) {
                    line = i;
                    return false;
                }
                //n count for 1 char;
                charCount += 1;
            });
            return lines[line].length < maxChars;
        }
    });



    






    $('a.addbuildingDialogOpen').click(function (e) {
        $('#dglAddBuilding').dialog('open');       
    });

    $('#dglAddBuilding').dialog({
        title: 'Add Building',
        width: 'auto',
        modal: true,
        autoOpen: false,
        closeOnEscape: true,
        open: function () {
            $('#divErrorLog').html('').removeClass('ui-state-error');
                    
        },
        buttons: [
        {
            text: 'Add',
            click: function (event, ui) {
                var errorMsg = "";                
                if ($('#tbAddBuildingId').val() == "") {
                    errorMsg = errorMsg + "Please Enter Building" + "<br>";
                }
                if ($('#tbAddAddress').val() == "") {
                    errorMsg = errorMsg + "Please Enter Address" + "<br>";
                }
                if ($('#tbAddCity').val() == "") {
                    errorMsg = errorMsg + "Please Enter City" + "<br>";
                }                
                if ($('#tbAddZipCode').val() == "") {
                    errorMsg = errorMsg + "Please Enter ZipCode" + "<br>";
                }
                if (errorMsg != "") {
                    $('#divErrorLog').html(errorMsg).addClass('ui-state-error');
                    return false;
                }
                $('#frmAddBuilding').submit();
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

});
