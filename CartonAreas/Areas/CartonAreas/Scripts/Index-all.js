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
});
