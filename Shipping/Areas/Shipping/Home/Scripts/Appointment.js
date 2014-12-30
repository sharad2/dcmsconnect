/// <reference path="../../../Scripts/jquery-1.8.2.intellisense.js" />
///$Id$
// Displays time in a jquery spinner. Used for entering arrival time and appointment time.
$.widget("ui.timespinner", $.ui.spinner, {
    options: {
        step: 15 * 60,  // Number of seconds in 15 minutes
    },
    _parse: function (time) {
        // Parses a string into a numberic UNIX timestamp
        if (!time) {
            return null;        // Empty value is null
        }
        switch ($.type(time)) {
            case 'string':
                // e.g. 2:40 AM which was returned by _format
                var timeObject = time.split(/[\s:]+/);
                if (timeObject[2] == 'PM' && Number(timeObject[0]) < 12) {
                    timeObject[0] = (Number(timeObject[0]) + 12) % 24;
                } else if (timeObject[2] == 'AM' && timeObject[0] == 12) {
                    // Change 12:xx AM to 0:xx
                    timeObject[0] = 0;
                }
                time = new Date(1970, 1, 1, timeObject[0], timeObject[1]);
                // fall through

            case 'date':
                return Number(time) / 1000;

            case 'number':
                return time;

            default:
                alert('Unexpected parse in timespinner');
        }
    },
    _format: function (value) {
        // Expects a UNIX timestamp and returns a readable time in string format
        value = new Date(value * 1000);
        value = $.fullCalendar.formatDate(value, 'h:mm TT');
        return value;
    }
});

$(document).ready(function () {
    // Dialog to create/edit appointment
    var _loader;
    $('#dlgAppointment').dialog(
     {
         autoOpen: false,
         width: 'auto',
         modal: true,
         cache: false,
         closeOnEscape: true,
         create: function (event, ui) {
             $('#tbAppointmentDate').datepicker({
                 showOn: "button",
                 buttonImage: $('#tbAppointmentDate').attr('data-image-url'),
                 buttonImageOnly: true,
                 dateFormat: 'm/d/yy',
                 onSelect: function () { $(this).change(); }
             });
             $("#tbAppointmentTime").timespinner();
         },
         // Populates input controls based on the value of custom option currentEvent
         open: function (event, ui) {
             // Copy current event values to text boxes
             var currentEvent = $(this).dialog('option', 'currentEvent');
             $('input:text, select, textarea', this).each(function (i) {
                 $(this).val(currentEvent[$(this).attr('name')]);
             });
             $('#tbAppointmentDate').val($.datepicker.formatDate('m/d/yy', currentEvent.start));
             if (currentEvent.id) {
                 // Updating event
                 $('#tbAppointmentTime').timespinner('value', currentEvent.start);
             } else {
                 // Adding event. Default 7:00 am. TODO: Remove hardwiring
                 $('#tbAppointmentTime').timespinner('value', 420);
             }
             $('#divAjaxError').html('').removeClass('field-validation-error').addClass('filed-validation-valid');
         },
         buttons: [{
             text: 'Save',
             click: function (event, ui) {
                 // Manually validate form
                 var bValid = true;
                 var b = $('#ddlBuilding').val();
                 $('#ddlBuilding').toggleClass('ui-state-error', !b);
                 bValid = bValid && b;

                 b = $('#tbAppointmentDate').val();
                 try {
                     // Make sure the date is valid
                     $.datepicker.parseDate('m/d/yy', b);
                 }
                 catch (e) {
                     b = false;
                 }
                 $('#tbAppointmentDate').toggleClass('ui-state-error', !b);
                 bValid = bValid && b;

                 var time = $('#tbAppointmentTime').timespinner('value')
                 b = time && $.isNumeric(time);
                 $('#tbAppointmentTime').toggleClass('ui-state-error', !b);
                 bValid = bValid && b;

                 // Post appointment if form is valid
                 if (!bValid) {
                     return;
                 }
                 var data = $('input:text, select, textarea', this).map(function (i) {
                     return {
                         name: $(this).attr('name'),
                         value: $(this).is('#tbAppointmentTime') ? $('#tbAppointmentTime').timespinner('value') : $(this).val()
                     };
                 }).get();
                 data.push({ name: 'id', value: $(this).dialog('option', 'currentEvent').id });
                 data.push({ name: 'RowSequence', value: $(this).dialog('option', 'currentEvent').RowSequence });

                 $.ajax({
                     url: $('#calendar').attr('data-createupdate-url'),
                     type: 'POST',
                     data: data,
                     context: this,
                     success: function (data, textStatus, jqXHR) {
                         // handling validation error 
                         if (jqXHR.status == 203) {
                             $('#divAjaxError').html(data).addClass('field-validation-error');
                             return false;
                         }
                         $(this).dialog('close');
                         $('#calendar').fullCalendar('refetchEvents');
                         var date = $.fullCalendar.parseDate(data.start);
                         $('#calendar').fullCalendar('gotoDate', date);
                         // Highlight recently added or updated event
                         _loader.done(function () {
                             var matches = $('#calendar').fullCalendar('clientEvents', data.id);
                             if (matches.length > 0) {
                                 matches[0].className = 'ui-selected';
                                 $('#calendar').fullCalendar('rerenderEvents');

                             }
                         });
                     },
                     error: function (jqXHR, textStatus, errorThrown) {
                         alert(jqXHR.responseText);
                     }
                 });
                 return false;
             }
         },
        {
            text: 'Cancel',
            click: function (event, ui) {
                $(this).dialog('close');
                return false;
            }
        }]
     });

    // Setup the calendar
    $('#calendar').each(function (i) {
        // The http request object used for loading events
        var self = this;

        // To force refetch when view changes
        var _previousViewName;


        var isUserLoggedIn = $(this).attr('data-userLogged-in');
        var initialDate = $(this).attr('data-initial-date');
        var id = $(this).attr('data-initial-appId');
        if (initialDate) {
            initialDate = $.fullCalendar.parseISO8601(initialDate,false);
        }
        $(this).fullCalendar({
            dayClick: function (date, allDay, jsEvent, view) {
                if ($(jsEvent.target).is('.app-addevent')) {
                    // Invoked when a new appointment is created.  
                    $('#dlgAppointment').dialog('option', { title: 'Create New Appointment', currentEvent: { start: date } })
                        .dialog('open');
                    // Handling case when + is clicked in week view.We don't want to change view in this case.
                    return;
                }
                else if ($(jsEvent.target).is('.fc-day-number')) {
                    // Show day view of the clicked date
                    $(self).fullCalendar('gotoDate', date)
                        .fullCalendar('changeView', 'basicDay');
                }
            },
            loading: function (isLoading, view) {
                $('#calendarLoading').toggleClass('ui-autocomplete-loading', isLoading).toggle(isLoading);
            },
            weekends: true,
            editable: false,
            disableResizing: true,
            defaultEventMinutes: 120,
            theme: true,
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,basicWeek,basicDay'
            },
            timeFormat: 'h:mm TT',
            ignoreTimezone: false,
            defaultView: initialDate ? 'basicDay' : 'basicWeek',    // If initial date is specified, show day view
            // Invoked when user clicks within an event
            eventClick: function (calEvent, jsEvent, view) {
                // pass appointment along with the event
                $(self).trigger(jsEvent, calEvent);
                // Prevent bubbling of the original event
                jsEvent.stopImmediatePropagation();
            },

            lazyFetching: true,
            allDayDefault: false,

            // Sets up the ajax call needed to fetch events
            events: function (start, end, callback) {
                var data = $('#divBuildings input:checkbox:checked,#divFilter input:radio:checked,#tbCarrierFilter,#divShipped input:checkbox:checked').map(function () {
                    return { name: $(this).attr('name'), value: $(this).val() }
                }).get();
                data.push({ name: $(self).attr('data-start-param'), value: Math.round(+start / 1000) });
                data.push({ name: $(self).attr('data-end-param'), value: Math.round(+end / 1000) });
                data.push({ name: $(self).attr('data-viewname-param'), value: $(self).fullCalendar('getView').name });

                // Save the return value of $.ajax so that we can later use it to execute functions after the ajax call has completed.
                // When we create an appointment, we refetch events and then search for the created event after the events have been fetched.
                _loader = $.ajax({
                    url: $(self).attr('data-event-url'),
                    dataType: 'json',
                    data: data,
                    traditional: true,
                    cache: false,
                    success: function (data, textStatus, jqXHR) {
                        callback(data);
                        // if intial appointment,highlight it
                        if (id) {
                            _loader.done(function () {
                                var matches = $('#calendar').fullCalendar('clientEvents', id);
                                if (matches.length > 0) {
                                    matches[0].className = 'ui-selected';
                                    $('#calendar').fullCalendar('rerenderEvents');
                                } //else {
                                    //alert('Appointment Number ' + val + ' not found');
                                //}
                            });
                        }

                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(jqXHR.responseText);
                        callback([]);       // Empty array
                    },
                });
            },

            // Add event html to the event display
            eventRender: function (event, element, view) {
                $('.fc-event-inner .fc-event-time', element).attr('title', event.Remarks);
                if (event.appointmentHtml) {
                    $('.fc-event-inner', element).append(event.appointmentHtml);
                }
                // Compare timezone of client and that of appointment if it's a scheduled appointment. 
                if (event.id) {
                    var appointmentDate = $.fullCalendar.parseDate(event.InitialDateIso);
                    var currentDate = new Date();
                    // Notify if user's timezone is different from zone in which appointment was created.
                    if ((event.OffSet.TotalMinutes + currentDate.getTimezoneOffset()) != 0) {
                        // We are adding offset of date at client side and that of appointmentdate beacuse Javascript date object returns an opposite sign (+,-) value  with respect to one returned by C#.
                        // e.g For UTC -4:0 Javascript date object's offset returns 240 minutes while C# gives -240
                        $('.fc-event-title', element).prepend('(Local:-' + $.fullCalendar.formatDate(appointmentDate, 'h:mm TT') + ')');
                        $('.fc-event-title', element).attr('title', event.AppointmentOffsetDisplay);
                    }
                }
            },
            viewDisplay: function (view) {
                // Show + sign on each day only if user is loggedIn.
                if (isUserLoggedIn == "True") {
                    $('td > div:not(:has(span.app-addevent))', this).prepend('<span title="Create a new event" class="app-addevent ui-icon ui-icon-plus"></span>');
                }
                // show refresh icon on day view.
                $('div.fc-view-basicDay thead th:not(:has(span.app-refresh))', this).prepend('<span title="Refresh" class="ui-icon ui-icon-refresh app-refresh"></span>');

                // Show tooltip in Week view header and month view 
                $('td div.fc-day-number, div.fc-view-basicWeek thead th', this).attr('title', 'Click to see the day view');
                
                if (_previousViewName && _previousViewName != view.name) {
                    // View has changed. Refetch events to ensure that the day html is displayed
                    $(this).fullCalendar('refetchEvents');
                }
                _previousViewName = view.name;
            },
            year: initialDate ? initialDate.getFullYear() : undefined,
            month: initialDate ? initialDate.getMonth() : undefined,
            date: initialDate ? initialDate.getDate() : undefined
        });
    }).on('click', 'span.ui-icon-close', function (e, app) {
        // Cancel editing of arrival time
        $(e.target).parent('span.arrival-editor')
            .find('input:text')
            .timespinner('destroy').end()
            .hide()
            .nextAll('a.arrival-editor-button').show();
    }).on('click', 'span.ui-icon-check', function (e, app) {
        // Update the arrival time
        var data = new Object();
        var $editor = $(e.target).parent('span.arrival-editor').find('input:text').each(function (i) {
            data[$(this).attr('name')] = $(this).timespinner('value');
        }).end();
        data[$(e.delegateTarget).attr('data-id-param')] = app.id;
        data[$(e.delegateTarget).attr('data-appointmentTime-param')] = Number(app.start) / 1000;
        $.ajax({
            url: $(e.delegateTarget).attr('data-updatearrival-url'),
            data: data,
            cache: false,
            type: 'POST',
            context: e.delegateTarget
        }).done(function (data, textStatus, jqXHR) {
            $(this).fullCalendar('refetchEvents');
        }).error(function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR.responseText);
        }).always(function () {
            $('span.ui-icon-close', $editor).click();
        });
    }).on('click', 'span.app-refresh', function (e, app) {
        // Refresh calendar
        $('#calendar').fullCalendar('refetchEvents');
    }).on('click', 'a.arrival-editor-button', function (e, app) {
        // Change arrival time button clicked. Show timespinner.
        $(e.target).prevAll('span.arrival-editor').show()
            .find('input:text').timespinner();      // timespinner will be removed in close
        $(e.target).hide();
        return false;
    }).on('click', 'a.app-edit', function (e, app) {
        // Show appointment editing dialog
        $('#dlgAppointment').dialog('option', { title: 'Edit Appointment', currentEvent: app })
           .dialog('open');
    }).on('click', 'a.app-delete', function (e, app) {
        // Deletion of appointment
        var result = confirm("Are you sure, you want to delete Appointment " + app.title + " ?");
        if (result) {
            $.ajax({
                url: $(e.delegateTarget).attr('data-delete-url').replace(-1, app.id),
                type: 'POST',
                data: app.id,
                success: function (data, textStatus, jqXHR) {
                    $('#divAppntDltdError').html(data).addClass('ui-state-highlight');
                    $('#calendar').fullCalendar('refetchEvents');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert(jqXHR.responseText);
                }
            });
        }
    }).on('click', 'div.fc-view-basicWeek thead', function (e) {
        // Switch from week view to the date which was clicked
        // e.currentTarget is thead. e.target is the th which was clicked
        // We are finding the index of the clicked th.
        var days = $('th', e.currentTarget).index($(e.target));
        var view = $(e.delegateTarget).fullCalendar('getView');
        var clickedDate = new Date(view.start);
        clickedDate.setDate(view.start.getDate() + days);
        $(e.delegateTarget).fullCalendar('gotoDate', clickedDate)
             .fullCalendar('changeView', 'basicDay');
    });

    $('#calendarLoading').position({
        of: $(window)
    });

    // Setup autocompletes
    $("#tbCarrier,#tbCarrierFilter").autocomplete({
        minLength: 0,
        source: function (request, response) {
            $.ajax({
                url: this.element.attr('data-list-url'),
                dataType: 'json',
                context: this.element,
                beforeSend: function (jqXHR, settings) {
                    this.addClass('ui-autocomplete-loading');
                },
                data: { term: request.term },
                success: function (data, textStatus, jqXHR) {
                    response(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    // Error encountered during the remote call. Just show some diagnostic. If this happens, system becomes unstable
                    alert(jqXHR.responseText);
                },
                complete: function (jqXHR, textStatus) {
                    this.removeClass('ui-autocomplete-loading');
                }
            });
        },
        change: function (event, ui) {
            $(this).change();
        },
        select: function (event, ui) {
            //$(this).val(ui.item.value);
            $('#divFilter').change();
        },
        autoFocus: true
    }).dblclick(function (e) {
        $(this).autocomplete('search');
    });

    // Whenever a filter changes, Requery after some delay so that the user can quickly manipulate multiple filters
    var _changeId;
    $('#divBuildings, #divFilter, #divShipped').on('change', function (e) {
        if (_changeId == null) {
            _changeId = setTimeout(function () {
                $('#calendar').fullCalendar('refetchEvents');
                _changeId = null;
            }, 1000);
        }
    });

    $('#tbGotoAppointmentDate').datepicker({
        showOn: "button",
        buttonImage: $('#tbGotoAppointmentDate').attr('data-image-url'),
        buttonImageOnly: true,
        dateFormat: 'm/d/yy',
        onSelect: function () {
            $(this).change();
        }
    }).change(function (e) {
        if ($(this).val()) {
            var date = new Date($(this).val());
            $('#calendar').fullCalendar('gotoDate', date).fullCalendar('changeView', 'basicDay');
        }
    });

    // Search for the entered appointment number and show it in day view
    $('#btnAppNumber').click(function (e) {
        var val = $('#tbAppointmentNumber').val();
        if (isNaN(val)) {
            alert("Appointment number must be a number.");
            $('#tbAppointmentNumber').val('');
            return;
        }
        if (!val) {
            $('#tbAppointmentNumber').val('');
            return;
        }
        $('#tbAppointmentNumber').val('');
        $.ajax({
            url: $(this).attr('data-url').replace('-1', val)
        }).done(function (app, textStatus, jqXHR) {
            if (jqXHR.status == 203) {
                alert('Appointment Number ' + val + ' not found');
                return;
            }
            //Need to ingnore time zone so that we can go to correct date on which appointment exist at client end
            var start = $.fullCalendar.parseISO8601(app.start, false);

            $('#calendar').fullCalendar('gotoDate', start).fullCalendar('changeView', 'basicDay');
            _loader.done(function () {
                var matches = $('#calendar').fullCalendar('clientEvents', app.id);
                if (matches.length > 0) {
                    matches[0].className = 'ui-selected';
                    $('#calendar').fullCalendar('rerenderEvents');
                } else {
                    alert('Appointment Number ' + val + ' not found');
                }
            });
        }).error(function (jqXHR, textStatus, errorThrown) {
            // Error encountered during the remote call. Just show some diagnostic. If this happens, system becomes unstable
            alert(jqXHR.responseText);
        });
    }).button();
});
