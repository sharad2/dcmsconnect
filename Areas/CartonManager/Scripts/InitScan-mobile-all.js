/*
This function should be called from the onload handler of the body element.
It sets focus to the first text box in the document.
Define the javascript global _emptyOk to true if you want a postback to occur even if the text box is empty.

It also makes it behave like a scan text box which mneans that focus is automatically set to it if a key is pressed outside
a text box.
*/
function OnBodyLoad(body) {
    // Find the first scan text box on the page
    var tbScan = document.getElementById('scan');
    tbScan.focus();
    tbScan.value = '';

    // If a key is pressed when the focus is not on the input box, set focus to the input
    //This fucntion is not getting invoked hence OnBodyKeyPress() has been defined separately
    body.onkeypress = function () {
        if (window.event.srcElement != tbScan) {
            tbScan.value = '';
            tbScan.focus();
        }
    };

    tbScan.onkeypress = function () {
        // Cancel bubble to make sure that body.onkeypress is not invoked
        window.event.cancelBubble = true;

        return true;
    };
}

// If a key is pressed when the focus is not on the input box, set focus to the input
function OnBodyKeyPress() {
    var tbScan = document.getElementById('scan');
    if (window.event.srcElement != tbScan) {
        tbScan.value = '';
        tbScan.focus();
    }

}

