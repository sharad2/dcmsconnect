

$(document).ready(function () {
    "use strict";
    $('#tabModal').on('show.bs.modal', function (e) {
        // Load modal content before the modal is shown
        $.ajax({
            url: $(e.relatedTarget).data('action-url'),
            type: 'get',
            cache: false
        }).done(function (data, textStatus, jqXHR) {
            $('.modal-content', $(this.modal)).html(data)
                .find('form').validateBootstrap(true);
        }.bind({ modal: e.delegateTarget })).fail(function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR.responseText);
        });
    }).on('click', 'button:not([data-dismiss])', function (e) {
        //finding the form to be post
        $('form', e.delegateTarget).submit();
    });
});
