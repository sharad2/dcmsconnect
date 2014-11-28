$(document).ready(function () {
	"use strict";
	$('#tbSkuId').typeahead(null, {
		name: 'skus',
		displayKey: 'label',
		source: function (query, cb) {
			var url = _autocompleteUrl.replace('~', query);

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
			empty: 'No matching SKUs found'
		}
	}).on('typeahead:selected typeahead:autocompleted', function (e, sug, ds) {
		// Store the id of the selected skus in the hdden field
		$('#hfSkuId').val(sug.value);
		//will trigger submit on selection in the drop down of autocomplete.
		$(e.delegateTarget).closest('form').trigger('submit');
	}).on('input', function (e) {
		// When user changes the skus, empty the hidden field
		$('#hfSkuId').val('');
	});

});