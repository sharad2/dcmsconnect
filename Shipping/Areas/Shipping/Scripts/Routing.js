/// <reference path="Routing.partial.js" />
/// Implements selectable feature
/// Implements Address dialog and RoutingEdditor dialog on Routing UI.
/// Checkbox id should be started with 'cb' and followed by the id of corresponding textbox .

$(document).ready(function () {

    $('#cbShowRouted').change(function (e) {
        $('#frmShowRouted').submit();
    });

    //Show the number of selected POs.
    function ShowSelectedPoCount() {
        var $selected = $('tbody tr.ui-selected').not('.text-changed');
        if ($selected.length >= 0) {
            $('span.spnPOSelected').text('Total : ' + $selected.length + ' POs selected');
        }
    }
    
    //Code to validate passed DC.
    $('#tbDC').change(function (e) {
        $.ajax({
            cache: false,
            url: $('#tbDC').attr('data-validate-url').replace('~', $('#tbDC').val()),
            type: 'POST',
            context: this,
            success: function (data, textStatus, jqXHR) {
                if (jqXHR.status == 203) {
                    alert(data);
                    return;
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR.responseText);
            }
        });
    });

    // Set up the routing editor dialog
    $('#dlgRoutingEditor').dialog({
        autoOpen: false,
        modal: false,
        title: 'Update Routing Information',
        dialogClass: 'routing-editor',
        create: function (event, ui) {
            // The dialog will open when button is clicked
            var self = this;
            var dragging = false;
            $('#btnRoutingEditor').click(function (e) {
                // Click event is raised after the user has dragged the button. Prevent the dialog from opening in this case.
                if (!dragging) {
                    $(self).dialog('option', 'position', { my: "left bottom", at: "left bottom", of: $(e.target) }).dialog('open');
                }
            }).button().draggable({
                start: function (event, ui) {
                    dragging = true;
                },
                stop: function (event, ui) {
                    setTimeout(function () {
                        // Make sure click does nothing
                        dragging = false;
                    }, 0);
                }
            });
        },
        open: function (event, ui) {
            //The RoutingEditor button will hide when RoutingEditor dialog will open. 
            $('#btnRoutingEditor').hide();
        },
        beforeClose: function (event, ui) {
            $('#btnRoutingEditor').show();
        },
        buttons: [
            {
                text: 'Apply',
                click: function (event, ui) {
                   
                    // Make sure that at least one checkbox is selected
                    if ($('input:checkbox:checked', this).length == 0) {
                        alert('Please select something to update');
                        return;
                    }
                    // Make sure that at least one PO is selected
                    var $selected = $('#divEdiList tbody input:checkbox:checked');
                    if ($selected.length == 0) {
                        alert('Please select some POs to apply the routing information to');
                        return;
                    }
                    // Clone the selection checkboxes, append to the form in dialog and post the form.
                    var $form = $('form', this);
                    $selected.clone().appendTo($form);
                    $form.submit();
                }
            },
            {
                text: 'Cancel',
                click: function (event, ui) {
                    $(this).dialog('close');
                }
            }
        ]
    }).on('change', 'input', function (e) {
        // If any tb or cb changes, raise change event of all selected rows
        $('#divEdiList tbody input:checked').change();
    }).on('keyup', 'input:text', function (e) {
        // Check the associcated cb as soon as the user types in the tb
        // The associated cb has the same id as the tb, with a prefix of 'cb'
        if ($(this).val()) {
            $('#cb' + $(this).attr('id')).prop('checked', true);
        }
    })
    // manage row selection
    $('#divEdiList').on('change', 'table thead input:checkbox', function (e) {
        // Handle header checkbox. Select all hidden checkboxes and raise change event of each selected check box
        var checked = $(this).is(':checked');
        $('tbody input:checkbox', $(this).closest('table'))
            .filter(function (i) {
                // Optimization. Only worry about those inputs which do not have the proper state.
                return $(this).prop('checked') != checked;
            }).prop('checked', checked)
            .change();
        var $tr = $('tbody tr.ui-selectee', $(this).closest('table'));
        $tr.toggleClass('ui-selected', checked);

        //Function call to show the number of selected POs on dialog(Active when header checkbox will be checked/unchecked).
        ShowSelectedPoCount();
        return true;
    }).on('change', 'tbody input:checkbox', function (e) {
        // The selection state of the row has changed.
        // Update the strike through status of my row based on the curently selected routing info checkboxes
        //Toggle Selected on single row
        var $tr = $(this).closest('tr');
        var checked = $(this).is(':checked');
        $tr.find('td.ui-selectee').toggleClass('ui-selected', checked);
        //Function call to show the number of selected POs on dialog(Active when particular row selected/unselected).
        ShowSelectedPoCount();
        $('#dlgRoutingEditor input:text').each(function (i) {
            if ($('#cb' + $(this).attr('id')).is(':checked')) {
                // Strike the old value and show the new value which is possibly empty
                $tr.find('label[for=' + $(this).attr('id') + ']')
                    .toggleClass('text-changed', checked)
                    .next('span')
                    .text(checked ? $(this).val() : '')
                    .toggle(checked);
            } else {
                // Unstrike the old value and remove the new value
                $tr.find('label[for=' + $(this).attr('id') + ']')
                    .removeClass('text-changed')
                    .next('span')
                    .empty()
                    .hide();
            }
        });
    })
        //.on('click', 'span.ui-icon-contact', function (e) {
        //// Set up the shipping address(show original address and current address)dialog.
        //$('#dlgOriginalAddress').dialog({
        //    position: { my: "left top", at: "left bottom", of: $(this) },
        //    data_url: $(e.target).attr('data-ship-address-url')
        //}).dialog('open');
        //return false;
//})
    .on('click', 'div.ui-icon-close', function (e) {
        // Handle user request to unroute
        if (!confirm("Do you want to remove this order ")) {
            return false;
        }
        $.ajax({
            cache: false,
            url: $(e.delegateTarget).attr('data-delete-url').replace('~', $(this).attr('data-key')),
            type: 'POST',
            context: this,
            success: function (data, textStatus, jqXHR) {
                if (jqXHR.status == 203) {
                    alert(data);
                    return;
                }
                //line-through the deleted tr and changed icon with text 'Done'
                $(this).parents('tr').addClass('text-changed ui-state-disabled').removeClass('ui-selected ui-selectee')
                    .find('div').removeClass('ui-icon ui-icon-close').text('Done');
                //Function call to show the number of selected POs on dialog(Active when any row deleted).
                ShowSelectedPoCount();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR.responseText);
            }
        });
    }).find('tbody')
    .selectable({
        filter: 'tr:not(.ui-state-disabled)',
        cancel: 'tr.ui-state-disabled,td:has(.ui-icon-close,.ui-icon-contact)',
        stop: function (event, ui) {
            // Raise the change event of each selected row so that the visual display gets updated
            $('tr', this).each(function () {
                // Optimization. If the cb already has the correct state, we do not raise its change event
                var $cb = $('input:checkbox', this);
                var newstate = $(this).is('.ui-selected');
                if ($cb.prop('checked') != newstate)
                    $cb.prop('checked', newstate).change();
            });
        }
    }).end().tooltip({
        //Used to show appropriate/conditional massage via tooltip
        content: function () {
            return $(this).next().html();
        },
        items: 'span.ui-icon.ui-icon-alert'
    });
    // Set up the Carrier autocomplete.
    $('#tbCarrier').autocomplete({
        minLength: 0,
        source: $('#tbCarrier').attr('data-carrier-autocomplete'),
        change: function (event, ui) {
            // Validate  carrier in case it is not selected from list
            if (ui.item == null) {
                $.ajax({
                    cache: false,
                    url: $(this).attr('data-validate-carrier').replace('~', $(this).val()),
                    type: 'POST',
                    context: this,
                    success: function (data, textStatus, jqXHR) {
                        if (jqXHR.status == 203) {
                            alert(data);
                            $(this).val('');
                            $('#cbtbCarrier').removeAttr('checked');
                            return;
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(jqXHR.responseText);
                    }
                });
            }
            // Raise the carrier change event
            $(this).change();
        },
        select: function (event, ui) {
            $('#cbtbCarrier').prop('checked', true);
        },
        autoFocus: true
    }).dblclick(function (e) {
        // Double clicking will unconditionally open the drop down
        $(this).autocomplete('search');
    });

    // Set up the datetime picker
    $('#tbPickupDate').datepicker({
        showOn: "button",
        buttonImage: $('#tbPickupDate').attr('data-calendar-img'),
        buttonImageOnly: true,
        dateFormat: 'm/d/yy',
        onSelect: function () {
            $('#cbtbPickupDate').prop('checked', true);
            $(this).change();
        }
    });

    $('#tbStartDate,#tbDcCancelDate').datepicker({
        showOn: "button",
        buttonImage: $('#tbStartDate,#tbDcCancelDate').attr('data-calendar-img'),
        buttonImageOnly: true,
        dateFormat: 'm/d/yy'
    });

    // When ATS date is clicked in the quick list, highlight the clicked date and the table we navigate to
    $('#quicklist').on('click', 'a[href^="#"]', function (e) {
        $('a', e.delegateTarget).removeClass('ui-state-highlight');
        $(this).addClass('ui-state-highlight');
        $('table caption').removeClass('ui-state-highlight');
        $('caption', $(this).attr('href')).addClass('ui-state-highlight');
    });

    ////Showing the original address in dialog
    //$('#dlgOriginalAddress').dialog({
    //    autoOpen: false,
    //    modal: false,
    //    title: 'Shipping address',
    //    open: function (event, ui) {
    //        $.ajax({
    //            cache: false,
    //            url: $(this).dialog('option', 'data_url'),
    //            type: 'GET',
    //            context: this,
    //            success: function (data, textStatus, jqXHR) {
    //                if (data == "") {
    //                    $('#divOriginalAddress').html('Address not entered yet.');
    //                } else {
    //                    $('#divOriginalAddress').html(
    //                        data.OriginalShipAddress.AddressLine1 + '<br/>' +
    //                        data.OriginalShipAddress.AddressLine2 + '<br/>' +
    //                        data.OriginalShipAddress.AddressLine3 + '<br/>' +
    //                        data.OriginalShipAddress.AddressLine4 + '<br/>' +
    //                        data.OriginalShipAddress.City + '<br/>' +
    //                        data.OriginalShipAddress.State + '<br/>' +
    //                        data.OriginalShipAddress.ZipCode + '<br/>' +
    //                        data.OriginalShipAddress.Country);
    //                }
    //                if (data == "") {
    //                    $('#divCurrentAddress').html('Address not entered yet.');
    //                } else {
    //                    //Showing the current address(changed address) in dialog if any.
    //                    $('#divCurrentAddress').html(
    //                        data.CurrentShipAddress.AddressLine1 + '<br/>' +
    //                        data.CurrentShipAddress.AddressLine2 + '<br/>' +
    //                        data.CurrentShipAddress.AddressLine3 + '<br/>' +
    //                        data.CurrentShipAddress.AddressLine4 + '<br/>' +
    //                        data.CurrentShipAddress.City + '<br/>' +
    //                        data.CurrentShipAddress.State + '<br/>' +
    //                        data.CurrentShipAddress.ZipCode + '<br/>' +
    //                        data.CurrentShipAddress.Country);
    //                }
    //            },
    //            error: function (jqXHR, textStatus, errorThrown) {
    //                alert(jqXHR.responseText);
    //            }
    //        });
    //    },
    //    buttons: [
    //        {
    //            text: 'OK',
    //            click: function (event, ui) {
    //                $(this).dialog('close');
    //            }
    //        }
    //    ]
    //});

    //This button is used to filter the orders for particular date.
    $('#btnApplyFilter').button();
});
// When the user is interacting with the mouse, pretend that he has the Ctrl key pressed.
$(function () {
    $.widget("ui.selectable", $.ui.selectable, {
        _mouseStart: function (event) {
            event.ctrlKey = true;
            this._super(event);
        },
        _mouseDrag: function (event) {
            event.ctrlKey = true;
            this._super(event);
        },
        _mouseStop: function (event) {
            // Do not let the base class select items which were already selected. Instead, we want to unselect them.
            event.ctrlKey = true;
            this._super(event);
        },
        _trigger: function (type, event, ui) {
            if (type == 'selecting') {
                $(ui.selecting).addClass('ui-selected');
            }
            this._super(type, event, ui);
        }
    });
});