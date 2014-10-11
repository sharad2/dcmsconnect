/*/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

This function should be called from the onload handler of the body element.
It sets focus to the first text box in the document.
Define the javascript global _emptyOk to true if you want a postback to occur even if the text box is empty.

It also makes it behave like a scan text box which mneans that focus is automatically set to it if a key is pressed outside
a text box.
*/
function OnBodyLoad(body) {
    //Seperate OnBodyKeyPress has been written as Body.onKeyPress is not getting invoked.
    // If a key matching task number is pressed, open that UI.
    //    body.onkeypress = function (e) {
    //        var keyCode = e.keyCode
    //        if (keyCode == 49) {
    //            //  1 is pressed
    //            var a = document.getElementById("item1");
    //            var url = a.getAttribute('href');
    //            window.location = url;
    //        }
    //        else if (keyCode == 50) {           
    //            //  2 is pressed
    //            var a = document.getElementById("item2");
    //            var url = a.getAttribute('href');
    //            window.location = url;
    //        }
    //        else if (keyCode == 51) {         
    //            //  3 is pressed
    //            var a = document.getElementById("item3");
    //            var url = a.getAttribute('href');
    //            window.location = url;
    //        }
    //    };

    //    return true;

}
function OnBodyKeyPress() {
    var a, url;
    if (window.event.keyCode == 49) {
        //  1 is pressed
        a = document.getElementById("item1");
        url = a.getAttribute('href');
        window.location = url;
    }
    else if (window.event.keyCode == 50) {
        //  2 is pressed
        a = document.getElementById("item2");
        url = a.getAttribute('href');
        window.location = url;
    }
    else if (window.event.keyCode == 51) {
        //  3 is pressed
        a = document.getElementById("item3");
        url = a.getAttribute('href');
        window.location = url;
    }
    else if (window.event.keyCode == 52) {
        //  4 is pressed
        a = document.getElementById("item4");
        url = a.getAttribute('href');
        window.location = url;
    }
}

