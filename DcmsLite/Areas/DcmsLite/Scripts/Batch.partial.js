$(document).ready(function () {
    $('#tabs').tabs();
    $('button').button();
    $('#btnPrintNow').button({ icons: { primary: 'ui-icon-print' } });
    $('#CurrentBatch table tbody tr a.ui-icon-print').on("click", function (e) {
        var printer = $('#frmPrintBatch input[type=radio]:checked');
        var uccId = $(e.target).attr('data-uccid');
        if ($(printer).length == 0) {
            $('#divInfo')
                      .removeClass('ui-state-highlight').addClass('ui-state-error')
                      .find('span.info-message').html('Please select a printer from list.')
                      .end()
                      .find('span:eq(1)').removeClass('ui-icon-info').addClass('ui-icon-alert')
                      .end()
                      .show();
            return false;
        }
        $('#hfUccId').val(uccId);
        $('#hfSelectedPrinter').val($(printer).val());
        if (confirm('Are you sure, you want to print the label for Box ' + uccId)) {
            $.ajax({
                url: $('#frmPrintDialog').attr('action'),
                data: $('#frmPrintDialog').serializeArray(),
                type: 'POST',
                cache: false
            }).done(function (data, textStatus, jqXHR) {
                $(e.target).closest('tr').find('td.ucc-print-date').text('Printed Now').addClass('ui-state-highlight');
                $('#divInfo')
                       .removeClass('ui-state-error').addClass('ui-state-highlight')
                       .find('span.info-message').html(data)
                       .end()
                       .find('span:eq(1)').removeClass('ui-icon-alert').addClass('ui-icon-info')
                       .end()
                       .show();
            }).fail(function (jqXHR, textStatus, errorThrown) {
                $('#divInfo')
                       .removeClass('ui-state-highlight').addClass('ui-state-error')
                       .find('span.info-message').html(jqXHR.responseText)
                       .end()
                       .find('span:eq(1)').removeClass('ui-icon-info').addClass('ui-icon-alert')
                       .end()
                       .show();
            });
        }
        return false;
    });
});