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