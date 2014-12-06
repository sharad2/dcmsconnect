$(document).ready(function () {
    $('#btnAplyForFilter').button();
});
/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />
/// <reference path="../../../Scripts/jquery.validate-vsdoc.js" />
// $Id: AutoComplete.partial.js 24597 2014-05-30 09:31:49Z ssinghal $

/*
Generic autocomplete script to be used in conjunction with autocomplete helpers
*/

(function ($) {
    // Static variable which keeps track of the number of autocomplete widgets created.
    //var __count = 0;
    $.widget("ui.autocompleteEx", $.ui.autocomplete, {

        widgetEventPrefix: 'autocomplete',

        // Called once when the autocomplete is associated with the input element
        _create: function () {
            // Default options
            var self = this;
            this.options.source = function (request, response) {
                $.ajax({
                    url: self.element.attr('data-ac-list-url'),
                    dataType: 'json',
                    data: { term: request.term, extra: self.element.attr('data-ac-extra-param') },
                    success: function (data, textStatus, jqXHR) {
                        response(data);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        // Error encountered during the remote call. Just show some diagnostic. If this happens, system becomes unstable
                        alert(jqXHR.responseText);
                    }
                });
            };
            this.options.autoFocus = false;
            this.options.delay = 2000;
            this.options.minLength = 2;

            // Call base class
            $.ui.autocomplete.prototype._create.apply(this, arguments);


            this.element.dblclick(function (e) {
                // Double clicking will unconditionally open the drop down
                var oldMinLength = self.options.minLength;
                self.options.minLength = 0;
                self.search();
                self.options.minLength = oldMinLength;
            });
            var valUrl = self.element.attr('data-ac-validate-url');
            if (valUrl) {
                this.element.rules('add', {
                    // remote validation of scanned bar code
                    remote: {
                        url: self.element.attr('data-ac-validate-url'),
                        //context: self,

                        // We need to immediately know whether the input is valid so that the form can be reliably posted
                        async: false,

                        dataType: 'json',
                        beforeSend: function (jqXHR, settings) {
                            self.element.addClass('ui-autocomplete-loading');
                        },
                        dataFilter: function (data, type) {
                            // Grab the chance to update textbox, hidden field and description
                            var json = $.parseJSON(data);
                            if ($.isPlainObject(json)) {
                                // Success. data is the autocomplete object
                                self._selectValue(json);
                                return JSON.stringify(true);
                            } else {
                                // Failure. data is the error message. Clear previous description if any.
                                var x = $.validator.format("span[data-ac-msg-for='{0}']", self.element.attr('name'));
                                $(x).empty();
                                return data;
                            }
                        },
                        complete: function (jqXHR, settings) {
                            self.element.removeClass('ui-autocomplete-loading');
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            // Error encountered during the remote call. Just show some diagnostic. If this happens, system becomes unstable
                            alert(jqXHR.responseText);
                        }
                    }
                });
            }
        },

        /* Special handle the select event. */
        _trigger: function (type, event, ui) {
            var b = $.ui.autocomplete.prototype._trigger.apply(this, arguments);
            switch (type) {
                case 'select':
                    this._selectValue(ui.item);
                    // Prevent the remote validator from trying to validate this value
                    var validator = this.element.closest('form').validate();
                    var prev = validator.previousValue(this.element[0]);
                    prev.old = ui.item.shortName;
                    prev.valid = true;
                    return false;
                    break;

            }
            return b;
        },

        /************** Private functions ******************/
        _selectValue: function (data) {
            // Textbox gets the short name
            this.element.val(data.shortName || data.value).removeClass('input-validation-error');

            // Hidden field just next to it gets the value
            this.element.prev('input:hidden').val(data.value);

            var name = this.element.attr('name');

            // Description span gets label
            var x = $.validator.format("span[data-ac-msg-for='{0}']", name);
            $(x).html(data.label);

            // Get rid of validation error
            x = $.validator.format("span[data-valmsg-for='{0}']", name);
            $(x).removeClass('field-validation-error').addClass('field-validation-valid');
        },

        /****************** Public functions *********************/
        // Clears the value in the control. The hidden field, description and error messages are cleared as well
        clear: function () {
            this._selectValue({ value: '', label: '', shortName: '' });

            // Make sure remote validator will perform the query again
            var validator = this.element.closest('form').validate();
            var prev = validator.previousValue(this.element[0]);
            prev.old = null;
            prev.valid = true;

        }
    })
})(jQuery);

$.validator.setDefaults({
    onkeyup: false,    // remote validation cannot tolerate validation on every keystroke
    onfocusout: false
});

$(document).ready(function () {
    $("input[data-ac-list-url]").autocompleteEx();
});



