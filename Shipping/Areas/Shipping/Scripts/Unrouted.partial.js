/// <reference path="~/Areas/Shipping/Scripts/Unrouted.bundle.js" />/
/// Implements selectable feature
/// Intiliazes datetime picker

// Handle select all checkbox. Select all checkboxes perform changes on each selected check box
$(document).ready(function () {
    $('table thead input:checkbox').click(function (e) {
        var $tr = $('tbody tr.ui-selectee', $(this).closest('table'));
        if ($(this).is(':checked')) {
            $tr.filter(':not(.ui-selected,tr.ui-state-disabled)').addClass('ui-selected')
                .find('input:checkbox').attr('checked', 'checked');
        } else {
            $tr.filter('.ui-selected').removeClass('ui-selected')
                .find('input:checkbox').removeAttr('checked');
        }
        ShowSelectedPoCount();
    });

    // Show/hide POs having no bucket.
    $('#cbShowUnavailableBucket').change(function (e) {
        $('#frmShowUnavailableBucket').submit();
    });

    //Show the number of selected POs.
    function ShowSelectedPoCount() {
        var $selected = $('tbody tr.ui-selected').not('.text-changed');
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
    $('.tbody').selectable({
        filter: 'tr',
        cancel: 'a,tr.ui-state-disabled',
        stop: function (event, ui) {
            $('tr', this).each(function () {
                //Check the hidden check boxes for each row selected by user.
                if ($(this).is('.ui-selected')) {                    
                    $('input:checkbox', this).attr('checked', 'checked');
                } else {
                    $('input:checkbox', this).removeAttr('checked');
                }
            });
            ShowSelectedPoCount();
        }
    }).end().tooltip({
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
        height: 138,
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
