/*
These global variables must be set before this script is loaded

_rcBaseUrl: URL of the RC website home page. This should be an absolute URL without the trailing /
_rcItemsUrl: URL which will return the json array of RC items. This should be a relative to _rcBaseUrl and must begin with /
*/

$(document).ready(function () {
    $("#footermenu").toolbar({ theme: "a" });   
});

// Get a list of items available on the release candidate site and show the RC link for each of the items available
$(document).one('pagecreate', function () {
    if (!_rcBaseUrl) {
        // No RC URL specified. Nothing to do
        //$('#rclink').hide();
    }
   
    //TODO: Show Contacting on RC button
    $.ajax(_rcBaseUrl + _rcItemsUrl, {
        dataType: 'jsonp',
        crossDomain: true
    }).done(function (data, textStatus, jqXHR) {
        // The call to RC was successful. Show the RC link at the bottom of the page
        // data is an array of {route: "DCMSConnect_App1", url: "/Inquiry/Home/Index"}
        // Show the rc link against each menu items
        $('a.rclink').show().find('span').text('(' + data.length + ')');
        $.each(data, function (e, ui) {
            var $a = $('<a></a>').attr({
                href: _rcBaseUrl + this.url,
                class: 'ui-btn ui-icon-comment ui-btn-icon-left'
            }).html('<em>RC</em>');
            $('div.' + ui.route + ' a:last').after($a);
        });        
        $('div.ui-page-active div[data-role="controlgroup"]').controlgroup("refresh");
        //TODO: Show item count on RC button
    }).fail(function (jqXHR, textStatus, errorThrown) {
        // TODO: Show error on RC button
        $('#linkRc small').text('Contact failed with error ' + jqXHR.status);
    });     
}).on("pagecontainershow", function (event, ui) {
    // Code taken from View source of page http://demos.jquerymobile.com/1.4.3/toolbar-fixed-persistent/
    // Find the id of the page which is currently active
    var curpageId = $("div.ui-page-active").attr('id');
    //alert(curpageId);
    // Remove active class from nav buttons
    $("#navbarFixed a.ui-btn-active").removeClass("ui-btn-active");
    // Add active class to nav button of current page.
    // We find the button whose href points to the id of the current page
    $("#navbarFixed a[href='#" + curpageId + "']").addClass("ui-btn-active");
});



