/// <reference path="../../../../Scripts/jquery-1.6.2-vsdoc.js" />
/// <reference path="../../../../Scripts/jquery-1.6.2-vsdoc.js" />

/*
This is a generic script.
It searches for div[data-edit-dialog] and associates a click handler with each qualifying div.
When a pencil icon is clicked, it opens the dialog specified by attribute data-edit-dialog.
When a close icon is clicked, it makes an ajax call to the url specified by the data-delete-ajax-url attribute.

The edit dialog can have spans with data-name attribute. When the dialog opens, the HTML of each of these spans will be set to the html of the span
with the same data-name attribute in the clicked row. When the Go button is clicked, the values of all inputs within the dialog, as well as the HTML of
all spans with data-name attribute is posted to the action.
*/
$(document).ready(function () {
    /*
    data-list-container: The id of the list to update after editing
    */
    $('input').keypress(function (e) {
        if (e.keyCode === $.ui.keyCode.ENTER) {
            return false;
        }
    });


    $('div[data-edit-dialog]').click(function (e) {
        var $dlg = $($(this).attr('data-edit-dialog'));
        $('tr', this).removeClass('ui-state-highlight');
        if ($(e.target).is('.ui-icon-pencil')) {
            var $th = $(e.target).closest('table').find('thead > tr > th');
            $(e.target).closest('tr')
                    .addClass('ui-state-highlight')
                    .find('td').each(function (index) {
                        //Binding all the values of span on the dialog box
                        // Populate the visible spans
                        var name = $th.eq(index).attr('data-name');
                        if (name) {
                            var selector = $.validator.format("span[data-name='{0}']", name);
                            $(selector, $dlg).html($(this).html());
                            // Populate the hidden fields
                            selector = $.validator.format("input[name='{0}']", name);
                            var inputType = $(selector, $dlg).attr('type');
                            if (inputType == 'checkbox')
                            {
                                var val = $(this).attr('data-val');
                                if (val == "True") {
                                    $(selector, $dlg).attr('checked', 'checked');
                                }
                                else {
                                    $(selector, $dlg).removeAttr('checked', 'checked')
                                }
                            }
                            $(selector, $dlg).val($(this).attr('data-val'));
                        }
                    });
            $dlg.dialog('open');
        } else if ($(e.target).is('.ui-icon-close')) {
            $(e.target).closest('tr')
                  .addClass('ui-state-highlight');
            var dialogData = new Object();
            var msg = $(e.target).attr('title') + ' for ';
            //Collecting details for Delete
            var $th = $(e.target).closest('table').find('thead > tr > th');
            $(e.target).closest('tr').find('td').each(function (index) {
                var name = $th.eq(index).attr('data-name');
                if (name) {
                    dialogData[name] = $(this).attr('data-val');
                }
                var displayName = $th.eq(index).attr('data-display-name');
                if (displayName) {
                    msg += $.validator.format("{0} : {1}; ", displayName, $(this).html()) ;
                }
            });
            var result = confirm(msg);
            if (result) {
                $.ajax({
                    url: $(this).attr('data-delete-ajax-url'),
                    type: 'POST',
                    context: this,
                    data: dialogData,
                    statusCode: {
                        // Success
                        200: function (data, textStatus, jqXHR) {
                            //updating the list with updated data
                            $(this).html(data);
                        },
                        // Error
                        203: function (data, textStatus, jqXHR) {
                            //alert(data);
                            $('div[data-valmsg-summary]').html(data)
                               .removeClass('validation-summary-valid')
                                .addClass('validation-summary-errors');
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(jqXHR.responseText);
                    }
                });
            }
        }
    }).each(function () {
        // Setup edit dialog
        var list = this;
        var $dlg = $($(this).attr('data-edit-dialog'));
        $dlg.dialog({
            create: function (event, ui) {
                $(this).dialog('option', 'title', $(this).attr('title'));
            },
            open: function (e) {
                $('form:first', this).validate().resetForm();
                $('div.validation-summary-errors').removeClass('validation-summary-errors').addClass('validation-summary-valid');
            },
            width: 'auto',
            autoOpen: false,
            buttons: [
    {
        text: 'Ok',
        click: function (e) {
            // Validate, make ajax call and then close on success
            if (!$('form:first', this).valid()) {
                return;
            }
            $.ajax({
                url: $('form:first', this).attr('action'),
                type: 'POST',
                context: this,
                data: $('form:first', this).serializeArray(),
                statusCode: {
                    // Success
                    200: function (data, textStatus, jqXHR) {
                        //updating the list with updated data
                        $(list).html(data);
                        $(this).dialog('close');
                    },
                    // Error
                    203: function (data, textStatus, jqXHR) {
                        //alert(data);
                        $('div[data-valmsg-summary]').html(data)
                            .removeClass('validation-summary-valid')
                            .addClass('validation-summary-errors');
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert(jqXHR.responseText);
                }
            });
        }
    },
    {
        text: 'Cancel',
        click: function (e) {
            $(this).dialog('close');           
        }
    }
    ]
        });
    }).each(function () {
        // Set up add functionality
        var list = this;
        var $dlg = $($(this).attr('data-add-dialog'));
        $dlg.dialog({
            create: function (event, ui) {
                $(this).dialog('option', 'title', $(this).attr('title'));
            },
            open: function (e) {
                $('input', this).val('').prop('disabled',false);
                $('select', this).val('');
                $('#rbSpecificStyle,#rbSpecificColor').attr('checked', 'checked');
                if ($('#cbIsSpotCheckEnable').is(':checked')) {
                    $('#cbIsSpotCheckEnable').val(true);
                }
                $('span[for="tbStyle"]').text('');
                $('span[for="tbColor"]').text('');
                $('span#lblStyleDesc,span#lblColorDesc').text('');               
                $('form:first', this).validate().resetForm();
                $('div.validation-summary-errors').removeClass('validation-summary-errors').addClass('validation-summary-valid');
            },
            autoOpen: false,
            width: 'auto',
            buttons: [
                    {
                        text: 'Add',
                        click: function (e) {
                            var $form = $('form:first', this);                          
                            if (!$form.valid()) {
                                return false;
                            }
                            $.ajax({
                                url: $form.attr('action'),
                                type: 'POST',
                                context: this,
                                data: $form.serializeArray(),
                                statusCode: {
                                    // Success
                                    200: function (data, textStatus, jqXHR) {
                                        //updating the list with updated data
                                        $(list).html(data);
                                        $(this).dialog('close');
                                    },
                                    // Error
                                    203: function (data, textStatus, jqXHR) {
                                        //alert(data);
                                        $('div[data-valmsg-summary]').html(data)
                                        .removeClass('validation-summary-valid')
                                            .addClass('validation-summary-errors');
                                    }
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    alert(jqXHR.responseText);
                                }
                            });
                        }
                    },
                    {
                        text: 'Cancel',
                        click: function (e) { $(this).dialog('close'); }
                    }
                ]
        });
        var $btn = $($(this).attr('data-add-button'));
        $btn.button().click(function (e) {
            $dlg.dialog('open');
        });
    });

    // Handle change event of radio button for All styles and AllColors.
    // Initialize value for radio button based on checked state.Also initialize value for style autocomplete.
    // Id of textbox for style,color and value for data-attr-disabletb must be same.
    $('#rbAllStyle,#rbAllColors').change(function (e) {
        var checked = $(this).is(':checked');
        var tb = $(this).attr('data-attr-disabletb')
        if (checked) {
            $(this).val(true);
            $('#' + tb).prop('disabled', checked).val("All").removeClass('input-validation-error');
            $('span[for='+ tb +']').text('');
           
        }
        else {
            $(this).val(false)
            $('#' + tb).prop('disabled', checked).val("");
        }      
       
       
    });

    // Hanlde change event of radio button for specific style and color
    // Id of textbox for style,color and value of data-attr-enabletb must be same.
    $('#rbSpecificStyle,#rbSpecificColor').change(function (e) {
        var checked = $(this).is(':checked');
        var tb = $(this).attr('data-attr-enabletb')
        if (checked) {
            $('#' + tb).prop('disabled',!checked).val("");
        }

    });

    // Handle change event of checkfor enable/disbale SpotCheck
    // Initialize value for check box based on checked state.
    $('#cbIsSpotCheckEnable,#cbEditIsSpotCheckEnable').change(function (e) {
        var checked = $(this).is(':checked');
        if (checked) {
            $(this).val(true);
        }
        else {
            $(this).val(false)
        }
    });
});



//$Id$