/// <reference path="~/Scripts/jquery-1.6.2-vsdoc.js" />

//Validates destination Pallet. Treats enter as tab
$(document).ready(function () {
    $('#tbPallet').change(function (e) {
        if (!$(this).closest('form').validate().element($(this))) {
            return false;
        }
        $.ajax({
            url: $(this).attr('data-validate-url'),
            data: { palletId: $(this).val() },
            context: this,
            type: 'GET',
            cache: false,
            success: function (data, textStatus, jqXHR) {
                // Success. Display data as status
                PlaySound('success');
                $('#spnPalletInfo').html("<br/>" + data);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR.responseText);
            }
        });
    });
});
