/// <reference1 path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

$(document).ready(function () {
    //$('#btnLocation,#btnAssignedSku').button({ icons: { primary: 'ui-icon-circle-check'} });
    $('button.mca-unassign').each(function () {
        $(this).button({ text: false, disabled: $(this).is('[disabled]'), icons: { primary: 'ui-icon-close' } });
    });
    $('button.mca-assign').each(function () {
        $(this).button({ text: false, icons: { primary: 'ui-icon-pencil' } });
    });

    // The dialog must be passed $tr (current row which has been clicked) in the custom option currentRow
    $('#divEditDialog').dialog({
        autoOpen: false,
        width: 'auto',
        modal: true,
        closeOnEscape: true,
        // Clear existing values
        open: function (event, ui) {            
            var $tr = $(this).dialog('option', 'currentRow');
            var locationId = $tr.find('span.mca-locationid').html();
            var sku = $tr.find('span.mca-sku').text().replace(/\s+/g, '');
            if (sku == "") {
                sku = "NONE";
            }
            var cartonSku = $tr.find('span.mca-ctnSku').text();
            if (cartonSku == "") {
                cartonSku = "NONE";
            }
            var vwh = $tr.find('span.mca-vwh').text().replace(/\s+/g, '');
            if (vwh == "") {
                vwh = "NONE";
            }
            var cartonCount = $tr.find('span.mca-cartoncount').html();
            $(this).dialog({ title: 'Update Location #' + locationId });
            $(this).find('#tbMaxAssignedCarton').removeClass('input-validation-error');
            $("#displayCartonCount").html("<b>Location contains " + cartonCount + " cartons of SKU " + cartonSku + ". </b>").addClass('ui-state-highlight');
            $('#displayWarning', this).html("Assigned SKU: (" + sku + ") | VWh: (" + vwh)
                .addClass('ui-state-highlight');
            if (sku != "") {
                var upccode = $tr.find('span.mca-sku span').attr('title');
                $('#tbSku').val(upccode);
                $('span.spnDisplaySku').html($tr.find('span.mca-sku').text().replace(/\s+/g, ''));
            }
            $('#tbAssignedVwh').val(vwh).attr('selected', true);
            $('#tbMaxAssignedCarton').val($tr.find('span.mca-maxassignedcartons').text().replace(/\s+/g, ''));
            $('#hfCurrentLocationId', this).val(locationId);
            $('#ajaxErrors', this).empty();
            $('div[data-valmsg-summary]', this).removeClass('validation-summary-errors').addClass('validation-summary-valid');
            $("#btnUpdate").button({ icons: { primary: "ui-icon-disk" } });
        },
        buttons: [
            {
                id: 'btnUpdate',
                text: 'Update',
                click: function (event, ui) {
                    var $form = $('form', this);
                    if (!$form.valid()) {
                        $("#frmEditLocation input[data-ac-list-url]").autocompleteEx('clear');
                        $('input:text', this).val('');
                        return false;
                    }
                    if (!$('#tbSku').val()) {
                        $("#frmEditLocation input[data-ac-list-url]").autocompleteEx('clear');
                    }
                    var dialogData = $form.serializeArray();
                    $.ajax({
                        url: $form.attr('action'),
                        type: 'POST',
                        data: dialogData,
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
                text: 'Close',
                click: function (event, ui) {
                    $(this).dialog('close');
                }
            }
        ]
    });
    // Table row gets ui-state-highlight when the dialog is opened. It gets ui-state-active after a successful assignment/unassignment is performed.
    // Only one row can have this state
    $('#divLocationList').click(function (e) {
        $('table tr', this).removeClass('ui-state-highlight').removeClass('ui-state-active');
        var $tr = $(e.target).closest('tr');
        if ($(e.target).is('button.mca-assign')) {
            $('#divEditDialog')
            .dialog('option', 'currentRow', $tr)
            .dialog('open');
        }

        // Unassign SKU
        var locationId = $tr.find('span.mca-locationid').html();
        // areaId is use for update areaInfo.
        var areaId = $('#hfAreaId').val();
        if ($(e.target).is('button.mca-unassign:not([disabled])') && confirm("Assigned SKU from location " + locationId + " will be removed. Are you sure?")) {
            $.ajax({
                url: $('#divEditDialog').attr('data-unassign-url'),
                type: 'POST',
                data: { locationId: locationId, areaId: areaId },
                statusCode: {
                    // Success
                    200: function (data, textStatus, jqXHR) {
                        // update areaInfo table.
                        $('#divupdatefilter').html(data);
                        //Clear the fields which like SKU and capacity in grid row
                        $tr.find('span.mca-sku,span.mca-maxassignedcartons,span.mca-vwh')
                            .empty()
                            .end()
                             .find('div.ui-progressbar-value')
                            .css('width', '0%').removeClass('ui-state-error')
                             .end()
                            .removeClass('ui-state-highlight')
                            .addClass('ui-state-active')
                            .find('button.mca-unassign')
                            .button('disable');
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert(jqXHR.responseText);
                }
            });
        }
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