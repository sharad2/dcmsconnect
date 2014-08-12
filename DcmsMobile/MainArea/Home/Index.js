/*
Control group is always horizontal within a header

HTML
----
<div id="responsivegroup">
    <h3>@Model.Categories[i].Name</h3>
    <div data-role="controlgroup" data-type="horizontal" style="text-align:center">
        @for (var j = 0; j < Model.Categories.Count; ++j)
        {
            <a class="ui-btn ui-btn-active">@(j + 1). @Html.DisplayFor(m => m.Categories[j].Description)</a>
        }
    </div>
</div>

Javascript
----------
$(document).on('pagecontainertransition', function (event, ui) {
    $('#responsivegroup', ui.toPage).responsivegroup();
});
*/
(function ($, undefined) {
    $.widget("dcmsmobile.responsivegroup", {
        options: {
            heading: "h1,h2,h3,h4,h5,h6,legend"
        },
        _create: function () {
            this._on(this.window, {
                resize: '_delayRefresh'
            });
            $(this.options.heading, this.element).eq(0).hide();
            this.refresh();
        },

        _timer: null,
        _delayRefresh: function () {
            if (this._timer) {
                // Do nothing
                return;
            }
            if (this.element.is('.ui-collapsible')) {
                this._destroyCollapsible();
            }
            this._timer = setTimeout(function (self) {
                //adaptMenu2();
                self.refresh();
            }, 500, this);
        },
        _createCollapsible: function () {
            $(this.options.heading, this.element).eq(0).show();
            this.element.collapsible({
                heading: this.options.heading
            });
            $('.ui-controlgroup', this.element).controlgroup('option', 'type', 'vertical');
        },
        _destroyCollapsible: function () {
            this.element.collapsible('destroy');
            $(this.options.heading, this.element).eq(0).hide();
            $('.ui-controlgroup', this.element).controlgroup('option', 'type', 'horizontal');
        },
        refresh: function () {
            if (this._timer) {
                // Clear the timer so that we do not refresh again. We should get here only when being called by _delayRefresh
                clearTimeout(this._timer);
                this._timer = null;
            }
            var x;
            var wrapped = false;
            $(".ui-controlgroup-controls >", this.element).each(function () {
                var top = Math.round($(this).position().top);
                if (x == null) {
                    x = top
                } else if (x != top) {
                    wrapped = true;
                }
            });
            if (this.element.is('.ui-collapsible')) {
                if (!wrapped) {
                    this._destroyCollapsible();
                }
            } else {
                if (wrapped) {
                    this._createCollapsible();
                }
            }
        }
    });
})(jQuery);

/*
These global variables must be set before this script is loaded

_rcBaseUrl: URL of the RC website home page. This should be an absolute URL without the trailing /
_rcItemsUrl: URL which will return the json array of RC items. This should be a relative to _rcBaseUrl and must begin with /
*/

// Get a list of items available on the release candidate site and show the RC link for each of the items available
$(document).one('pagecreate', function () {
    if (!_rcBaseUrl) {
        // No RC URL specified. Nothing to do
        return;
    }
   
    $('a.rclink span').text('Contacting...');
    $.ajax(_rcBaseUrl + _rcItemsUrl, {
        dataType: 'jsonp',
        crossDomain: true
    }).done(function (data, textStatus, jqXHR) {
        // The call to RC was successful. Show the RC link at the bottom of the page
        // data is an array of {route: "DCMSConnect_App1", url: "/Inquiry/Home/Index"}
        // Show the rc link against each menu items
        $('a.rclink').show().find('span').text('(' + data.length + ')');
        $.each(data, function (e, ui) {
            $('div.' + ui.route).show()
                .find('a').attr('href', _rcBaseUrl + this.url);
        });        
        //$('div.ui-page-active div[data-role="controlgroup"]').controlgroup("refresh");
    }).fail(function (jqXHR, textStatus, errorThrown) {
        // TODO: Show error on RC button
        $('a.rclink span').text('Contact failed with error ' + jqXHR.status)
            .addClass('validation-summary-errors');
    });     
});
//$(document).ready(function () {
//    $('.responsivegroup').responsivegroup();
//});
$(document).on('pagecontainertransition', function (event, ui) {
    $('.responsivegroup', ui.toPage).responsivegroup();
});





