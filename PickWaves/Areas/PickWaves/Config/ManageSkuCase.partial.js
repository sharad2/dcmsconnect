$(document).ready(function () {
    $('#tabs').tabs({ active: $('#tabs').attr('data-active-tab') });
    $('#btnAddSkuCase').button({ icons: { primary: 'ui-icon-plus' } })
        .click(function (e) {
            LoadAjaxDialog($(this).attr('data-add-action'), 'New SKU Case');
        });
    $('#btnAddCustSkuCasePreference').button({ icons: { primary: 'ui-icon-plus' } });

    $('#btnAddPackinRule').button({ icons: { primary: 'ui-icon-plus' } })
    .click(function (e) {
        LoadAjaxDialog($(this).attr('data-add-packing-rule'), 'Add Packing Rule');
    });

    $('#tab1').on('click', 'span.ui-icon-pencil', function (e) {
        LoadAjaxDialog($(this).attr('data-edit-action'), 'Edit SKU Case');
    });

    $('span.ui-icon-close.delete-customer-sku').click(function (e) {
        $('table tr').removeClass('ui-state-highlight');
        var $tr = $(e.target).closest('tr');
        $tr.addClass('ui-state-highlight');
        if (confirm('Are you sure you want to delete SKU case ' + $tr.find('td.data-customer-prefered-skucase').text() + ' from customer ' + $tr.attr('data-customerId') + '?')) {
            $('#hfPreferedCaseId').val($tr.find('td.data-customer-prefered-skucase').text());
            $('#hfCustomerId').val($tr.attr('data-customerId'));
            $('#frmDelCustCasePreference').submit();
        }
        else {
            return false;
        }
    });

    $('#btnAddCustSkuCasePreference').click(function (e) {
        LoadAjaxDialog($(this).attr('data-add-customer-preference'), 'Add customer SKU case preferences');
    });

    $('span.ui-icon-close.delete-packing-rule').click(function (e) {
        var $tr = $(e.target).closest('tr');
        if (confirm('Are you sure you want to remove ignorance of case ' + $tr.find('td.data-ignored-case').text().replace(/\s+/g, '') + ' from style ' + $tr.attr('data-style'))) {
            $('#hfStyle').val($tr.attr('data-style'));
            $('#hfIgnoredCase').val($tr.find('td.data-ignored-case').text().replace(/\s+/g, ''));
            $('#frmDelPackingRule').submit();
        }

    });

    $('span.ui-icon-plus.add-cust-sku-case').click(function(e) {
        LoadAjaxDialog($(this).attr('data-add-selected-cust-case'), 'Add customer SKU case preferences');
    });

    $('span.ui-icon-pencil.edit-packing-rule').click(function (e) {
        LoadAjaxDialog($(this).attr('data-edit-packing-rule'), 'Add Packing Rule');
    });

    // Calls an action to load HTML and displays it in a dialog
    function LoadAjaxDialog(url, title) {
        $.ajax({
            url: url,
            type: 'get',
            cache: false
        }).done(function (data, textStatus, jqXHR) {
            $('#dlg').html(data).dialog({
                title: title,
                modal: true,
                closeOnEscape: true,
                width: 'auto',
                autoOpen: true,
                buttons: [
            {
                text: 'Save',
                click: function (event, ui) {
                    var $form = $('form', this);
                    if (!$form.valid()) {
                        return false;
                    }
                    $form.submit();
                }
            },
            {
                text: 'Cancel',
                click: function (event, ui) {
                    $(this).dialog('destroy').empty();
                }
            }
                ]
            });
        }).fail(function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR.responseText);
        });
    }

});
