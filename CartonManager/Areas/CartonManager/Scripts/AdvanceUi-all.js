/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

///Script used by advance Ui to perform validation and also to create dialog
$(document).ready(function () {
    //Supports the Enter button on Dialog to submit the Carton or Pallet for update
    $('#tbScan').handlescan({ formMain: '#frmMain' });
    $('#btnUpdate').button().click(function (e) {
        if (!$('#frmMain').valid()) {
            return;
        }
        $('span.field-validation-error').removeClass('field-validation-error').addClass('field-validation-valid');
        $('#divUpdate').dialog('open');
    });

    // Setup the dialog
    $('#divUpdate').dialog({
        title: 'Scan Cartons/Pallet',
        autoOpen: false,
        width: 400,
        modal: true,
        closeOnEscape: false,
        // Clear existing values
        open: function (event, ui) {
            $('#tbScan').handlescan('clearErrors');
            $('#tbConfirm').val('');
        },
        buttons: [
            {
                text: 'Go', click: function (event, ui) {
                    var $tbScan = $('#tbScan');
                    if ($tbScan.val()) {
                        $tbScan.handlescan('scan');
                    }
                    else {
                        $('#ajaxErrors').html('Please scan carton/pallet.');
                        $('#ajaxErrors').show().removeClass('success-display').addClass('field-validation-error');
                        return false;
                    }
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

    // Enable disable the associated radio buttons
    $('#cbqReworkStatus').change(function (e) {
        var checked = $(this).is(':checked');
        if (checked) {
            $('#rbqDoesNotNeedRework,#rbqNeedsRework').removeAttr('disabled').eq(0).attr('checked', 'checked');
        } else {
            $('#rbqDoesNotNeedRework,#rbqNeedsRework').attr('disabled', 'disabled').removeAttr('checked');
        }
    });


    // Enable disable the associated radio buttons
    // Assign value to check box on check uncheck. This is done to implement logic of validating updating rules against their values.
    $('#cbqUpdateReworkStatus').change(function (e) {
        var checked = $(this).is(':checked');
        if (checked) {
            $(this).val('Y');
            $('#rbuCompleteRework,#rbuNotSet').removeAttr('disabled').eq(0).attr('checked', 'checked');
        } else {
            $('#rbuCompleteRework,#rbuNotSet').attr('disabled', 'disabled').removeAttr('checked');
            $(this).val('');
        }
    });

    // Enable disable Pallet TextBox 
    $('#cbremoveFromPallet').change(function (e) {
        var checked = $(this).is(':checked');
        if (checked) {
            $(this).val('true');
            $('#tbPallet').attr('disabled', 'disabled');
        } else {
            $('#tbPallet').removeAttr('disabled');
            $(this).val('');
        }
    });
});

$(document).ready(function () {

    $('form').submit(function (e) {
        // None of the forms should ever be submitted. This page never posts back. It only issues ajax calls.
        // Occasionally, pressing enter causes the form to get submitted.
        return false;
    });
    $('#tbMultiField').rules("add", {
        updatable: true
    });
    $('#tbUpdatePieces').rules("add", {
        notequal: '#tbQualifyPieces',
        messages: {
            notequal: 'Qualifying pieces cannot be same as pieces to update'
        }
    });
    $('#ddlUpdateQuality').rules("add", {
        notequal: '#ddlQualifyingQuality',
        messages: {
            notequal: 'Qualifying quality code cannot be same as quality code to update'
        }
    });
    $('#ddlUpdateVwhId').rules("add", {
        notequal: '#ddlQualifyingVwhId',
        messages: {
            notequal: 'Qualifying virtual warehouse cannot be same as virtual warehouse to update'
        }
    });
    $('#tbUpdateSKU').rules("add", {
        notequal: '#tbQualifyingSKU',
        messages: {
            notequal: 'Qualifying SKU cannot be same as SKU to update'
        }
    });
    $('#ddlPriceSeasonCode').rules("add", {
        notequal: '#ddlQualifyingPriceSeasonCode',
        messages: {
            notequal: 'Qualifying season code cannot be same as season code to update'
        }
    });
    $('#tbScan').rules("add", {
        notequal: '#tbPallet',
        messages: {
            notequal: 'Pallet to move cannot be same as pallet scanned'
        }
    });
});

// Make sure that user specifies something to update
$.validator.addMethod("updatable", function (value, element) {
    var allnull = true;
    $('.update-rule').each(function () {
        if ($(this).val()) {
            allnull = false;
            return false;    // exit the each loop
        }
    });
    return !allnull;
}, "Please specify something to update or move");

$.validator.addMethod("notequal", function (value, element, param) {
    var other = $(param).val();
    if (other && value && other == value) {
        return false;
    }
    return true;
});



//$Id: AdvanceUi.partial.js 12130 2012-06-13 12:08:39Z rverma $

/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />
// $Id: AutoComplete.partial.js 12312 2012-06-21 09:14:20Z bkumar $

/*
Generic autocomplete script to be used in conjunction with autocomplete helpers
*/

(function ($) {
    // Static variable which keeps track of the number of autocomplete widgets created.
    //var __count = 0;
    $.widget("ui.autocompleteEx", $.ui.autocomplete, {

        widgetEventPrefix: 'autocomplete',

        // Called once when the autocomplete is associated with the input element
        _create: function() {
            // Default options
            var self = this;
            this.options.source = function(request, response) {
                $.ajax({
                        url: self.element.attr('data-ac-list-url'),
                        dataType: 'json',
                        data: { term: request.term },
                        success: function(data, textStatus, jqXHR) {
                            response(data);
                        },
                        error: function(jqXHR, textStatus, errorThrown) {
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


            this.element.dblclick(function(e) {
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
                        beforeSend: function(jqXHR, settings) {
                            self.element.addClass('ui-autocomplete-loading');
                        },
                        dataFilter: function(data, type) {
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
                        complete: function(jqXHR, settings) {
                            self.element.removeClass('ui-autocomplete-loading');
                        },
                        error: function(jqXHR, textStatus, errorThrown) {
                            // Error encountered during the remote call. Just show some diagnostic. If this happens, system becomes unstable
                            alert(jqXHR.responseText);
                        }
                    }
                });
            }
        },

        /* Special handle the select event. */
        _trigger: function(type, event, ui) {
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
        _selectValue: function(data) {
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
        clear: function() {
            this._selectValue({ value: '', label: '', shortName: '' });

            // Make sure remote validator will perform the query again
            var validator = this.element.closest('form').validate();
            var prev = validator.previousValue(this.element[0]);
            prev.old = null;
            prev.valid = true;

        }
    });
})(jQuery);

$.validator.setDefaults({
    onkeyup: false,    // remote validation cannot tolerate validation on every keystroke
    onfocusout: false
});

$(document).ready(function () {
    $("input[data-ac-list-url]").autocompleteEx();
});



/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

// Apply to the text box whose scan will be passed to the server via ajax call
// When user presses enter on the text box, the value in the text box is passed to the action method.
// The response code returned by the action indicates whether the scan was successful or not.
// In the case of success, the returned string is displayed with success colrs.
// In the case of error, the returned string is displayed with error colors.
// Raises success event when ajax call is successful.
// async is set to false so that this call is executed in asynchronous mode. It will aslo force other call to be excuted after it's callback. bkumar:19 June 2012
$.widget("ui.handlescan", {
    // default options
    options: {
        // Selector to the enclosing form if any. Inputs of this form will be posted in addition to inputs of the form enclosing the element to which this widget is attached.
        formMain: null,
        // Container which displays validation errors
        errors: '#ajaxErrors',

        // Function which returns the name of the action. Default: action attribute of the closest form
        action: null,

        // Selector of the input which will store the value of the scan to confirm
        confirmInput: '#tbConfirm'
    },
    _create: function () {
        // creation code for mywidget
        var self = this;
        this.element.keypress(function (e) {
            if (e.keyCode === $.ui.keyCode.ENTER && $(this).val()) {
                self.scan();
                //$(this).val('').focus();
                return false;
            }
        });
    },

    // Executes the ajax call
    scan: function () {
        var $form = this.element.closest('form');
        if (!$form.valid()) {
            // Form is invalid. Do nothing
            return;
        }
        var data = $form.serializeArray();
        if (this.options.formMain) {
            if (!$(this.options.formMain).valid()) {
                return;
            }
            data = data.concat($(this.options.formMain).serializeArray());
        }
        $.ajax({
            cache: false,
            async: false,
            url: (this.options.action && this.options.action()) || $form.attr('action'),
            data: data,
            context: this,
            type: 'POST',
            statusCode: {
                // Success. Display data as status
                202: function (data, textStatus, jqXHR) {
                    PlaySound('success');
                    $(this.options.errors).html(data).removeClass('field-validation-error').addClass('success-display');
                },
                203: function (data, textStatus, jqXHR) {
                    // Error
                    PlaySound('error');
                    $(this.options.errors).show().html(data).removeClass('success-display').addClass('field-validation-error');


                },
                // Request confirmation.
                201: function (data, textStatus, jqXHR) {
                    $(this.options.confirmInput).val(jqXHR.getResponseHeader('ConfirmScan'));
                    $(this.options.errors).show().html(data).addClass('success-display').removeClass('field-validation-error');
                },
                200: function (data, textStatus, jqXHR) {
                    PlaySound('error');
                    alert(data);
                    alert("Refreshing the page will solve this problem.");
                }
            },
            success: function (data, textStatus, jqXHR) {
                //PlaySound('success');
                this._trigger("success", null, { jq: jqXHR });
                $('#tbConfirm').val('');
            },
            error: function (jqXHR, textStatus, errorThrown) {
                // Catastrophic error
                PlaySound('error');
                $(this.options.errors, this.element).html(jqXHR.responseText);
            },
            beforeSend: function (jqXHR, settings) {
                // Show loading image
                this.element.addClass('ui-autocomplete-loading');
            },
            complete: function (jqXHR, textStatus) {
                // Remove loading image
                this.element.removeClass('ui-autocomplete-loading');
                this.element.val('');

            }
        });

    },

    // Clear display of all validation errors
    clearErrors: function () {
        var $form = this.element.closest('form');
        $(this.options.errors).empty();
        this.element.val('').removeClass('input-validation-error');
        $('span.field-validation-error', $form).removeClass('field-validation-error').addClass('field-validation-valid');
    }
});


function PlaySound(file) {
    // alert(file);
    try {
        $('#sound_' + file)[0].play();
    }
    catch (e) {
        // No plugin available? Browser does not support HTML5? Ignore the error
    }
    //    var $embed = $('embed', $sound).removeAttr('autostart').attr('autostart', true);
    //    $sound.children('span:first').html($embed[0].outerHTML);
}





//$Id: HandleScan.partial.js 23095 2013-11-12 10:31:31Z spandey $
/// <reference path="~/Scripts/jquery-1.6.2-vsdoc.js" />

//Validates destination Pallet. Treats enter as tab
$(document).ready(function () {
    $('#tbPallet').change(function (e) {
        if (!$(this).closest('form').validate().element($(this))) {
            return false;
        }
        $.ajax({
            url: $(this).attr('data-validate-url'),
            data: { palletId: $(this).val() },
            context: this,
            type: 'GET',
            cache: false,
            success: function (data, textStatus, jqXHR) {
                // Success. Display data as status
                PlaySound('success');
                $('#spnPalletInfo').html("<br/>" + data);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR.responseText);
            }
        });
    });
});

