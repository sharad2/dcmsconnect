/// <reference1 path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

$(document).ready(function () {
    //$('#btnLocation,#btnAssignedSku').button({ icons: { primary: 'ui-icon-circle-check'} });
    $('button.mca-unassign').each(function () {
        $(this).button({ text: false, disabled: $(this).is('[disabled]'), icons: { primary: 'ui-icon-close' } });
    });
    $('button.mca-assign').each(function () {
        $(this).button({ text: false, icons: { primary: 'ui-icon-pencil' } });
    });

    // The dialog must be passed $tr (current row which has been clicked) in the custom option currentRow
    $('#divEditDialog').dialog({
        autoOpen: false,
        width: 'auto',
        modal: true,
        closeOnEscape: true,
        // Clear existing values
        open: function (event, ui) {            
            var $tr = $(this).dialog('option', 'currentRow');
            var locationId = $tr.find('span.mca-locationid').html();
            var sku = $tr.find('span.mca-sku').text().replace(/\s+/g, '');
            if (sku == "") {
                sku = "NONE";
            }
            var cartonSku = $tr.find('span.mca-ctnSku').text();
            if (cartonSku == "") {
                cartonSku = "NONE";
            }
            var vwh = $tr.find('span.mca-vwh').text().replace(/\s+/g, '');
            if (vwh == "") {
                vwh = "NONE";
            }
            var cartonCount = $tr.find('span.mca-cartoncount').html();
            $(this).dialog({ title: 'Update Location #' + locationId });
            $(this).find('#tbMaxAssignedCarton').removeClass('input-validation-error');
            $("#displayCartonCount").html("<b>Location contains " + cartonCount + " cartons of SKU " + cartonSku + ". </b>").addClass('ui-state-highlight');
            $('#displayWarning', this).html("Assigned SKU: (" + sku + ") | VWh: (" + vwh)
                .addClass('ui-state-highlight');
            if (sku != "") {
                var upccode = $tr.find('span.mca-sku span').attr('title');
                $('#tbSku').val(upccode);
                $('span.spnDisplaySku').html($tr.find('span.mca-sku').text().replace(/\s+/g, ''));
            }
            $('#tbAssignedVwh').val(vwh).attr('selected', true);
            $('#tbMaxAssignedCarton').val($tr.find('span.mca-maxassignedcartons').text().replace(/\s+/g, ''));
            $('#hfCurrentLocationId', this).val(locationId);
            $('#ajaxErrors', this).empty();
            $('div[data-valmsg-summary]', this).removeClass('validation-summary-errors').addClass('validation-summary-valid');
            $("#btnUpdate").button({ icons: { primary: "ui-icon-disk" } });
        },
        buttons: [
            {
                id: 'btnUpdate',
                text: 'Update',
                click: function (event, ui) {
                    var $form = $('form', this);
                    if (!$form.valid()) {
                        $("#frmEditLocation input[data-ac-list-url]").autocompleteEx('clear');
                        $('input:text', this).val('');
                        return false;
                    }
                    if (!$('#tbSku').val()) {
                        $("#frmEditLocation input[data-ac-list-url]").autocompleteEx('clear');
                    }
                    var dialogData = $form.serializeArray();
                    $.ajax({
                        url: $form.attr('action'),
                        type: 'POST',
                        data: dialogData,
                        context: this,
                        statusCode: {
                            // Success
                            200: function (data, textStatus, jqXHR) {
                                // Update areaInfo table.
                                $('#divupdatefilter').html(data);
                                // update location list.
                                var $row = $(this).dialog('option', 'currentRow');
                                var cartonCount = parseInt($('span.mca-cartoncount', $row).html());
                                var value = parseInt($('#tbMaxAssignedCarton', this).val());
                                var maxAssignedCartons = isNaN(value) ? null : value;
                                var pct = Math.min((cartonCount * 100) / maxAssignedCartons, 100);
                                $row.removeClass('ui-state-highlight')
                                    .addClass('ui-state-active')
                                    .find('span.mca-sku')
                                    .html('<span title=' + $('#tbSku').val() + '>' + $('span.spnDisplaySku', this).html() + '</span>')
                                    .end()
                                    .find('span.mca-vwh')
                                    .html($('#tbAssignedVwh', this).val())
                                    .end()
                                    .find('span.mca-maxassignedcartons')
                                    .html(maxAssignedCartons)
                                    .end()
                                    .find('div.ui-progressbar-value')
                                    .width(pct + '%')
                                    .toggleClass('ui-state-error', (maxAssignedCartons == null ? 0 : maxAssignedCartons) < cartonCount)
                                    .end()
                                    .find('button.mca-unassign')
                                    .button('enable');
                                $(this).dialog('close');
                                $('input:text', this).val('');
                                $("#frmEditLocation input[data-ac-list-url]").autocompleteEx('clear');
                            },
                            // Error
                            500: function (jqXHR, textStatus, errorThrown) {
                                $('div[data-valmsg-summary]', this).html(jqXHR.responseText)
                                    .removeClass('validation-summary-valid')
                                    .addClass('validation-summary-errors');
                                $('span.spnDisplaySku,#ajaxErrors', this).empty();
                                $('input:text', this).val('');
                                $("#frmEditLocation input[data-ac-list-url]").autocompleteEx('clear');
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            alert(jqXHR.responseText);
                        }
                    });
                }
            },
            {
                text: 'Close',
                click: function (event, ui) {
                    $(this).dialog('close');
                }
            }
        ]
    });
    // Table row gets ui-state-highlight when the dialog is opened. It gets ui-state-active after a successful assignment/unassignment is performed.
    // Only one row can have this state
    $('#divLocationList').click(function (e) {
        $('table tr', this).removeClass('ui-state-highlight').removeClass('ui-state-active');
        var $tr = $(e.target).closest('tr');
        if ($(e.target).is('button.mca-assign')) {
            $('#divEditDialog')
            .dialog('option', 'currentRow', $tr)
            .dialog('open');
        }

        // Unassign SKU
        var locationId = $tr.find('span.mca-locationid').html();
        // areaId is use for update areaInfo.
        var areaId = $('#hfAreaId').val();
        if ($(e.target).is('button.mca-unassign:not([disabled])') && confirm("Assigned SKU from location " + locationId + " will be removed. Are you sure?")) {
            $.ajax({
                url: $('#divEditDialog').attr('data-unassign-url'),
                type: 'POST',
                data: { locationId: locationId, areaId: areaId },
                statusCode: {
                    // Success
                    200: function (data, textStatus, jqXHR) {
                        // update areaInfo table.
                        $('#divupdatefilter').html(data);
                        //Clear the fields which like SKU and capacity in grid row
                        $tr.find('span.mca-sku,span.mca-maxassignedcartons,span.mca-vwh')
                            .empty()
                            .end()
                             .find('div.ui-progressbar-value')
                            .css('width', '0%').removeClass('ui-state-error')
                             .end()
                            .removeClass('ui-state-highlight')
                            .addClass('ui-state-active')
                            .find('button.mca-unassign')
                            .button('disable');
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert(jqXHR.responseText);
                }
            });
        }
    });
});

/*
$Id: ManageCartonAreas.partial.js 24559 2014-05-27 07:57:27Z spandey $ 
$Revision: 24559 $
$URL: http://server.eclipse.com/svn/dcmsconnect/Projects/Mvc/DcmsMobile.CartonAreas/trunk/CartonAreas/Areas/CartonAreas/Scripts/ManageCartonAreas.partial.js $
$Header: http://server.eclipse.com/svn/dcmsconnect/Projects/Mvc/DcmsMobile.CartonAreas/trunk/CartonAreas/Areas/CartonAreas/Scripts/ManageCartonAreas.partial.js 24559 2014-05-27 07:57:27Z spandey $
$Author: spandey $
$Date: 2014-05-27 13:27:27 +0530 (Tue, 27 May 2014) $
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



