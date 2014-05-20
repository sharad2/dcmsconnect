$(document).ready(function () {   
    $('div.boxContent').on('click', 'a.editPalletLimit', function () {
        var $item = $(this).closest('div');
        $item.find('strong.palletLimit').hide();
        $(this).hide();
        $item.find('input.tbPalletLimit').show();
        $item.find('button.btPalletLimit').show().button();
    });
});
