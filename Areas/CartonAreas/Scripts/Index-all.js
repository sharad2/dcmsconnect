$(document).ready(function () {
    //Pallet limit edit button shows editing option.
    $('div.boxContent').on('click', 'a.editPalletLimit', function () {
        $(this).hide().next().css("display", "inline-block").find('button').button();
    }).on('click', 'a.cancelEdit', function () {
        $(this).parent().css("display", "none").prev().show();
    });
});
