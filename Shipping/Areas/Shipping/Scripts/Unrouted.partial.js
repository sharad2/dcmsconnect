/// <reference path="~/Areas/Shipping/Scripts/Unrouted.bundle.js" />/
/// Implements selectable feature
/// Intiliazes datetime picker

// Handle select all checkbox. Select all checkboxes perform changes on each selected check box
$(document).ready(function () {
    var _filter = 'tr';

    // Handle selectable event on selected orders.
    $('#pogroups tbody').selectable({
        filter: _filter,
        cancel: 'a, input:checkbox, td[rowspan]',
        stop: function (event, ui) {
            $(_filter, this).each(function () {
                //Check the hidden check boxes for each row selected by user.
                $('input:checkbox', this).prop('checked', $(this).is('.ui-selected'));
            });
            ShowSelectedPoCount();
        }
    }).on('click', 'td.dc', function (e) {
        // When a DC is clicked, select all rows for that DC
        var $tr = $(this).closest(_filter);
        var isSelected = $tr.hasClass('ui-selected');
        $tr.nextAll()
            .andSelf()
            .slice(0, $(this).attr('rowspan'))
            .toggleClass('ui-selected', !isSelected)
            .find('input:checkbox')
            .prop('checked', !isSelected);
    }).tooltip({
        content: function () {
            return $(this).next().html();
        },
        items: 'span.ui-icon.ui-icon-alert'
    });

    $('#pogroups thead').on('change', 'input:checkbox', function (e) {
        // When header checkbox clicked, select or unselect all rows
        $(this).closest('table').find('tbody input:checkbox')
            .prop('checked', $(this).is(':checked'))
            .closest(_filter)
            .toggleClass('ui-selected', $(this).is(':checked'));

        ShowSelectedPoCount();
    });

    // Show/hide POs having no bucket.
    $('#cbShowUnavailableBucket').change(function (e) {
        $('#frmShowUnavailableBucket').submit();
    });

    //Show the number of selected POs.
    function ShowSelectedPoCount() {
        var $selected = $('tr.ui-selected');
        $('span.spnPOSelected').text('Total : ' + $selected.length + ' POs selected');
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
            id: 'btnAssign',
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
