$(document).ready(function () {
    $('#auditContents,#auditContentsLocAssg').on('shown.bs.collapse', function (e) {
        // Change + icon to -
        $('a[href="#' + e.target.id + '"] span').removeClass('glyphicon-plus').addClass('glyphicon-minus');
        // Scroll to the top of the collapsible content
        $('html, body').animate({
            scrollTop: $('#' + e.target.id).offset().top
        }, 2000);
    }).on('hidden.bs.collapse', function (e) {
        // Change - icon to +
        $('a[href="#' + e.target.id + '"] span').removeClass('glyphicon-minus').addClass('glyphicon-plus');
    }).one('shown.bs.collapse', function (e) {
        // Load collapsible contents via AJAX
        $('div.ajaxContent', this).load($(this).attr('data-url'), function (response, status, xhr) {
            if (status == "error") {
                var msg = "Sorry but there was an error: ";
                alert(msg + xhr.status + " " + xhr.statusText);
            }
        });
    });
});
