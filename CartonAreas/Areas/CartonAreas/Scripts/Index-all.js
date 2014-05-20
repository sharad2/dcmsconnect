$(document).ready(function () {
    $('.btPalletLimit,.tbPalletLimit').hide();
    $('div.boxContent').on('click', 'a.editPalletLimit', function () {
        var $item = $(this).closest('div');
        $item.find('strong.palletLimit').hide();
        $(this).hide();
        $item.find('input.tbPalletLimit').show();
        $item.find('button.btPalletLimit').show().button();
    });

    $('button.btPalletLimit').click(function () {
        var $form = $(this).closest('form');
        var $item = $(this).closest('div');
        $.ajax({
            type: "POST",
            url: $form.attr('action'),
            data: $form.serializeArray(),            
            success: function (data, textStatus, jqXHR) {
                // handling validation error
                if (jqXHR.status == 203) {
                    alert(jqXHR.responseText);
                    return;
                }
                //Success
                $item.find('strong.palletLimit').html(data);
                $item.find('strong.palletLimit,a.editPalletLimit').show();
                $item.find('input.tbPalletLimit,button.btPalletLimit').hide();
                return;
            }
        }).error(function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR.responseText);
        });
    });
});
