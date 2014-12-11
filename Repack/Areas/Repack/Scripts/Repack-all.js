///#source 1 1 /Areas/Repack/Scripts/conditional-validation.partial.js
/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />


$.validator.addMethod('requiredif',
    function (value, element, parameters) {
        var id = 'input[name=' + parameters['dependentproperty'] + ']';

        // get the target value (as a string, 
        // as that's what actual value will be)
        var targetvalue = parameters['targetvalue'];
        targetvalue = 
          (targetvalue == null ? '' : targetvalue).toString();

        // get the actual value of the target control
        // note - this probably needs to cater for more 
        // control types, e.g. radios
        var control = $(id);
        var controltype = control.attr('type');
        var actualvalue = controltype === 'checkbox' ? control.is(':checked') : control.val();

        // if the condition is true, reuse the existing 
        // required field validator functionality
        if (targetvalue === actualvalue.toString())
            return $.validator.methods.required.call(this, value, element, parameters);

        return true;
    }
);

$.validator.unobtrusive.adapters.add(
    'requiredif',
    ['dependentproperty', 'targetvalue'], 
    function (options) {
        options.rules['requiredif'] = {
            dependentproperty: options.params['dependentproperty'],
            targetvalue: options.params['targetvalue']
        };
        options.messages['requiredif'] = options.message;
    });

///#source 1 1 /Areas/Repack/Scripts/Repack.partial.js
/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />
/// <reference path="../../../Scripts/jquery.validate-vsdoc.js" />

$.validator.setDefaults({ onkeyup: false });
$(document).ready(function () {
    // Triggering the change event at startup to update the state of the carton id text box
    $('#cbScanCarton').change(function (e) {
        var checked = $(this).is(':checked');
        $('#divCartonId').toggle(checked);
        $('#divNoOfCartons').toggle(!checked);
        if (!checked) {
            $('#tbCartonId').val('');
        }
    }).change();
    $('#cbConversion').each(function () {
        $(this).change(function (e) {
            var checked = $(this).is(':checked');
            $('#divConversion').toggle(checked);
        }).change();
    });
});

// Pallet is required only if destination area requires pallet
$(document).ready(function () {
    // Enable/disable pallet depending on whether destination area required pallet
    $('#ddlDestinationArea').change(function (e) {
        var g = $('optgroup:has(option[value="' + $(this).val() + '"])', this).attr('label');
        // Show div for pallet area, else hide
        if (g) {
            if (g.indexOf('P') == 0) {
                $("#tbPalletId").rules("add", {
                    required: true,
                    messages: {
                        required: "Pallet is required for this area",
                    }
                });
                $('sup.ui-helper-hidden').show();
            } else {
                $("#tbPalletId").rules("remove", 'required');
                $('sup.ui-helper-hidden').hide();
            }
            $("span[for='tbPalletId']").hide();
            $('#tbPalletId').removeClass('input-validation-error');
        }
    });
});

// When enter is pressed on UPC code, focus pieces if entered UPC is valid
//$(document).ready(function () {
//    $('#tbUpcScan').keydown(function (e) {
//        if (e.keyCode === $.ui.keyCode.ENTER) {
//            $(this).valid();   // Ask remote validator to validate and then check whether entry was valid
//            if ($(this).valid()) {
//                setTimeout(function () {
//                    // Due to sounds, we must set the focus after minimal delay
//                    $('#tbPieces').focus();
//                }, 0);
//            }
//            return false;
//        }
//    });

//    $('#tbCartonId').keydown(function (e) {
//        if (e.keyCode === $.ui.keyCode.ENTER) {
//            setTimeout(function () {
//                // Due to sounds, we must set the focus after minimal delay
//                $('#tbUpcScan').focus();
//            }, 0);
//            return false;
//        }
//    });

   
   
//});


//$Id$

///#source 1 1 /Areas/Repack/Scripts/CreateDialog.partial.js
/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />
/// <reference path="../../../Scripts/jquery.validate-vsdoc.js" />

// The autocomplete widget must trigger th change event even if the value in the text box has not changed
$.widget("ui.autocomplete", $.ui.autocomplete, {
    _change: function (event) {
        //if (this.previous !== this._value()) {
            this._trigger("change", event, { item: this.selectedItem });
        //}
    },
});

$(document).ready(function () {

    $('form').submit(function (e) {
        // None of the forms should ever be submitted. This page never posts pack. It only issues ajax calls.
        // Occasionally, pressing enter causes the form to get submitted.
        return false;
    });
    // Open the repack dialog
    $('#btnRepack').click(function (e) {
        if (!$('#frmMain').valid()) {
            return;
        }
        $('#divCreate').dialog('open');
    }).button();

    // Setup the dialog
    $('#divCreate').dialog({
        title: 'Repack',
        autoOpen: false,
        width: 500,
        closeOnEscape: false,
        create: function (event, ui) {
            var dlg = this;

            $('input:text[data-url]', this).each(function (i, elem) {
                $(elem).autocomplete({
                    delay: 2000,
                    minLength: 1,
                    source: $(elem).attr('data-url'),
                    change: function (event, ui) {
                        //var $label = $('label[data-valmsg-for=' + this.id + ']', dlg).empty();
                        var $label = $('span[data-valmsg-for=' + this.name + ']', dlg)
                            .removeClass('field-validation-valid field-validation-error')
                            .addClass('field-validation-description')
                            .empty();
                        var val = $(this).val();                        
                        if (!val) {
                            // Text box is empty. No description needed
                            $('span[data-valmsg-for=' + this.name + ']', dlg).removeClass('field-validation-description').addClass('field-validation-error');
                            return;
                        }

                        if (ui.item && ui.item.value == val) {
                            // User chose from the list
                            //$label.text(ui.item.label);
                            $label.html(ui.item.standardSkuSize == null ? ui.item.label : ui.item.label + '<br>Standard Quantity ' + ui.item.standardSkuSize);
                            return;
                        }

                        // User scanned the UPC without selecting from list. Get description via ajax call
                        $.getJSON($(this).autocomplete('option', 'source'), { term: val }, $.proxy(function (data) {
                            if (data && data.length > 0 && data[0].value == this.val) {                                
                                //$label.text(data[0].label);
                                $label.html(data[0].standardSkuSize == null ?  data[0].label : data[0].label + '<br> Standard Quantity ' + data[0].standardSkuSize);
                            } else {
                                $label.text('Bad choice');
                            }
                        }, { val: val }));
                    }
                }).on('dblclick', function (e) {
                    $(this).autocomplete('option', 'minLength', 0).autocomplete('search', '').autocomplete('option', 'minLength', 1);
                });
            });

        },
        buttons: [
            {
                text: 'Reset',
                click: function (event, ui) {
                    $('#ajaxErrors').empty();
                    $('input:text, select', this).val('').removeClass('input-validation-error');
                    $('span.field-validation-error', this).removeClass('field-validation-error').addClass('field-validation-valid');                    
                    //setting focus to the first element of the dialog                    
                    $('input:tabbable:first', $('#divCreate')).focus();
                    $('span[data-valmsg-for="SkuBarCode"]').text('');
                    $('span[data-valmsg-for="TargetSkuBarCode"]').text('');

                }
            },
            {
                text: 'Create',
                click: function (event, ui) {
                    var $form = $('form:first', this);
                    if ($('#frmMain').valid() && $form.valid()) {
                        $.ajax({
                            cache: false,
                            url: $form.attr('action'),
                            data: $form.serializeArray().concat($('#frmMain').serializeArray()),
                            context: this,
                            type: 'POST'
                        }).done(function (data, textStatus, jqXHR) {
                            $('#ajaxErrors', this).html(data);
                            $('input:text', this).val('');  // Clear UPC and pieces
                        })
                        .error(function (jqXHR, textStatus, errorThrown) {
                            $('#ajaxErrors', this).html(jqXHR.responseText);
                        });
                    }
                    var toFocus = $('input:tabbable:first', this);     // Set focus to the first focusable input
                    setTimeout(function () {
                        // Due to sounds, we must set the focus after minimal delay
                        toFocus.focus();
                    }, 0);
                }
            },
            {
                text: 'Close',
                click: function (event, ui) {
                    $(this).dialog('close');
                }
            }

        ]
    }).on('keydown', 'input,select', function (e) {
        if (e.keyCode === $.ui.keyCode.ENTER) {
            // Enter behaves like tab            
            var inputs = $('input:text:visible,select:visible', e.delegateTarget);
            var myindex = inputs.index(this);
            if (myindex + 1 < inputs.length) {
                // Tab to next input
                inputs.eq(myindex + 1).focus();
            } else {
                //var x = $(e.delegateTarget).closest('div.ui-dialog').find('.ui-dialog-buttonpane button:eq(1)').length;
                // Click the create button. BUG: Assuming that Create button is at index 1             
                $(e.delegateTarget).closest('div.ui-dialog').find('.ui-dialog-buttonpane button:eq(1)').click();
                
            }
            //$(this).parents('.ui-dialog-buttonpane button:eq(0)').click();
            //e.preventDefault();
        }
    });
});



//$Id$
