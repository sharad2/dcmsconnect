$(document).ready(function () {
    "use strict";
    // Handle priority up and down buttons
    $(document).on('click', 'button[data-priority-url]', function (e) {
        $.post($(e.target).data('priority-url')).done(function (data, textStatus, jqXHR) {
           // alert(data);
            $(e.target).closest('div.input-group').find('input:text').val(data);
        }).error(function () {
            alert('error');
        });
    });
});

