$(document).ready(function () {
    $('button').button();

    $('#btnSelectAll').click(function (e) {
        var $tr = $('tbody tr.ui-selectee');
        if ($(this).is(':checked')) {
            $tr.addClass('ui-selected')
                .find('input:checkbox').prop('checked', true);
        } else {
            $tr.removeClass('ui-selected')
                .find('input:checkbox').removeAttr('checked');
        }
    });


    // Handle selectable event on selected orders.
    $('#pickslipListBody').selectable({
        filter: 'tr',
        cancel: 'a',
        stop: function (event, ui) {
            $('tr', this).each(function () {
                //Check the hidden check boxes for each row selected by user.
                if ($(this).is('.ui-selected')) {
                    $('input:checkbox', this).prop('checked', true);
                } else {
                    $('input:checkbox', this).removeAttr('checked');
                }
            });
        }
    });
});