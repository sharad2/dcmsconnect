///#source 1 1 /Areas/PickWaves/Scripts/ManageWave-Index.partial.js
$(document).ready(function () {
    $('ul.waveActionMenu').menu({
        select: function (event, ui) {           
            if (ui.item.is('.increase-action')) {
                ui.item.closest('tr').find('div.bucketModelPartial').bucketmodel('increasePriority');
                return false;
            } if (ui.item.is('.decrease-action')) {
                ui.item.closest('tr').find('div.bucketModelPartial').bucketmodel('decreasePriority');
                return false;
            }            
        }
    }).on('mouseleave', function (e) {
        $(this).menu("collapseAll", null, true);
    });

    $('div.bucketModelPartial').bucketmodel();
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