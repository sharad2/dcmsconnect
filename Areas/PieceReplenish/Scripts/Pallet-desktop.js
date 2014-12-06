/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />


$(document).ready(function () {
    $('#tbl').click(function (e) {
        if ($(e.target).is('a.set-priority')) {
            var $tr = $(e.target).closest('tr');
            if (!$tr.is('.ui-state-disabled')) {
                ajaxSetPriority($(e.target).attr('href'), $tr);
            }
            return false;
        }
    });

    function ajaxSetPriority(url, $tr) {
        return $.ajax({
            url: url,
            type: 'POST',
            statusCode: {
                // Success
                202: function (data, textStatus, jqXHR) {
                    $('#spanMessage').html(data).show();
                    if ($tr) {
                        $tr.addClass('ui-state-disabled');
                        $('#spanMessage').position({ my: 'left top', at: 'left top', of: $tr }).delay(2000).hide(2000);
                        $('#divPriorityAlert').show();

                    }
                },
                200: function (data, textStatus, jqXHR) {
                    alert(data);
                    alert("Refreshing the page will solve this problem.")
                },
                203: function (data, textStatus, jqXHR) {
                    alert(data);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR.responseText);
            }
        });
    }
});