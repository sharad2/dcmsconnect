$(document).ready(function () {

    //shows the "Go" and "Cancel" button for clicking the "Edit" option of the Pallet Limit and hide Edit.
    $('div.boxContent').on('click', 'a.editPalletLimit', function () {
        var $item = $(this).closest('div');
        $item.find('strong.palletLimit').hide();
        $(this).hide();
        $item.find('input.tbPalletLimit').show();
        $item.find('button.btPalletLimit').show().button();
        $item.find('a.cancelEdit').show();
    });
    //Comes with previous option of Edit for Pallet Limit Edit option.
    $('div.boxContent').on('click', 'a.cancelEdit', function () {
        var $item = $(this).closest('div');
        $('.cancelEdit').hide();
        $item.find('input.tbPalletLimit').hide();
        $item.find('button.btPalletLimit').hide();
        $item.find('strong.palletLimit').show();
        $item.find('a.editPalletLimit').show();

    });



    //Open the Dialogue box for Edit Address and Insert default 
    //values of the corresponding fields in the input fields.
    $('a.editAddressDialogOpen').click(function (e) {
        var $item = $(this).closest('div');
        $('#hfGetBuildingId').val($item.find('.hfPassBuildingId').val());
        $('#tbAddress').val($item.find('.hfPassAddress').val().split(',').join('\n'));
        $('#tbCity').val($item.find('.hfPassCity').val());
        $('#tbState').val($item.find('.hfPassState').val());
        $('#tbZipCode').val($item.find('.hfPassZipCode').val());
        $('#dglEditAddress').dialog('open');
    });


    //Edit Address dialogue box.
    $('#dglEditAddress').dialog({
        width: 'auto',
        modal: true,
        autoOpen: false,
        closeOnEscape: true,
        open: function (event, ui) {
            var $item = $('a.editAddressDialogOpen').closest('div');          
            var BuildingId = $('#hfGetBuildingId').val();
            //Shows Title of the Dialogue box
            $(this).dialog({ title: 'Edit Address : ' + BuildingId });
        },
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

    //Handel the maximum rows(4) and cols(40) for the text area field for address.
    var textArea = $('#tbAddress, #tbAddAddress');
    var maxRows = textArea.attr('rows');
    var maxChars = textArea.attr('cols');
    textArea.keypress(function (e) {
        var text = textArea.val();
        var lines = text.split('\n');
        //max rows=4 
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



    




    //Open Dialogue box for Adding new building.

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
            $('#divErrorLog').html(''); 
            emptyInputOnLoad = $("#dglAddBuilding").html();
        },
        close : function(event, ui) {
            $("#dglAddBuilding").html(emptyInputOnLoad);
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
