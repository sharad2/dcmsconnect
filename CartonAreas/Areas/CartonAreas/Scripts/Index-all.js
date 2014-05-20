$(document).ready(function () {
    $('.btPalletLimit,.tbPalletLimit').hide();
    $('div.boxContent').on('click', 'a.editPalletLimit', function () {
       
        var $item = $(this).closest('div');
        $item.find('strong.oldPalletValue').hide();
            $(this).hide();         
            $item.find('input.tbPalletLimit').show();
            $item.find('button.btPalletLimit').show().button();
        });

});