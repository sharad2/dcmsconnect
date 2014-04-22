$(document).ready(function () {
    $('#btnCreateBucket').button();

    $('#matrixPartial').on('keypress', 'td.ui-selectable', function (e) {
        if (e.which == $.ui.keyCode.ENTER) {
            // Simulate a click on the cell
            $(this).click();
        }
    }).on('change', 'th select', function (e) {
        // Close the dialog if it is open
        $('#divDlg').dialog('close');

        // Ajax load the new matrix when a dimension changes
        $(e.delegateTarget).addClass('ui-state-disabled')
            .load(
                $(e.delegateTarget).attr('data-url'), $('input,select', $(e.delegateTarget)).serialize(),
                $.proxy(function (responseText, textStatus, xhr) {
                    if (textStatus == "error") {
                        alert("Sorry but there was an error: " + xhr.status + " " + xhr.statusText + ". Refresh the page.");
                    }
                    this.self.removeClass('ui-state-disabled');
                }, { self: $(e.delegateTarget) })
            );
        var dimText = $('option:selected', this).text();
      
        if ($(this).closest('th').attr('rowspan')) {
            // Represents ddl of row dimension
            $('#dlgRowDimSpan').text(dimText);
        } else {
            // represents ddl of col dimension
            $('#dlgColDimSpan').text(dimText);
        }
        return true;
    }).on('click', 'td.ui-selectable', function (e) {
        // When a selectable cell is clicked, Select the row and col radio buttons and open dialog
        var $tr = $(this).closest('tr');
        $('input:radio', $tr).prop('checked', true);

        $('thead tr.dc-header input:radio', e.delegateTarget).eq($('td', $tr).index(this) - 2).prop('checked', true);
        $('#divDlg').dialog('open', $(this));
    }).on('click', 'input:radio', function (e) {
        // Just show the dialog
        $('#divDlg').dialog('open', $(this).closest('td,th'));
    });

    var _dlgOpen;
    // Good tutorial on extending widgets
    // http://learn.jquery.com/jquery-ui/widget-factory/extending-widgets/
    // This dialog displays text associated with selected radio buttons
    // This is a singleton dialog. Automatically closes other dialogs when a new instance is opened
    $.widget("ui.dialog", $.ui.dialog, {
        options: {
            show: "slide",
            hide: "slide",
            autoOpen: false,
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
        },
        // Pass the td which will be used to position the dialog
        open: function ($td) {
            if (_dlgOpen) {
                // Something is already open. Close it and then call this same function again after closing is complete
                // http://stackoverflow.com/questions/6923647/how-to-attach-callback-to-jquery-effect-on-dialog-show
                var self = this;
                _dlgOpen.close();
                _dlgOpen.widget().promise().done(function () {
                    _dlgOpen = null;
                    self.options.position = {
                        of: $td,
                        my: 'left top',
                        at: 'right bottom'
                    };
                    self.open($td);
                });
                return;
            }
            this.option({
                position: {
                    of: $td,
                    my: 'left top',
                    at: 'right bottom'
                }
            });

            _dlgOpen = this;
            $('#dlgMessage').empty();

            // Find the checked radio buttons and their row and column indexes
            var $elems = $('#matrixPartial thead input:radio');
            var $rb = $elems.filter('input:radio:checked');
            $('#dlgColDimSpanVal').text($rb.val());
            var j = $elems.index($rb);

            var $tr = $('#matrixPartial tbody tr').has('input:radio:checked'); // Row containing selected radio button
            $rb = $('input:radio', $tr);  // The only radio button in this row
            $('#dlgRowDimSpanVal').text($rb.val());

            // j'th td is the selected cell of $tr. Show its text as pickslip count
            var $td = $('td', $tr).eq(j + 2);  // td which contains the pickslip count

            // Open the dialog only if the td is selctable
            if ($td.is('.ui-selectable')) {
                $('#dlgSpanPsCount').text($td.text());
                this._super();
            }
        }
    });

    // Navigate to the page which will list pickslips of the selected cell
    $('#divDlg').dialog().on('click', '#btnViewPickslips', function (e) {
        $('#hfViewPickslips').val('Y');
        $('#frmMain').submit();
    });
});
