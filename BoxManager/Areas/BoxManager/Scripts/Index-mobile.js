function RedirectToChoice() {
    if (window.event.keyCode == 13) {
        var choice = document.getElementById("scan").value.toString().toUpperCase();
        //Redirect to url using choice given in list
        var element = document.getElementById(choice);
        if (element) {
            window.location = element.getAttribute('href');
            return true;
        }
        //Redirect to url using short name given in choice menu
        if (!element) {
            element = document.getElementsByName(choice);
            if (element.length > 0) {
                window.location = element[0].getAttribute('href');
                return true;
            }
        }
    }
    return false;
}
