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

$(document).ready(function () {
    $('button').button();

    $('#btnSelectAll').click(function (e) {
        var $tr = $('tbody tr.ui-selectee');
        if ($(this).is(':checked')) {
            $tr.addClass('ui-selected')
                .find('input:checkbox').prop('checked', true);
        } else {
            $tr.removeClass('ui-selected')
                .find('input:checkbox').removeAttr('checked');
        }
    });


    // Handle selectable event on selected orders.
    $('#pickslipListBody').selectable({
        filter: 'tr',
        cancel: 'a',
        stop: function (event, ui) {
            $('tr', this).each(function () {
                //Check the hidden check boxes for each row selected by user.
                if ($(this).is('.ui-selected')) {
                    $('input:checkbox', this).prop('checked', true);
                } else {
                    $('input:checkbox', this).removeAttr('checked');
                }
            });
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

