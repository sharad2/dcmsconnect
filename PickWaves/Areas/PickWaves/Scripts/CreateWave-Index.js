$(document).ready(function () {
    $('#btnCreateBucket').button();

    $('#matrixPartial').on('keypress', '.ui-state-highlight', function (e) {
        if (e.which == $.ui.keyCode.ENTER) {
            $('#frmMain').submit();
        }
    }).on('change', 'th select', function (e) {
        $(e.delegateTarget).addClass('ui-state-disabled')
            .load($(e.delegateTarget).attr('data-url'), $('input,select', $(e.delegateTarget)).serialize(),
            $.proxy(function (responseText, textStatus, xhr) {
                if (textStatus == "error") {
                    alert("Sorry but there was an error: " + xhr.status + " " + xhr.statusText + ". Refresh the page.");
                }
                this.self.removeClass('ui-state-disabled');
            }, { self: $(e.delegateTarget) }));
        return true;
    }).on('click', 'td.ui-selectable', function (e) {

        $('#dlgSpanPsCount').text($(this).text());

        var $tr = $(this).closest('tr');
        // Display the values of the selected row and column dimensions
        var $rb = $('input:radio', $tr)
            .prop('checked', true);

        $('#dlgRowDimSpanVal').text($rb.val());

        $rb = $('thead tr.dc-header input:radio', e.delegateTarget).eq($('td', $tr).index(this) - 2).prop('checked', true);
        $('#dlgColDimSpanVal').text($rb.val());


        // Open the dialog which asks for confirmation to add pickslips
        var $dlg = $('#divDlg');
        var td = this;

        // Open the dialog after close has completed
        // http://stackoverflow.com/questions/6923647/how-to-attach-callback-to-jquery-effect-on-dialog-show
        $dlg.dialog('close').parent().promise().done(function () {
            $dlg.dialog('option', {
                position: {
                    of: $(td),
                    my: 'left top',
                    at: 'right bottom'
                }
            }).dialog('open');
        });

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
                $('#hfViewPickslips').val('');
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
    }).on('click', '#btnViewPickslips', function (e) {
        $('#hfViewPickslips').val('Y');
        $('#frmMain').submit();
    });
});

