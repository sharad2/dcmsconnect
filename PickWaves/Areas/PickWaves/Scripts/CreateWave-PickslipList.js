///#source 1 1 /Areas/PickWaves/Scripts/selectable.partial.js
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

///#source 1 1 /Areas/PickWaves/Scripts/CreateWave-PickslipList.partial.js
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
///#source 1 1 /Areas/PickWaves/Scripts/bucketModel.partial.js

/*
Provides method and event to Increase or Decrease priority bucket
*/
$.widget("custom.bucketmodel", {
    options: {
      //priorityIncreased: null
    },
    // the constructor
    _create: function () {
        this._on({            
            'click div.div-bucket >span.ui-icon-arrowthick-1-s,div.div-bucket >span.ui-icon-arrowthick-1-n': function (e) {
                this._doChangePriority(e.target);
            }
        });
    },

    // called when created, and later when changing options
   // _refresh: function () { },

    // events bound via _on are removed automatically
    // revert other modifications here
   // _destroy: function () { },
    // _setOptions is called with a hash of all options that are changing
    // always refresh when changing options
    //_setOptions: function () {
    //    // _super and _superApply handle keeping the right this-context
    //    this._superApply(arguments);
    //    this._refresh();
    //},
    // _setOption is called for each individual option that is changing
    //_setOption: function (key, value) {

    //    this._super(key, value);
    //},
    _doChangePriority: function (icon) {
        //Increase or Decrease the pick wave priority
        $.ajax($(icon).closest('div.div-bucket').attr($(icon).is('.ui-icon-arrowthick-1-n') ? 'data-increase-priority-url' : 'data-decrease-priority-url'), {
            cache: false,
            type: 'POST',
            context: icon,
            beforeSend: function (jqXHR, settings) {
                $(settings.context).closest('div.div-bucket').addClass('ui-state-highlight');
            }
        }).done(function (data, textStatus, jqXHR) {
            $(this).closest('div.div-bucket').removeClass('ui-state-highlight').find('span.span-Priority').html(data);
        }).error(function (data, textStatus, jqXHR) {
            $(this).closest('div.div-bucket').removeClass('ui-state-highlight');
            alert(data.responseText);
        });
    },    
    increasePriority: function () {
        var $icon = $('span.ui-icon-arrowthick-1-n', this.element);
        this._doChangePriority($icon[0]);
    },
    decreasePriority: function () {
        var $icon = $('span.ui-icon-arrowthick-1-s', this.element);
        this._doChangePriority($icon[0]);
    }
});