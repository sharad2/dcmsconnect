///#source 1 1 /Areas/BoxPick/Scripts/session.partial.js
// Wrapping all code in an anonymous function to make the variables used private.
// This is a standard javascript trick.
(function () {
    var timeoutValueElem = null;
    var currentTimeout;
    var intervalInSeconds = false;
    function UpdateSessionTimeout() {
        if (!timeoutValueElem) {
            timeoutValueElem = document.getElementById("timeoutValue");
            currentTimeout = timeoutValueElem.innerHTML;    // In minutes
        }
        currentTimeout = currentTimeout - 1;
        if (currentTimeout == 1) {
            if (!intervalInSeconds) {
                clearInterval(intervalId);
                intervalId = setInterval(UpdateSessionTimeout, 1000);
                intervalInSeconds = true;
                currentTimeout = 60;
                document.getElementById("timeoutUnit").innerHTML = 'seconds';
                document.getElementById("timeout").className = "ui-state-error";
            }
        } else if (currentTimeout <= 0) {
            clearInterval(intervalId);
            currentTimeout = "Idle Timeout";
            document.getElementById("timeoutUnit").innerHTML = '';
        }
        timeoutValueElem.innerHTML = currentTimeout;
    }
    var intervalId = setInterval(UpdateSessionTimeout, 60000);
})();



//$Id$
///#source 1 1 /Areas/BoxPick/Scripts/scan.partial.js
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
    inputs[0].value = "";
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

    tbScan.onkeypress = function () {
        // Cancel bubble to make sure that body.onkeypress is not invoked
        window.event.cancelBubble = true;
        // Postback on enter, unless _emptyOk says otherwise
        if (window.event.keyCode == 13 && tbScan.value == '') {
            if (typeof (_emptyOk) === "undefined" || !_emptyOk) {
                return false;    // eat the enter key
            }
        }
        return true;
    };
}



//$Id$
