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





//$Id$