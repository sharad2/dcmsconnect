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
                var top = $(this).position().top;
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
