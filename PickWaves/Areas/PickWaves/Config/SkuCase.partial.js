
// Generic handling for AJAX loaded modal
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
            //alert(jqXHR.responseText);
            var $div = $('<div></div>').addClass('bg-danger').text(jqXHR.responseText);
            $('.modal-content', $(this.modal)).html($div);
        }.bind({ modal: e.delegateTarget }));
    }).on('click', 'button:not([data-dismiss])', function (e) {
        //finding the form to be post
        $('form', e.delegateTarget).submit();
    });
});

// Customer SKU case Preference editor
$(document).ready(function () {
    $('#tabModal').on('shown.bs.modal', function (e) {
        "use strict";
        // Associate type ahead behavior after dialog is loaded
        $('#tbCustomer').typeahead(null, {
            name: 'customers',
            displayKey: 'label',
            source: function (query, cb) {
                //alert(_customerAutocompleteUrl);
                var url = _customerAutocompleteUrl.replace('~', query);
                $.get(url).done(function (data, textStatus, jqXHR) {
                    this.cb(data);
                }.bind({ cb: cb })).fail(function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status == 500) {
                        this.cb([{ label: 'Error ' + (jqXHR.responseText || errorThrown), value: '' }]);
                    } else {
                        this.cb([{ label: 'Http Error ' + jqXHR.status + ': ' + errorThrown + ' ' + this.url, value: '' }]);
                    }
                }.bind({ cb: cb, url: url }));
            },
            templates: {
                empty: 'No matching customers found'
            }
        }).on('typeahead:selected typeahead:autocompleted', function (e, sug, ds) {
            // Store the id of the selected customers in the hdden field
            $('#hfCustomer').val(sug.value);
        }).on('input', function (e) {
            // When user changes the customers, empty the hidden field
            $('#hfCustomer').val('');
        });
    });



});