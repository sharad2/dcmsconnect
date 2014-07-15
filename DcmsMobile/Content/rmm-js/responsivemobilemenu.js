/*

Responsive Mobile Menu v1.0
Plugin URI: responsivemobilemenu.com

Author: Sergio Vitov
Author URI: http://xmacros.com

License: CC BY 3.0 http://creativecommons.org/licenses/by/3.0/

*/

function responsiveMobileMenu(page) {
    $('div.rmm', page).each(function () {
        $(this).children('ul').addClass('rmm-main-list');	// mark main menu list


        var $style = $(this).attr('data-menu-style');	// get menu style
        if (typeof $style == 'undefined' || $style == false) {
            $(this).addClass('graphite'); // set graphite style if style is not defined
        }
        else {
            $(this).addClass($style);
        }


        /* 	width of menu list (non-toggled) */

        var $width = 0;
        $(this).find('ul li').each(function () {
            $width += $(this).outerWidth();
        });

        // if modern browser
        // Sharad: leadingWhitespace does not work in jquery 2. So added || true for now
        if ($.support.leadingWhitespace || true) {
            $(this).css('max-width', $width * 1.05 + 'px');
        }
            // 
        else {
            $(this).css('width', $width * 1.05 + 'px');
        }

    });
}
function getMobileMenu(page) {

    /* 	build toggled dropdown menu list */

    $('.rmm', page).each(function () {
        var menutitle = $(this).attr("data-menu-title");
        if (menutitle == "") {
            menutitle = "Menu";
        }
        else if (menutitle == undefined) {
            menutitle = "Menu";
        }
        var $menulist = $(this).children('.rmm-main-list').html();
        var $menucontrols = "<div class='rmm-toggled-controls'><div class='rmm-toggled-title'>" + menutitle + "</div><div class='rmm-button'><span>&nbsp;</span><span>&nbsp;</span><span>&nbsp;</span></div></div>";
        $(this).prepend("<div class='rmm-toggled rmm-closed'>" + $menucontrols + "<ul>" + $menulist + "</ul></div>");

    });
}

function adaptMenu(page) {

    /* 	toggle menu on resize */

    $('.rmm', page).each(function () {
        var $width = $(this).css('max-width');
        $width = $(this).width();
        //$width = $width.replace('px', ''); 
        if ($(this).parent().width() < $width * 1.05) {
            $(this).children('.rmm-main-list').hide(0);
            $(this).children('.rmm-toggled').show(0);
        }
        else {
            $(this).children('.rmm-main-list').show(0);
            $(this).children('.rmm-toggled').hide(0);
        }
    });

}

$(document).on('pagecontainertransition', function (event, ui) {
    //alert('Hi');
    var x = ui.toPage;  // $('div.ui-page-active');
    if (x.hasClass('Sharad')) {
        // Nothing to do because the page has already been shown before
        return;
    }
    responsiveMobileMenu(x);
    getMobileMenu(x);
    adaptMenu(x);

    /* slide down mobile menu on click */
    x.on('click', '.rmm-toggled.rmm-closed', function (e) {
        alert('click1');
        $(this).find('ul').stop().show(300);
        $(this).removeClass("rmm-closed");
    }).on('click', '.rmm-toggled:not(.rmm-closed)', function (e) {
        alert('click2');
        $(this).find('ul').stop().hide(300);
        $(this).addClass("rmm-closed");
    }).addClass('Sharad');  // Remember that the page has been shown before;

    //$('.rmm-toggled, .rmm-toggled .rmm-button', x).click(function () {
    //    if ($(this).is(".rmm-closed")) {
    //        $(this).find('ul').stop().show(300);
    //        $(this).removeClass("rmm-closed");
    //    }
    //    else {
    //        $(this).find('ul').stop().hide(300);
    //        $(this).addClass("rmm-closed");
    //    }

    //});
    //x.addClass('Sharad');  // Remember that the page has been shown before
});

//$(function () {
//    alert('ready');
//    return;
//    responsiveMobileMenu();
//    getMobileMenu();
//    adaptMenu();

//    /* slide down mobile menu on click */

//    $('.rmm-toggled, .rmm-toggled .rmm-button').click(function () {
//        if ($(this).is(".rmm-closed")) {
//            $(this).find('ul').stop().show(300);
//            $(this).removeClass("rmm-closed");
//        }
//        else {
//            $(this).find('ul').stop().hide(300);
//            $(this).addClass("rmm-closed");
//        }

//    });

//});

/* 	hide mobile menu on resize */
$(window).resize(function () {
    adaptMenu($('div.ui-page-active'));
});