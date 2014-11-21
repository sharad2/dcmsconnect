$(document).ready(function () {

    var _refreshIndex = (new Date()).getTime();  //Increment this to force reloads

    $('#bucketModelPartial').bucketmodel();

    // Filter all detail lists by the type of pieces clicked
    $('#bucketModelPartial,#clearFilter').on('click', 'div[data-activity] .ui-progressbar-value', function (e) {
        var $tabs = $('#divTabs');
        var values = {
            state: $(this).attr('data-boxstate'),  // Which state was clicked? Complete or inprogress?
            activity: $(this).closest('div[data-activity]').attr('data-activity') // Which activity is this progress bar for? Pitching or pulling
        };
        // Change the href of each tab
        $('> ul > li > a', $tabs).each($.proxy(function (i, elem) {
            // Each tab header is supposed to contain the name of the parameters to modify in the URL
            var activityParam = $(elem).attr('data-activity-param');
            var stateParam = $(elem).attr('data-state-param');
            var href = $(elem).attr('href').replace(new RegExp(activityParam + '=\\w+'), activityParam + "=" + this.activity);
            href = href.replace(new RegExp(stateParam + '=\\w+'), stateParam + "=" + this.state);
            $(elem).attr('href', href);
        }, values));
        // reload the current tab
        _refreshIndex++;
        $tabs.tabs('load', $tabs.tabs('option', 'active'));
        $('div[data-activity] .ui-progressbar-value', e.delegateTarget);
        //$(this).addClass('ui-state-active');

        //showing applied filter
        $('#filterStatus')
            .find('div[data-activity]').hide().end()
            .find('div[data-activity=' + values.activity + '][data-boxstate=' + values.state + ']').show();
        //Show clear filter link if filter is being applied
        if ($('#filterStatus div[data-activity]:visible').length > 0) {
            $('#clearFilter').show();
        } else {
            $('#clearFilter').hide();
        }
    }).on('click', 'a.edit-wave', function (e) {
        // Freeze warning
        $('#hfDisplayEditable').val("True");
        $('#dlgFreeze').dialog('open');
        return false;
    });

    $('#divTabs').tabs({
        active: $('#divTabs').attr('data-active-tab'),
        // Honor _refreshIndex. Show progress image
        beforeLoad: function (event, ui) {
            var $img = $('<img></img>').attr({
                src: $(this).attr('data-load-image-url'),
                alt: 'Loading..'
            });
            ui.panel.html($img[0].outerHTML + '<br/><strong>Please Wait..</strong>');
            ui.panel.removeClass('ui-state-error');
            ui.jqXHR.error(function (jqXHR, textStatus, errorThrown) {
                ui.panel.html(jqXHR.responseText).addClass('ui-state-error');
            });
            ui.ajaxSettings.url = ui.ajaxSettings.url.replace(/_=\d*/g, '_=' + _refreshIndex);
        }
    });
    $('#btnGo').button();
    $('#btnSave').button({ icons: { primary: 'ui-icon-disk' } });

    $('#dlgFreeze').dialog({
        autoOpen: false,
        closeOnEscape: true,
        modal: true,
        buttons: [
            {
                text: 'OK',
                click: function () {
                    $('form', this).submit();
                }
            }, {
                text: 'Cancel',
                click: function () {
                    $(this).dialog('close');
                }
            }
        ]
    });
    $('#actionMenu').menu({
        select: function (event, ui) {
            var $icon = $('span.ui-icon', ui.item);
            if ($icon.is('.ui-icon-arrowthick-1-n')) {
                $('#bucketModelPartial').bucketmodel('increasePriority');
            } else if ($icon.is('.ui-icon-arrowthick-1-s')) {
                $('#bucketModelPartial').bucketmodel('decreasePriority');
            } else if (ui.item.is('.freeze-action')) {
                $('#hfDisplayEditable').val("False");
                $('#dlgFreeze').dialog('open');
            } else if ($icon.is('.ui-icon-pencil') && !$('a', ui.item).attr('href')) {
                // Freeze warning
                $('#hfDisplayEditable').val("True");
                $('#dlgFreeze').dialog('open');
                //alert('You must freeze the pick wave before attempting to edit it');
            }
        }
    });


    $('#unfreezeWave').click(function (e) {
        $('#actionMenu .freeze-action .ui-icon-unlocked').click();
    });

    // Post call to remove pickslip action method
    $('#divTabs').on('click', 'table tbody tr span[data-pickslip-remove-url]', function (e) {
        if (confirm('Are you sure, you want to remove ' + $(this).attr('data-pickslip-val') + ' pickslip from pick wave?')) {
            $.ajax($(this).attr('data-pickslip-remove-url'), {
                type: 'POST',
                success: function (data, textStatus, jqXHR) {
                    // handling validation error 
                    if (jqXHR.status == 203) {
                        alert(jqXHR.responseText);
                        return;
                    }
                    location.reload(true);
                    return;
                }
            }).error(function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR.responseText);
            });
        } else {
            return;
        }
    });
});
$(document).ready(function () {
    "use strict";
    $('#freezeModal').on('show.bs.modal', function (e) {
        //alert($(e.relatedTarget).data('bucketid'));
        $('#hfBucketid', e.delegateTarget).val($(e.relatedTarget).data('bucketid'));
    }).on('click', 'button:not([data-dismiss])', function (e) {
        //alert('submit');
        $('form', e.delegateTarget).trigger('submit');
    });
    $(document).on('click', 'button[data-priority-url]', function (e) {
    
       // alert($(e.target).data('priority-url'));
        $.post($(e.target).data('priority-url')).done(function (data, textStatus, jqXHR) {
           // alert(data);
            $(e.target).closest('div.input-group').find('input:text').val(data);
        }).error(function () {
            alert('error');
        });
    });
});



/*
Provides method and event to Increase or Decrease priority bucket
*/
//$.widget("custom.bucketmodel", {
//    options: {
//      //priorityIncreased: null
//    },
//    // the constructor
//    _create: function () {
//        this._on({            
//            'click div.div-bucket >span.ui-icon-arrowthick-1-s,div.div-bucket >span.ui-icon-arrowthick-1-n': function (e) {
//                this._doChangePriority(e.target);
//            }
//        });
//    },
//    _doChangePriority: function (icon) {
//        //Increase or Decrease the pick wave priority
//        $.ajax($(icon).closest('div.div-bucket').attr($(icon).is('.ui-icon-arrowthick-1-n') ? 'data-increase-priority-url' : 'data-decrease-priority-url'), {
//            cache: false,
//            type: 'POST',
//            context: icon,
//            beforeSend: function (jqXHR, settings) {
//                $(settings.context).closest('div.div-bucket').addClass('ui-state-highlight');
//            }
//        }).done(function (data, textStatus, jqXHR) {
//            $(this).closest('div.div-bucket').removeClass('ui-state-highlight').find('span.span-Priority').html(data);
//        }).error(function (data, textStatus, jqXHR) {
//            $(this).closest('div.div-bucket').removeClass('ui-state-highlight');
//            alert(data.responseText);
//        });
//    },    
//    increasePriority: function () {
//        var $icon = $('span.ui-icon-arrowthick-1-n', this.element);
//        this._doChangePriority($icon[0]);
//    },
//    decreasePriority: function () {
//        var $icon = $('span.ui-icon-arrowthick-1-s', this.element);
//        this._doChangePriority($icon[0]);
//    }
//});

$(document).ready(function () {
    $("#tbPriority").spinner({
        spin: function (event, ui) {
            if (ui.value > 99) {
                $(this).spinner("value", 1);
                return false;
            } else if (ui.value < 1) {
                $(this).spinner("value", 99);
                return false;
            }
        }
    });
});
