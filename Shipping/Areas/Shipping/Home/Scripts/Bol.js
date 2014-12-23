///#source 1 1 selectable.partial.js
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

///#source 1 1 Bol.partial.js
// Returns true if the two passed appointments have similar properties
function AppointmentSimilar(app1, app2) {
    if (app1.id && app2.id) {
        return app1.id == app2.id;
    }
    return app1.CarrierId && app2.CarrierId && app1.start && app2.start &&
        app1.CarrierId == app2.CarrierId && Number(app1.start) == Number(app2.start);
}

$(function () {
    // It is a wrapper to the fulCalendar plug-in. You can pass the option initialAppointment
    // which will ensure that the displayed date range includes this appointment and the appointment is selected.
    // The method selectAppointment can be used to navigate to an appointment and select it. If the appointment is not found, the user will be given an option to create it.
    // An appointment is automatically selected when it is clicked
    // The following attributes are expected on the element to which this widget is applied:
    // data-start-param, data-end-param, data-event-source, data-create-url
    $.widget("bol.calendar", {
        // default options
        options: {
            initialAppointment: null,  // The initial appointment which should be displayed and highlighted
            click: null,             // An event which is triggered when an appointment is selected by a user click. ui.app contains the app which was clicked
            change: null           // An event which is triggered when appointment selection changes. ui.app contains the new selected appointment
        },

        // The http request object used for loading events. Global because it is used by the widget
        _loader: null,
        _create: function () {
            var initialDate
            if (this.options.initialAppointment && this.options.initialAppointment.start) {
                initialDate = $.fullCalendar.parseDate(this.options.initialAppointment.start);
            }
            var self = this;
            this.element.fullCalendar({
                theme: true,
                weekends: true,
                height: 350,
                header: {
                    left: 'prev,next today',
                    center: 'title',
                    right: ''
                },
                defaultView: 'basicWeek',
                timeFormat: 'h:mm TT',
                allDayDefault: false,
                ignoreTimezone: false,
                year: initialDate ? initialDate.getFullYear() : undefined,
                month: initialDate ? initialDate.getMonth() : undefined,
                date: initialDate ? initialDate.getDate() : undefined,
                events: function (start, end, callback) {
                    self._load(start, end, callback);
                },
                eventClick: function (calEvent, jsEvent, view) {
                    if ($(jsEvent.target).is('.new-event')) {
                        // Create link was clicked. Create the appointment.
                        self._createAppointment(calEvent);
                        return;
                    }
                    if (calEvent) {
                        if (calEvent.id) {
                            // This is an existing appointment. Toggle selection state
                            self.selectAppointment(calEvent);
                            self._trigger('click', jsEvent, { app: calEvent });
                            return;
                        }
                        // This is an uncreated appointment. Ask the user for confirmation.
                        if (confirm("This appointment has not yet been created. Would you like to create it now?")) {
                            // Uncreated appointments are selected only after confirmation
                            self._createAppointment(calEvent);
                        }
                    }
                },
                eventRender: function (event, element, view) {
                    element.attr('title', event.EventToolTip);
                    if (event.appointmentHtml) {
                        $('.fc-event-inner', element).append(event.appointmentHtml);
                    }
                    $('.fc-event-title', element).attr('title', event.AppointmentDateDispaly);
                }
            });
        },

        // Loads appointments
        _load: function (start, end, callback) {
            var data = new Object();
            data[this.element.attr('data-start-param')] = Math.round(+start / 1000);
            data[this.element.attr('data-end-param')] = Math.round(+end / 1000);
            this._loader = $.ajax({
                url: this.element.attr('data-event-source'),
                dataType: 'json',
                data: data,
                traditional: true,
                cache: false,
                context: this,
                success: function (data, textStatus, jqXHR) {
                    if (this.options.initialAppointment && this.options.initialAppointment.id) {
                        var app = $.each(data, function (i, val) {
                            if (val.id == _initialAppointment.id) {
                                val.className = 'ui-selected';
                            }
                        });
                    }
                    callback(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert(jqXHR.responseText);
                    callback([]);       // Empty array
                },
            });
        },

        // Selects the appointment which matches the passed appointment.
        selectAppointment: function (app) {
            if (app.start) {
                this.element.fullCalendar('gotoDate', app.start);
            }
            var self = this;
            this._loader.done(function () {
                self._doSelectAppointment(app);
                self._trigger('change', null, { app: app });
            });
        },

        // Select the passed appointment. If it does not exist, create it in the UI only
        _doSelectAppointment: function (app) {
            var matches = this.element.fullCalendar('clientEvents', function (calapp) {
                if (AppointmentSimilar(app, calapp)) {
                    calapp.className = 'ui-selected';
                    return true;
                } else {
                    calapp.className = '';
                    return false;
                }
            });
            if (matches.length > 0) {
                this.element.fullCalendar('rerenderEvents');
            } else {
                app.title = 'New Appointment for Building ' + app.BuildingId + ', Carrier ' + app.CarrierId + '.';
                app.className = 'ui-state-highlight';
                app.appointmentHtml = '<a href="#" class="new-event">Create</a>';
                this.element.fullCalendar('renderEvent', app);
            }
        },
        // Create the passed appointment using an AJAX call, and then select it.
        _createAppointment: function (app) {
            // Create this event now
            $.ajax({
                url: this.element.attr('data-create-url'),
                type: 'POST',
                data: {
                    start: Number(+app.start / 1000),
                    CarrierId: app.CarrierId,
                    BuildingId: app.BuildingId
                },
                context: this,
                success: function (data, textStatus, jqXHR) {
                    if (jqXHR.status == 203) {
                        // handling validation error. Never expected
                        alert(data);
                        return;
                    }
                    this.element.fullCalendar('refetchEvents');
                    var self = this;
                    this._loader.done(function () {
                        // Select the newly created appointment
                        if ($.type(data.start) == 'string') {
                            data.start = $.fullCalendar.parseDate(data.start);
                        }
                        self.selectAppointment(data);
                    });
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert(jqXHR.responseText);
                }
            });
        },
        rerenderEvents: function () {
            this.element.fullCalendar('rerenderEvents');
        },

        // Returns the selected appointment, or null if nothing is selected
        selectedAppointment: function () {
            var x = this.element.fullCalendar('clientEvents', function (app) {
                if (app.className == 'ui-selected') {
                    return true;
                }
            });
            if (x.length == 0) {
                return null;
            }
            return x[0];
        }
    });
});

$(function () {



    // Applied to tbody. Provides enhanced selection capabilities of rows.
    // The refresh() method can be called to display a popup indicating how many matching BOLs exist. It is
    //    passed the appointment for which BOLs need to be matched. The popup allows the user to actually select the matching rows,
    //    find a matching appointment, and if a matching appointment is not found, provide the option to create it.
    // This widget does not contain any id hardwiring.
    $.widget("ui.selectable", $.ui.selectable, {
        // default options
        options: {
            dialog: null,   // Selector of the associated dialog
            appointment: null,       // function which gets a row and returns appointment
            // Event raised when matching rows have been selected. ui.app contains appointment which was matched
            matched: null,

            // Selectable options
            filter: 'tr:not(.ui-state-disabled)',
            cancel: 'a,td:has(.ui-icon-close),td:has(.ui-icon-trash),td:has(.ui-icon-calendar),tr.ui-state-disabled'
        },

        _dlg: null,  // Jquery object representing the associated dialog
        _appointment: null,  // The appointment associated with the associated dialog. Set by refreshPopup(). Used by _synchronize()

        // the constructor
        _create: function () {
            this._super();
            var self = this;
            this._dlg = $('#popup');
            this._on(this._dlg, {
                "click": "_synchronize"
            });
        },

        _trigger: function (type, event, data) {
            switch (type) {
                case 'stop':
                    var $tr = $('tr', this.element);
                    $tr.removeClass('ui-state-highlight');
                    var $selected = $tr.filter('.ui-selected');
                    var app;
                    if ($selected.length == 1) {
                        app = this.options.appointment($selected);
                    }
                    this.refreshPopup(app, event);
                    break;
            }
            this._super(type, event, data);
        },
        // Called when the popup is clicked. Selects the matching rows and looks for a matching appointment.
        // If no matching appointment is found, it creates a dummy appointment which the user can choose to actually create.
        _synchronize: function (e) {
            if ($(e.target).is('.ui-icon-close')) {
                // Close icon hides the tip
                this._dlg.hide('explode');
                return;
            }
            if (!$(e.target).is('a')) {
                // Handle the click on the select link only
                return;
            }
            if (!this._appointment) {
                alert('Should never happen');
                return;
            }
            var self = this;
            $(this.options.filter, this.element).filter(function () {
                return AppointmentSimilar(self.options.appointment($(this)), self._appointment);
            }).addClass('ui-selected');
            this._trigger('matched', e, { app: self._appointment });
        },

        // If app is passed, all rows matching app are counted. If app is not passed the popup is hidden.
        // The popup is placed near the passed event
        refreshPopup: function (app, event) {
            var count;
            if (app) {
                // Find matching rows
                var self = this;
                count = $(this.options.filter, this.element).filter(function () {
                    return AppointmentSimilar(self.options.appointment($(this)), app);
                }).length;
            } else {
                // Pretend that no rows matched
                count = 0;
            }

            if (count > 0) {
                // Update the contents of the popup.
                this._dlg.show()
                    .find('span:first')
                    .html(count + ' matching BOL')
                    .end()
                    .find('[data-for=title]').text(app.title)
                    .end()
                    .position({
                        my: 'left bottom',
                        at: 'left top',
                        of: event
                    })
                    .effect('slide', {}, 500);
                this._appointment = app;
            } else {
                this._dlg.hide();
                this._appointment = null;
            }
        }
    });
});

$(document).ready(function () {
    // set up calendar
    $('#calendar').calendar({
        initialAppointment: _initialAppointment,
        click: function (event, ui) {
            $('#tbody').selectable('refreshPopup', ui.app, event.originalEvent);
        }
    });

    // Manage enable state of assign/unassign buttons and show count of selected BOLs.
    function ManageApplyState() {
        if (!$('#dlgCalendar').dialog('isOpen')) {
            // Do nothing
            return;
        }
        var $selected = $('#tbody tr.ui-selected');
        var enabled = $('#calendar').calendar('selectedAppointment') != null && $selected.length > 0;
        var $btn = $('#btnApply').prop('disabled', !enabled).toggleClass('ui-state-disabled', !enabled);
        if (enabled) {
            $('span.ui-button-text', $btn).text('Assign ' + $selected.length + ' BOL');
        } else {
            $('span.ui-button-text', $btn).text('Assign BOL');
        }
    }
    // set up dialog for assigning appointments
    $('#dlgCalendar').dialog({
        title: "Pick Appointment",
        autoOpen: _initialAppointment ? true : false,
        width: 600,
        height: 450,
        modal: false,
        cache: false,
        closeOnEscape: true,
        dialogClass: 'dlg-appointment',
        position: { my: 'right center', at: 'right top' },
        create: function (event, ui) {
            var self = this;
            // Capture change events needed to manage enabled state of the Apply button
            $('#tbody').bind('selectablestop', function (event, ui) {
                ManageApplyState();
            });
            $('#calendar').bind('calendarchange', function (event, ui) {
                ManageApplyState();
            });
        },
        open: function (event, ui) {
            // Bug hide:Needed to display events
            $('#calendar').calendar('rerenderEvents');
            ManageApplyState();
        },
        buttons:
        [
           {
               text: 'Assign BOL',
               id: 'btnApply',
               click: function (event, ui) {
                   var $form = $('#frmBol')
                   var app = $('#calendar').calendar('selectedAppointment');
                   if (!app) {
                       alert('No Appointment selected');
                       return;
                   }
                   $('#hfAppointmentId').val(app.id);
                   $('#hfAssignFlag').val('true');
                   var $selected = $('#tbody tr.ui-selected input:checkbox').clone().prop('checked', true).appendTo($form);
                   if ($selected.length == 0) {
                       alert('Please select some BOLs');
                       return;
                   }
                   //$selected.clone().appendTo($form);
                   $(this).dialog('close');
                   $form.submit();
               }
           },
        {
            text: 'Cancel',
            click: function (event, ui) {
                $('#dlgCalendar').dialog('close');
            }
        }]
    });

    $('#tbody').on('click', 'a.ui-icon-close', function (e) {
        //For single Bol deletion.
        $(this).parents('tr').addClass('ui-selected ui-selectee');
        //Getting the list of removable bol.
        var shippingIdList = $('#tbody tr.ui-selected').map(function () {
            return $(this).attr('data-shippingid');
        }).get();
        if (!confirm("Do you want to delete " + shippingIdList.length + " BOLs")) {
            return false;
        }
        var url = $(this).attr('data-url-delete');
        url = url.replace('X', shippingIdList.join(','));
        // ajax call to delete BOL
        $.ajax({
            cache: false,
            url: url,
            type: 'POST',
            context: this,
            success: function (data, textStatus, jqXHR) {
                if (jqXHR.status == 203) {
                    $('#divAjaxError').html(data).addClass('ui-state-highlight');
                    return false;
                }
                $('#tbody').find('tr.ui-selected').addClass('text-changed ui-state-disabled').removeClass('ui-selected ui-selectee')
                         .find('a').removeClass('ui-icon ui-icon-close ui-icon-calendar').text('Done');
                ManageApplyState();
                $('#divAjaxError').html(data).addClass('ui-state-highlight');
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR.responseText);
            }
        });
        //Due to using ancher tag with close icon.
        return false;
    }).selectable({
        appointment: function ($tr) {
            // Returns the appointment object associated with the first row in the set of context rows
            var app = _appointments[$tr.attr('data-shippingid')];
            app.start = $.fullCalendar.parseDate(app.start);
            return app;
        },
        dialog: '#automatch',   // Selector of the associated dialog
        calendar: '#calendar',   // Selector of the calendar
        matched: function (event, ui) {
            $('#dlgCalendar').dialog('option', 'position', { my: "left-bottom", at: "rigth", of: window }).dialog('open');
            $('#calendar').calendar('selectAppointment', ui.app);
        }
    });
    $('table thead input:checkbox').click(function (e) {
        // // Select/unselect all rows, check/uncheck all checkboxes
        var $tr = $('tbody tr.ui-selectee', $(this).closest('table'));
        $tr.removeClass('ui-state-highlight');
        var checked = $(this).is(':checked');
        $tr.toggleClass('ui-selected', checked);
        // Tell everyone that we are done modifying selections
        $('#tbody').trigger('selectablestop');
    });


    $('#btnAppointments').click(function (e) {
        $('#dlgCalendar').dialog('open');
    }).button();


    $('#cbShowScheduled').change(function (e) {
        $('#frmShowScheduled').submit();
    });


    $('#tbody').on('click', 'a.ui-icon-calendar', function (e) {
        //Getting the selected bol.
        var $selectedRow = $(e.target).closest('tr').addClass('ui-selected ui-selectee');
        var $selectedShippingId = $('#tbody tr.ui-selected input:checkbox').clone().prop('checked', true);
        if (!confirm("Do you want to unassign appointment from " + $selectedShippingId.length + "BOL")) {
            return false;
        }
        //Remove appointment from BOL
        var $form = $('#frmBol')
        $('#hfAssignFlag').val('false');
        var $selected = $selectedShippingId.appendTo($form);
        if ($selected.length == 0) {
            alert('Please select some BOLs');
            return;
        }
        $form.submit();
    })
});

