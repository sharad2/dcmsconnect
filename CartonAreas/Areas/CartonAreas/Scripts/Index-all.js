$(document).ready(function () {
    //shows the "Go" and "Cancel" button for clicking the "Edit" option of the Pallet Limit and hide Edit.
    $('div.boxContent').on('click', 'a.editPalletLimit', function () {
        $(this).hide().next().css("display", "inline-block").find('button').button();
    }).on('click', 'a.cancelEdit', function () {
        $(this).parent().css("display", "none").prev().show();
    });
});
