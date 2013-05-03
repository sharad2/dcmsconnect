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
