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

/*
This function should be called from the onload handler of the body element.
It sets focus to the first text box in the document.
Define the javascript global _emptyOk to true if you want a postback to occur even if the text box is empty.

It also makes it behave like a scan text box which mneans that focus is automatically set to it if a key is pressed outside
a text box.
*/
function InitScanTextBox(body) {
    var inputs = document.getElementsByTagName('input');
    if (inputs.length == 0) {
        // Do nothing if there are multiple input elements
        return;
    }
    // Set focus to first input box when the page loads
    inputs[0].focus();

    // Find the first scan text box on the page
    var tbScan = null;
    for (var i = 0, j = inputs.length; i < j && !tbScan; ++i) {
        inputs[i].getAttribute("data-scan") == "true" && (tbScan = inputs[i]);
    }

    if (tbScan == null) {
        // No problem if none found
        return;
    }

    // If a key is pressed when the focus is not on the input box, set focus to the input
    body.onkeypress = function () {
        if (window.event.srcElement != tbScan) {
            tbScan.value = '';
            tbScan.focus();
        }
    };

    if (!tbScan.onkeypress) {
        tbScan.onkeypress = function () {
            // Cancel bubble to make sure that body.onkeypress is not invoked
            window.event.cancelBubble = true;
            return true;
        };
    }
}


