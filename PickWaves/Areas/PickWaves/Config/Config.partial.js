$(document).ready(function () {
    $('#tabs').tabs({ active: $('#tabs').attr('data-active-tab') });

    // Autocomplete for customer.
    $('input[data-autocomplete-url]').autocomplete({
        minLength: 0,
        source: $('input[data-autocomplete-url]').attr('data-autocomplete-url'),
        autoFocus: true,
        change: function (ui, event) {
            var $form = $(this).closest('form');
            if (ui.val == null) {
                $(this).val('');
                if (!$form.valid()) {
                    return false;
                }
            }
            return true;
        }
    }).dblclick(function (e) {
        $(this).autocomplete('search');
    });

    $('#btnAddNewCustConstraint').click(function (e) {
        LoadAjaxDialog($(this).attr('data-add-customer'), $('#tabs').tabs('option','active'));
    }).button();

    $('tbody').on('click', 'span.ui-icon-pencil', function (e) {
        LoadAjaxDialog($(this).attr('data-editor-url'), $('#tabs').tabs('option','active'));
    });
    // Calls an action to load HTML and displays it in a dialog
    function LoadAjaxDialog(url, activeTabIndex) {
        $.ajax({
            url: url,
            type: 'get',
            cache: false
        }).done(function (data, textStatus, jqXHR) {
            $('#dlgConstraintsEditor').html(data).dialog({
                title: "Add/Edit Customer Constraint",
                modal: true,
                closeOnEscape: true,
                width: '500',
                open: function (e) {
                    $('#hfActiveTabIndex').val(activeTabIndex);
                },

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