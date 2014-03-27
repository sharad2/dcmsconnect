$(document).ready(function () {
    $('#btnCreateBucket').button();
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
});
