$(document).ready(function () {

    //Make submit type button look better. .
    $('button:submit').button();
    $('#btnSelectCustomer').button();

    // Search Po, redirect to UI in which PO exist.
    $('#btnSearchPo').on('click', function (e) {
        $('#poErrorMessage').hide();
        var val = $('#tbPoId').val();
        if (!val) {
            $('#poErrorMessage').show().html('Enter something');
            return;
        }

        $.ajax({
            url: $(this).attr('data-search-url').replace('~', val),
            cache: false
        }).done(function (data, textStatus, jqXHR) {
            if (data) {
                window.location = data;
            } else {
                $('#poErrorMessage').show().html('Invalid PO');
            }
        }).error(function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR.responseText);
        }).always(function () {
            $('#tbPoId').val('')
        });
    });
    // Setup autocomplete for customer
    $("#tbSelectCustomer").autocomplete({
        minLength: 0,
        source: function (request, response) {
            $.ajax({
                url: this.element.attr('data-list-url'),
                dataType: 'json',
                context: this.element,
                beforeSend: function (jqXHR, settings) {
                    this.addClass('ui-autocomplete-loading');
                },
                data: { term: request.term },
                success: function (data, textStatus, jqXHR) {
                    response(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    // Error encountered during the remote call. Just show some diagnostic. If this happens, system becomes unstable
                    alert(jqXHR.responseText);
                },
                complete: function (jqXHR, textStatus) {
                    this.removeClass('ui-autocomplete-loading');
                }
            });
        },
        change: function (event, ui) {
            $(this).change();
        },
        select: function (event, ui) {       
            $(this).val(ui.item.value);
            $('#btnSelectCustomer').click();
        },
        autoFocus: true
    }).dblclick(function (e) {
        $(this).autocomplete('search');
    }).focus();
    $('#layoutTabMenu').menu();
});


