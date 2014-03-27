///#source 1 1 /Areas/PickWaves/Scripts/_pickslipMatrixPartial.partial.js
/*
This widget display a matrix of pickslip counts for the dimension selected by the user.

When the user selects a different dimension, it auto refreshes itself via Ajax, raising the events refreshing and refreshed

When the user clicks on a populated matrix cell, it highlights it and raises the event selected.
*/
$.widget("custom.pickslipmatrix", {
    options: {
        // Event raised before HTML is refreshed. Return false to cancel the refresh
        refreshing: null,
        // Event raised after HTML has been updated due to dimension change by user
        refreshed: null,

        // Raised when the user clicks on a populated matrix cell
        // ui contains rowValue and colValue. Also td which is a DOM object representing the td which was clicked.
        selected: null
    },
    // the constructor
    _create: function () {
        this._on({
            'change th select': function (e) {
                if (this._trigger('refreshing') === false) {
                    return false;
                }
                this.element.addClass('ui-state-disabled')
                    .load($('div[data-url]:first', this.element).attr('data-url'), $('input,select', this.element).serialize(),
                    $.proxy(function (responseText, textStatus, xhr) {
                        if (textStatus == "error") {
                            alert("Sorry but there was an error: " + xhr.status + " " + xhr.statusText + ". Refresh the page.");
                        }
                        this.self.element.removeClass('ui-state-disabled');
                        this.self._trigger('refreshed');
                    }, { self: this }));
                return true;
            },
            'click td.ui-selectable': function (e) {
                this._selectTd(e.target, e);
            },
            'keypress td.ui-selectable': function (e) {
                // Simulate click when space pressed on a populated cell
                if (e.which == $.ui.keyCode.SPACE) {
                    this._selectTd(e.target, e);
                    //suppressing the space event to bubble out, just because of browser listen this event to scroll the page.
                    e.preventDefault();
                }
            }
        });
    },

    // Highlight the passed cell
    // td -> the DOM object representing the td to select. e-> Event which is causing the selection
    _selectTd: function (td, e) {
        $('td.ui-state-highlight', this.element).removeClass('ui-state-highlight');
        var $tr = $(td).closest('tr');
        this._trigger('selected', e, {
            rowValue: $('input:radio', $tr).prop('checked', true).val(),
            colValue: $('thead tr.dc-header input:radio', this.element).eq($('td', $tr).index(td) - 2).prop('checked', true).val(),
            td: td
        });
        $(td).addClass('ui-state-highlight').focus();
    },

    // called when created, and later when changing options
    _refresh: function () { },

    // events bound via _on are removed automatically
    // revert other modifications here
    _destroy: function () { },
    // _setOptions is called with a hash of all options that are changing
    // always refresh when changing options
    _setOptions: function () {
        // _super and _superApply handle keeping the right this-context
        this._superApply(arguments);
        this._refresh();
    },
    // _setOption is called for each individual option that is changing
    _setOption: function (key, value) {

        this._super(key, value);
    },
    // Returns the display name of the currently selected row dimension
    dimRowDisplayName: function () {

        return $('div[data-row-dimension]', this.element).attr('data-row-dimension');
    },
    // Returns the display name of the currently selected column dimension
    dimColDisplayName: function () {

    return $('div[data-col-dimension]', this.element).attr('data-col-dimension');
}
});

///#source 1 1 /Areas/PickWaves/Scripts/CreateWave-Index.partial.js
$(document).ready(function () {
    // Positions the dialog at the passed td and opens it
    // this represents the dialog DOM object
    function OpenDialog(td) {
        $(this).dialog('option', {
            position: {
                of: td,
                my: 'left top',
                at: 'right bottom'
            }
        }).dialog("open");
    }
    $('#matrix').pickslipmatrix();
    $('#matrixPartial').pickslipmatrix({
        refreshing: function (event, ui) {
            // Before the ajax call, close the popup just in case it is open
            $('#divDlg').dialog("close");
        },
        selected: function (event, ui) {
            // User selected a cell. Show popup and update its contents
            var $dlg = $('#divDlg');
            $('#dlgColDimSpan', $dlg).text($('#matrixPartial').pickslipmatrix("dimColDisplayName"));
            $('#dlgRowDimSpan', $dlg).text($('#matrixPartial').pickslipmatrix("dimRowDisplayName"));
            $('#dlgColDimSpanVal', $dlg).text(ui.colValue);
            $('#dlgRowDimSpanVal', $dlg).text(ui.rowValue);
            $('#dlgSpanPsCount', $dlg).text($(ui.td).text());

            var isOpen = $dlg.dialog('isOpen');

            if (isOpen) {
                $dlg.one('dialogclose', $.proxy(function (event, ui2) {
                    OpenDialog.apply($(event.target), [this.td]);
                }, { td: ui.td })).dialog('close');
            } else {
                OpenDialog.apply($dlg, [ui.td]);
            }

        }
    }).on('keypress', '.ui-state-highlight', function (e) {
        if (e.which == $.ui.keyCode.ENTER) {
            $('#frmMain').submit();
        }
    });

    $('#divDlg').dialog({
        show: "slide",
        hide: "slide",
        autoOpen: false,
        open: function (event, ui) {
            $('#dlgMessage').empty();
        },
        buttons: [
        {
            text: "Ok",
            click: function () {
                $('#frmMain').submit();
            }
        },
        {
            text: "Cancel",
            click: function () {
                $(this).dialog("close");
            }
        }
        ]
    });

    //For pull area
    $('#divPullArea').on('click', '.pull-area input:radio', function (e) {
        $(e.delegateTarget).find('li.ui-state-active').removeClass('ui-state-active');
        $(this).closest('li').addClass('ui-state-active');
        $('#cbAllowPulling').prop('checked', true);
    }).on('click', '#seeAllPullAreas', function (e) {
        $(e.delegateTarget).find('li.ui-helper-hidden').show();
        $(this).hide();
    });

    //For pitch area
    $('#divPitchArea').on('click', '.pitch-area input:radio', function (e) {
        $(e.delegateTarget).find('li.ui-state-active').removeClass('ui-state-active');
        $(this).closest('li').addClass('ui-state-active');
        $('#cbAllowPitching').prop('checked', true);
    }).on('click', '#seeAllPitchAreas', function (e) {
        $(e.delegateTarget).find('li.ui-helper-hidden').show();
        $(this).hide();
    });
});