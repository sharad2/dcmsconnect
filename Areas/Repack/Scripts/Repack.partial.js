/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />
/// <reference path="../../../Scripts/jquery.validate-vsdoc.js" />

$.validator.setDefaults({ onkeyup: false });
$(document).ready(function () {
    // Triggering the change event at startup to update the state of the carton id text box
    $('#cbScanCarton').change(function (e) {
        var checked = $(this).is(':checked');
        $('#divCartonId').toggle(checked);
        $('#divNoOfCartons').toggle(!checked);
        if (!checked) {
            $('#tbCartonId').val('');
        }
    }).change();
    $('#cbConversion').each(function () {
        $(this).change(function (e) {
            var checked = $(this).is(':checked');
            $('#divConversion').toggle(checked);
        }).change();
    });
});

// Pallet is required only if destination area requires pallet
$(document).ready(function () {
    // Enable/disable pallet depending on whether destination area required pallet
    $('#ddlDestinationArea').change(function (e) {
        var g = $('optgroup:has(option[value="' + $(this).val() + '"])', this).attr('label');
        // Show div for pallet area, else hide
        if (g) {
            if (g.indexOf('P') == 0) {
                $("#tbPalletId").rules("add", {
                    required: true,
                    messages: {
                        required: "Pallet is required for this area",
                    }
                });
                $('sup.ui-helper-hidden').show();
            } else {
                $("#tbPalletId").rules("remove", 'required');
                $('sup.ui-helper-hidden').hide();
            }
            $("span[for='tbPalletId']").hide();
            $('#tbPalletId').removeClass('input-validation-error');
        }
    });
});

// When enter is pressed on UPC code, focus pieces if entered UPC is valid
//$(document).ready(function () {
//    $('#tbUpcScan').keydown(function (e) {
//        if (e.keyCode === $.ui.keyCode.ENTER) {
//            $(this).valid();   // Ask remote validator to validate and then check whether entry was valid
//            if ($(this).valid()) {
//                setTimeout(function () {
//                    // Due to sounds, we must set the focus after minimal delay
//                    $('#tbPieces').focus();
//                }, 0);
//            }
//            return false;
//        }
//    });

//    $('#tbCartonId').keydown(function (e) {
//        if (e.keyCode === $.ui.keyCode.ENTER) {
//            setTimeout(function () {
//                // Due to sounds, we must set the focus after minimal delay
//                $('#tbUpcScan').focus();
//            }, 0);
//            return false;
//        }
//    });

   
   
//});


//$Id$
