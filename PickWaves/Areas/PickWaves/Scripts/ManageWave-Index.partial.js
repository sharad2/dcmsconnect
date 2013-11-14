$(document).ready(function () {
    $('ul.waveActionMenu').menu({
        select: function (event, ui) {           
            if (ui.item.is('.increase-action')) {
                ui.item.closest('tr').find('div.bucketModelPartial').bucketmodel('increasePriority');
                return false;
            } if (ui.item.is('.decrease-action')) {
                ui.item.closest('tr').find('div.bucketModelPartial').bucketmodel('decreasePriority');
                return false;
            }            
        }
    }).on('mouseleave', function (e) {
        $(this).menu("collapseAll", null, true);
    });

    $('div.bucketModelPartial').bucketmodel();
});