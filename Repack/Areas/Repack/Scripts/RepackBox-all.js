
$(document).ready(function () {
    var $frmMain = $('#frmMain');
    var $ajaxError = $('#ajaxErrors');
    var $btnRepackBox = $('#btnRepackBox');
    var $tbBoxscan = $('#tbUcc128Id');
    $tbBoxscan.keypress(function (e) {
        if (e.which == $.ui.keyCode.ENTER) {
            $btnRepackBox.button().click();
        }
    });

    function convertBox() {
        $.ajax({
            url: $frmMain.attr('action'),
            data: $frmMain.serialize(),
            context: this,
            type: 'POST',
            statusCode: {
                // Success. Display data as status
                200: function (data, textStatus, jqXHR) {
                    $tbBoxscan.val('');
                    $tbBoxscan.focus();
                    $ajaxError.html(data).removeClass('field-validation-error').addClass('success-display');
                    $('div[data-valmsg-summary]').removeClass('validation-summary-errors ').addClass('validation-summary-valid');
                },
                203: function (data, textStatus, jqXHR) {
                    // Error
                    $tbBoxscan.val('');
                    $tbBoxscan.focus();
                    $ajaxError.html(data).removeClass('success-display').addClass('field-validation-error');
                    $('div[data-valmsg-summary]').removeClass('validation-summary-valid').addClass('validation-summary-errors');
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR.responseText);
            }
        });
    }
    $btnRepackBox.button().click(function () {
        if (!$frmMain.valid()) {
            return false;
        }
        if (!$tbBoxscan.val()) {
            $ajaxError.html("Box is required.").removeClass('success-display').addClass('field-validation-error');
            return false;
        }
        convertBox();
    });
});
// Pallet is required only if destination area requires pallet
$(document).ready(function () {
    // Enable/disable pallet depending on whether destination area required pallet
    $('#ddlDestinationArea').change(function (e) {
        var g = $('optgroup:has(option[value="' + $(this).val() + '"])', this).attr('label');
        // Show div for pallet area, else hide
        if (g) {
            $('#tbPalletId').toggleClass('required', g.indexOf('P') == 0).val('');
        }
    });
    $('#tbPalletId').keypress(function (e) {
        if (e.which == $.ui.keyCode.ENTER) {
            if ($('#tbPalletId').val()) {
                $('#tbUcc128Id').focus();
            }
        }
    });
});