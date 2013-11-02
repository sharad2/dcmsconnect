/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />
$(document).ready(function () {
    $('ul.actionMenu').menu();
    // Creating menu to edit, create, delete request.
    $('ul.actionMenu').menu({
        select: function (event, ui) {
            var resvid = $('a', ui.item).attr('data-resvid');
            if ($('a', ui.item).attr('data-delete-request')) {
                var result = confirm("Are you sure, you want to delete Request# " + resvid + " ?");
                if (result) {
                    $('a', ui.item).attr('href');
                    return;
                }
                //Prevent link from posting to an action on click cancel button.
                event.preventDefault();
                return;
            }
        }
    });
});
$('#tbody').on('mouseleave', 'ul.actionMenu', function (e) {
    // Close the menu if user navigates out of it
    $(this).menu('collapseAll', e, true);
});

$('#btnGo').button({ icons: { secondary: 'ui-icon-search' } }).click(function (e) {
    var RE = /^\d+$/;    //Regular expression to validate the number for ctnresvId 
    var id = $('#frmSearch input:text').val();
    //validating the ctnresvId
    if (id == '' || !RE.test(id)) {
        $('#divError').html("Please enter the valid Request ID")
                           .removeClass('validation-summary-valid')
                            .addClass('validation-summary-errors');
        return false;
    }
});