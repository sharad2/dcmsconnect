/// <reference1 path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

$(document).ready(function () {
    $('button.mca-unassign').button({ text: false, icons: { primary: 'ui-icon-close' } });
    $('button.mca-assign').button({ text: false, icons: { primary: 'ui-icon-pencil' } });


    // The dialog must be passed $tr (current row which has been clicked) in the custom option currentRow
    $('#divEditDialog').dialog({
        autoOpen: false,
        width: 'auto',
        modal: true,
        closeOnEscape: true,
        // Clear existing values
        open: function (event, ui) {
            var $tr = $(this).dialog('option', 'currentRow');
            var vwh = $('span.mca-vwh', $tr).text().trim();

            $('#lblSku em, span.sku-display', this).text($('span.mca-sku', $tr).text().trim());
            $('#lblAssignedVwh em', this).text(vwh);

            var capacity = $('span.mca-maxassignedcartons', $tr).text().trim();
            $('#lblMaxAssignedCarton em', this).text(capacity);
            $('#tbMaxAssignedCarton').val(capacity);

            $(this).dialog({ title: 'Assign SKU to Location #' + $tr.attr('data-location-id') });
            $(this).find('#tbMaxAssignedCarton').removeClass('input-validation-error');
            $("#displayCartonCount span", this).text($('span.mca-cartoncount', $tr).text());
            $('#tbSku').val($tr.attr('data-upc-code'));
            $('#tbAssignedVwh').val(vwh);

            $('#hfCurrentLocationId', this).val($tr.attr('data-location-id'));
            $('#ajaxErrors', this).empty();
            $('div[data-valmsg-summary]', this).removeClass('validation-summary-errors').addClass('validation-summary-valid');
        },
        buttons: [
            {
                id: 'btnUpdate',
                text: 'Update',
                icons: { primary: "ui-icon-disk" },
                click: function (event, ui) {
                    var $form = $('form', this);
                    if (!$form.valid()) {
                        return false;
                    }

                    $.ajax($form.attr('action'), {
                        type: 'POST',
                        data: $form.serializeArray(),
                        context: this,
                        statusCode: {
                            // Success
                            200: function (data, textStatus, jqXHR) {
                                // Update areaInfo table.
                                $('#divupdatefilter').html(data);
                                // update location list.
                                var $row = $(this).dialog('option', 'currentRow');
                                var cartonCount = parseInt($('span.mca-cartoncount', $row).html());
                                var value = parseInt($('#tbMaxAssignedCarton', this).val());
                                var maxAssignedCartons = isNaN(value) ? null : value;
                                var pct = Math.min((cartonCount * 100) / maxAssignedCartons, 100);
                                $row.removeClass('ui-state-highlight')
                                    .addClass('ui-state-active')
                                    .find('span.mca-sku')
                                    .html('<span title=' + $('#tbSku').val() + '>' + $('span.spnDisplaySku', this).html() + '</span>')
                                    .end()
                                    .find('span.mca-vwh')
                                    .html($('#tbAssignedVwh', this).val())
                                    .end()
                                    .find('span.mca-maxassignedcartons')
                                    .html(maxAssignedCartons)
                                    .end()
                                    .find('div.ui-progressbar-value')
                                    .width(pct + '%')
                                    .toggleClass('ui-state-error', (maxAssignedCartons == null ? 0 : maxAssignedCartons) < cartonCount)
                                    .end()
                                    .find('button.mca-unassign')
                                    .button('enable');
                                $(this).dialog('close');
                                $('input:text', this).val('');
                                $("#frmEditLocation input[data-ac-list-url]").autocompleteEx('clear');
                            },
                            // Error
                            500: function (jqXHR, textStatus, errorThrown) {
                                $('div[data-valmsg-summary]', this).html(jqXHR.responseText)
                                    .removeClass('validation-summary-valid')
                                    .addClass('validation-summary-errors');
                                $('span.spnDisplaySku,#ajaxErrors', this).empty();
                                $('input:text', this).val('');
                                $("#frmEditLocation input[data-ac-list-url]").autocompleteEx('clear');
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            alert(jqXHR.responseText);
                        }
                    });
                }
            },
            {
                text: 'Cancel',
                click: function (event, ui) {
                    var $tr = $(this).dialog('option', 'currentRow').removeClass('ui-state-highlight');
                    $(this).dialog('close');
                }
            }
        ]
    });

    $('#divLocationList').on('click', 'button.mca-unassign', function (e) {
        // Unassign SKU
        var $tr = $(this).closest('tr').addClass('ui-state-highlight');
        var locationId = $tr.attr('data-location-id');
        if (!confirm("Assigned SKU from location " + locationId + " will be removed. Are you sure?")) {
            $tr.removeClass('ui-state-highlight');
            return false;
        }

        $.ajax($(this).attr('data-unassign-url'), {
            type: 'POST',
        }).done($.proxy(function (data, textStatus, jqXHR) {            
            $('#divupdatefilter').html(data);
            this.row.addClass('ui-state-active')
                .find('span.mca-sku,span.mca-maxassignedcartons,span.mca-vwh')
                .empty();
            this.button.button('disable');
        }, { row: $tr, button: $(this) })).fail(function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR.responseText);
        }).always($.proxy(function () {
            $tr.removeClass('ui-state-highlight');
        }, { row: $tr }));
    }).on('click', 'button.mca-assign', function (e) {
        var $tr = $(this).closest('tr').addClass('ui-state-highlight');
        $('#divEditDialog')
            .dialog('option', 'currentRow', $tr)
            .dialog('open');
    });
});

/*
$Id$ 
$Revision$
$URL$
$Header$
$Author$
$Date$
*/