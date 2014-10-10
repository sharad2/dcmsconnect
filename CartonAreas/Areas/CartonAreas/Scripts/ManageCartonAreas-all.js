/// <reference1 path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

$(document).ready(function () {
    //$('button.mca-unassign').button({ text: false, icons: { primary: 'ui-icon-close' } });
    //$('button.mca-assign').button({ text: false, icons: { primary: 'ui-icon-pencil' } });

    $('#btnAplyForFilter').button();
    // The dialog must be passed $tr (current row which has been clicked) in the custom option currentRow
    $('#divEditDialog').dialog({
        autoOpen: false,
        width: 'auto',
        modal: true,
        closeOnEscape: true,
        open: function (event, ui) {
            // Copy the values in the current to the dialog's input boxes
            var $tr = $(this).dialog('option', 'currentRow');
            var vwh = $('span.mca-vwh', $tr).text().trim();

            $('#lblSku em, span.sku-display', this).text($('span.mca-sku', $tr).text().trim());
            $('#lblAssignedVwh em', this).text(vwh);

            var capacity = $('span.mca-maxassignedcartons', $tr).text().trim();
            $('#lblMaxAssignedCarton em', this).text(capacity);
            $('#tbMaxAssignedCarton').val(capacity);

            $(this).dialog({ title: 'Assign SKU to Location #' + $tr.attr('data-location-id') });
            $(this).find('#tbMaxAssignedCarton').removeClass('input-validation-error');
            $("#displayCartonCount span", this).text($('span.mca-cartoncount', $tr).text());
            $('#tbSku').val($tr.attr('data-upc-code'));
            $('#tbAssignedVwh').val(vwh);

            $('#hfCurrentLocationId', this).val($tr.attr('data-location-id'));

            // Clear the validation error classes. If the user closed the dialog after receiving validation errors, this code will clean up all that.
            $('div[data-valmsg-summary]', this).removeClass('validation-summary-errors').addClass('validation-summary-valid');
            $('input,select', this).removeClass('input-validation-error');
        },
        buttons: [
            {
                id: 'btnUpdate',
                text: 'Update',
                icons: { primary: "ui-icon-disk" },
                click: function (event, ui) {
                    var $form = $('form', this);
                    if (!$form.valid()) {
                        // Do nothing if the form is invalid
                        return false;
                    }

                    // Make the ajax call to update the SKU assignment
                    $('#imgAjaxLoader', this).show();
                    $.ajax($form.attr('action'), {
                        type: 'POST',
                        data: $form.serializeArray(),
                        context: this
                    }).done(function (data, textStatus, jqXHR) {
                        // Update the location count matrix
                        $('#divupdatefilter').html(data);
                        // Success. Update the SKU assignemnt in the current row
                        var $row = $(this).dialog('option', 'currentRow').addClass('ui-state-active');
                        $('span.mca-sku', $row).text($('span.sku-display', this).text());
                        $('span.mca-maxassignedcartons', $row).text($('#tbMaxAssignedCarton', this).val());
                        $('span.mca-vwh', $row).text($('#tbAssignedVwh', this).val());
                        //$('button.mca-unassign', $row).button('enable');
                        $('button.mca-unassign', $row).prop('disabled', false).removeClass('ui-state-disabled');
                        $(this).dialog('close');
                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        alert(jqXHR.responseText);
                    }).always(function () {
                        $(this).dialog('option', 'currentRow').removeClass('ui-state-highlight');
                        $('#imgAjaxLoader', this).hide();
                    });
                }
            },
            {
                text: 'Cancel',
                click: function (event, ui) {
                    var $tr = $(this).dialog('option', 'currentRow').removeClass('ui-state-highlight');
                    $(this).dialog('close');
                }
            }
        ]
    });

    $('#divLocationList').on('click', 'button.mca-unassign', function (e) {
        // Unassign SKU
        var $tr = $(this).closest('tr').addClass('ui-state-highlight');
        var locationId = $tr.attr('data-location-id');
        if (!confirm("Assigned SKU from location " + locationId + " will be removed. Are you sure?")) {
            $tr.removeClass('ui-state-highlight');
            return false;
        }

        $.ajax($(this).attr('data-unassign-url'), {
            type: 'POST',
            context: { row: $tr, button: $(this) }
        }).done(function (data, textStatus, jqXHR) {
            $('#divupdatefilter').html(data);
            this.row.addClass('ui-state-active')
                .find('span.mca-sku,span.mca-maxassignedcartons,span.mca-vwh')
                .empty();
            //this.button.button('disable');
            this.button.prop('disabled', true).addClass('ui-state-disabled');
        }).fail(function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR.responseText);
        }).always(function () {
            this.row.removeClass('ui-state-highlight');
        });
    }).on('click', 'button.mca-assign', function (e) {
        var $tr = $(this).closest('tr').addClass('ui-state-highlight');
        $('#divEditDialog')
            .dialog('option', 'currentRow', $tr)
            .dialog('open');
    });
});

/*
$Id: ManageCartonAreas.partial.js 24663 2014-06-03 09:46:17Z spandey $ 
$Revision: 24663 $
$URL: http://server/svn/DcmsConnect/Projects/Mvc/DcmsMobile.CartonAreas/trunk/CartonAreas/Areas/CartonAreas/Scripts/ManageCartonAreas.partial.js $
$Header: http://server/svn/DcmsConnect/Projects/Mvc/DcmsMobile.CartonAreas/trunk/CartonAreas/Areas/CartonAreas/Scripts/ManageCartonAreas.partial.js 24663 2014-06-03 09:46:17Z spandey $
$Author: spandey $
$Date: 2014-06-03 02:46:17 -0700 (Tue, 03 Jun 2014) $
*/
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



