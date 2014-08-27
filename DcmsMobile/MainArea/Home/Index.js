/*
These global variables must be set before this script is loaded

_rcBaseUrl: URL of the RC website home page. This should be an absolute URL without the trailing /
_rcItemsUrl: URL which will return the json array of RC items. This should be a relative to _rcBaseUrl and must begin with /
*/
// Sharad 27 Aug 2014: This script is not working after conversion to bootstrap

// Get a list of items available on the release candidate site and show the RC link for each of the items available
$(document).ready(function () {
    if (!_rcBaseUrl) {
        // No RC URL specified. Nothing to do
        return;
    }
   
    $('#rclinkText').text('Contacting...');
    return;
    $.ajax(_rcBaseUrl + _rcItemsUrl, {
        dataType: 'jsonp',
        crossDomain: true
    }).done(function (data, textStatus, jqXHR) {
        // The call to RC was successful. Show the RC link at the bottom of the page
        // data is an array of {route: "DCMSConnect_App1", url: "/Inquiry/Home/Index"}
        // Show the rc link against each menu items
        $('a.rclink').show().find('span').text('(' + data.length + ')');
        $.each(data, function (e, ui) {
            $('#' + ui.route).attr('href', _rcBaseUrl + this.url).removeClass('invisible');
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        // TODO: Show error on RC button
        $('a.rclink span').text('Contact failed with error ' + jqXHR.status)
            .addClass('validation-summary-errors');
    });     
});





