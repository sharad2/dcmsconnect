
// Generic handling for AJAX loaded modal
/*
 * tabModal is supposed to be invoiked via a button click. The button must have data-action-url attribute specified. This is the URL from which
 * the contents of the modal will be loaded.
 *  If the loaded content has a form, it will be prepared by calling validateBootstrap() on the form.
 * 
 * If the loaded content has in input with class typeahead and data-typeahead-url attribute specified, then twitter typeahead will be associated with the input after
 *   the modal is shown. When the user makes a selection from the suggestion list, then the selected value is copied to the input whose selector is specified in
 *   the attribute data-typeahead-hf.
 *   Example: 
 *             @Html.HiddenFor(m => m.CustomerId, new
   {
       id = "hfCustomer"
   })
       ...
 *     <input placeholder="Search Customer"
                               class="form-control typeahead" data-typeahead-url="@Url.Action(MVC_PickWaves.PickWaves.Config.CustomerAutocomplete("~"))" data-typeahead-hf="#hfCustomer" />
 */
$(document).ready(function () {
    "use strict";
    $('#tabModal').on('show.bs.modal', function (e) {
        // Load modal content before the modal is shown
        $.ajax({
            url: $(e.relatedTarget).data('action-url'),
            type: 'get',
            cache: false
        }).done(function (data, textStatus, jqXHR) {
            $('.modal-content', this.modal).html(data)
                .find('form').validateBootstrap(true);
        }.bind({ modal: e.delegateTarget })).fail(function (jqXHR, textStatus, errorThrown) {
            //alert(jqXHR.responseText);
            var $div = $('<div></div>').addClass('bg-danger').text(jqXHR.responseText);
            $('.modal-content', this.modal).html($div);
        }.bind({ modal: e.delegateTarget })).done(function (data, textStatus, jqXHR) {
            $('form', this.modal).validateBootstrap(true);
        }.bind({ modal: e.delegateTarget })).done(function (data, textStatus, jqXHR) {
            // Associate typeaahead
            $('input.typeahead[data-typeahead-url]', this.modal).each(function (index, elem) {
                //alert($(elem).attr('data-typeahead-url'));
                $(elem).typeahead(null, {
                    displayKey: 'label',
                    source: function (query, cb) {
                        $.get(this.url.replace('~', query)).done(function (data, textStatus, jqXHR) {
                            this.cb(data);
                        }.bind({ cb: cb })).fail(function (jqXHR, textStatus, errorThrown) {
                            if (jqXHR.status == 500) {
                                this.cb([{ label: 'Error ' + (jqXHR.responseText || errorThrown), value: '' }]);
                            } else {
                                this.cb([{ label: 'Http Error ' + jqXHR.status + ': ' + errorThrown + ' ' + this.url, value: '' }]);
                            }
                        }.bind({ cb: cb, url: this.url }));
                    }.bind({ url: $(elem).attr('data-typeahead-url') }),
                    templates: {
                        empty: 'No matching customers found'
                    }
                }).on('typeahead:selected typeahead:autocompleted', function (e, sug, ds) {
                    // Store the id of the selected customers in the hidden field
                    //alert($(this).attr('data-typeahead-hf'));
                    var hf = $(this).attr('data-typeahead-hf');
                    $(hf).val(sug.value);
                }).on('input', function (e) {
                    // When user changes the customers, empty the hidden field
                    //alert($(this).attr('data-typeahead-hf'));
                    var hf = $(this).attr('data-typeahead-hf');
                    $(hf).val('');
                });
            });
        }.bind({ modal: e.delegateTarget }));
    }).on('click', 'button:not([data-dismiss])', function (e) {
        //finding the form to be post
        $('form', e.delegateTarget).submit();
    });
});
