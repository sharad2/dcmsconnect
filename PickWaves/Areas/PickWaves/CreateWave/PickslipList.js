///#source 1 1 /Areas/PickWaves/SharedViews/_bucketModel.partial.js
$(document).ready(function () {
    "use strict";
    $('#freezeModal').on('show.bs.modal', function (e) {
        //alert($(e.relatedTarget).data('bucketid'));
        $('#hfBucketid', e.delegateTarget).val($(e.relatedTarget).data('bucketid'));
        $('#spanBucketId', e.delegateTarget).text($(e.relatedTarget).data('bucketid'));
        $('#countNotStartedBoxes', e.delegateTarget).html($(e.relatedTarget).data('countnotstartedboxes'));
    }).on('click', 'button:not([data-dismiss])', function (e) {
        //alert('submit');
        $('form', e.delegateTarget).trigger('submit');
    });
    $(document).on('click', 'button[data-priority-url]', function (e) {
    
       // alert($(e.target).data('priority-url'));
        $.post($(e.target).data('priority-url')).done(function (data, textStatus, jqXHR) {
           // alert(data);
            $(e.target).closest('div.input-group').find('input:text').val(data);
        }).error(function () {
            alert('error');
        });
    });
});


