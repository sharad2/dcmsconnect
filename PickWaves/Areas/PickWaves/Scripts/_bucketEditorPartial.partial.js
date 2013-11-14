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