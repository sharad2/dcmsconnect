// This function must be called from onkeypress handler. It redirects when enter is pressed
function RedirectToChoice(choice) {
    //window.event.cancelBubble = true;
    if (window.event.keyCode == 13) {
        var a = document.getElementById("choice_" + choice.value);
        if (a) {
            window.location = a.href;
        } else {
            // Unreasonable choice. Redirect to choice_default
            a = document.getElementById("choice_default");
            if (a) {
                window.location = a.href.replace('X', choice.value);
            }
            var b = document.getElementById('tb');
            b.value = '';
            return false;
        }
        return false;
    }
}
