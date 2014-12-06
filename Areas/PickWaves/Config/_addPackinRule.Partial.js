$(document).ready(function () {
    $('#tbStyle').typeahead(null, {
        name: 'styles',
        displayKey: 'label',
        source: function (query, cb) {
            var url = _styleAutocompleteUrl.replace('~', query);
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
            empty: 'No matching styles found'
        }
    }).on('typeahead:selected typeahead:autocompleted', function (e, sug, ds) {
        // Store the id of the selected customers in the hdden field
        $('#hfStyle').val(sug.value);
    }).on('input', function (e) {
        // When user changes the customers, empty the hidden field
        $('#hfStyle').val('');
    });

});