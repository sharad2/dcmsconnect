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