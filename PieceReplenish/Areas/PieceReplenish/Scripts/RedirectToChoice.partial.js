// This function must be called from onkeypress handler. It redirects when enter is pressed
function RedirectToChoice(e, choice) {
    var key = e.keyCode || e.which;  // Firefox uses e.which
    if (key == 13) {
        var a = document.getElementById("choice_" + choice.value);
        if (a) {
            window.location = a.href;
            window.event.cancelBubble = true;
            return false;
        } else {
            // Unreasonable choice. Clear the value
            //choice.value = '';
            return true;
        }
    }
}
