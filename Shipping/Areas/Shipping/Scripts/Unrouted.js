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

/// <reference path="~/Areas/Shipping/Scripts/Unrouted.bundle.js" />/
/// Implements selectable feature
/// Intiliazes datetime picker

// Handle select all checkbox. Select all checkboxes perform changes on each selected check box
$(document).ready(function () {
    $('#pogroups').on('click', 'thead input:checkbox', function (e) {
        $(this).closest('table').find('tbody input:checkbox')
            .prop('checked', $(this).is(':checked'))
            .closest('td')
            .toggleClass('ui-selected', $(this).is(':checked'));

        ShowSelectedPoCount();
    });

    // Show/hide POs having no bucket.
    $('#cbShowUnavailableBucket').change(function (e) {
        $('#frmShowUnavailableBucket').submit();
    });

    //Show the number of selected POs.
    function ShowSelectedPoCount() {
        var $selected = $('td.ui-selected');
        if ($selected.length >= 0) {
            $('span.spnPOSelected').text('Total : ' + $selected.length + ' POs selected');
        }
    }
    // Handle datepicker event on UnRouted UI and used to assign the selected date to each selected orders.
    $("#tbAtsDate").datepicker({
        showOn: "button",
        buttonImage: $('#tbAtsDate').attr('data-calendar-img'),
        buttonImageOnly: true,
        dateFormat: 'm/d/yy',
        onSelect: function () {
            $(this).focus();
        },
        numberOfMonths: 2,
        beforeShowDay: function (date) {
            var ats = null;
            // This code expects the variable _atsDates to exists which is defined on UnRouted UI.
            $.each(_atsDates, function (i, val) {
                if (val.date == date.getFullYear() * 10000 + (date.getMonth() + 1) * 100 + date.getDate()) {
                    ats = val;
                    return false;
                }
            });
            // This code is written to distinguish the weekends.
            var uiClasses = "";
            if (date.getDay() == 0 || date.getDay() == 6) {
                uiClasses = 'weekend-day';
            }
            // This code is written to provide the total POs available for routing on a particular ATS date.
            if (ats) {
                return [true, uiClasses + " ui-selected", 'This ATS date has ' + (ats.count).toString() + ' POs for routing'];
            } else {
                return [true, uiClasses];
            }
        }
    });
     // Handle selectable event on selected orders.
    $('#pogroups tbody').selectable({
        filter: 'td.po',
        cancel: 'a, input',
        stop: function (event, ui) {
            $('td.po', this).each(function () {
                //Check the hidden check boxes for each row selected by user.
                $('input:checkbox', this).prop('checked', $(this).is('.ui-selected'));
                //if ($(this).is('.ui-selected')) {                    
                //    $('input:checkbox', this).attr('checked', 'checked');
                //} else {
                //    $('input:checkbox', this).removeAttr('checked');
                //}
            });
            ShowSelectedPoCount();
        }
    }).on('click', 'td.dc', function (e) {
        alert('Hi');
    }).tooltip({
        content: function () {
            return $(this).next().html();
        },
        items: 'span.ui-icon.ui-icon-alert'
    });

    // When Dc Cancel date and DC is clicked in the quick list, highlight the clicked date and the table we navigate to
    $('#quicklist').on('click', 'a[href^="#"]', function (e) {
        $('a', e.delegateTarget).removeClass('ui-state-highlight');
        $(this).addClass('ui-state-highlight');
        $('table caption').removeClass('ui-state-highlight');
        $('caption', $(this).attr('href')).addClass('ui-state-highlight');
    });

    // Creating dialog for Ats Date.
    $('#divAtsDate').dialog({
        dialogClass: 'dlg-atsdate',
        resizable: false,
        height: 150,
        width: 233,
        closeOnEscape: false,
        //It will fixed the position of the dialog at right center of the window.
        position: { my: 'right center', at: 'right-20% center+10%' },
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close").hide();
        },
        buttons: [{
            text: 'Assign',
            id:'btnAssign',
            click: function (event, ui) {
                var $form = $('#frmAtsDate');
                var $selected = $('table tbody input:checkbox:checked');
                // Explicity set value for checkbox if it is checked...hide MVC bug
                if ($('#cbElectronicEdi').is(':checked')) {
                    $('#cbElectronicEdi').val(true);
                }
                if ($selected.length == 0) {
                    alert("Please select POs to route.");
                    return;
                }
                // Append selection checkboxes to the form in dialog and post the form.
                $selected.appendTo($form);
                $form.submit();
            }
        }]
    });
});

