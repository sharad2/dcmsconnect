$(document).ready(function () {
    $('#btnSearch').button({ icons: { primary: 'ui-icon-search' } }).on('click', function (e) {
        var $form = $(this).closest('form');
        $.ajax({
            url: $form.attr('action'),
            data: $form.serializeArray(),
            type: 'GET',
            cache: false,
            statusCode: {
                200: function (data, textStatus, jqXHR) {
                    //alert('200 ' + data);
                    window.location.href = data;
                    return true;
                },
                203: function (data, textStatus, jqXHR) {
                    $('#layoutError').text(data).show();
                }
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR.responseText);
        });
        return false;
    });
});
