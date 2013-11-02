/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

//Ajax call to add an SKU to the Request and updating the list
$(document).ready(function () {

    // Initial focus
    $('#tbNewStyle').focus();

    // Display helpful tip for adding SKUs
    $('tr').tooltip({
        position: {
            my: "left bottom",
            at: "left top",
            using: function (position, feedback) {
                $(this).css(position);
                $("<div>")
                .addClass("arrow")
                .addClass(feedback.vertical)
                .addClass(feedback.horizontal)
                .appendTo(this);
            }
        },
        content: function () {
            return $('#addSkuTip').html();
        },
        items: '#trAddSku'
    }).tooltip('open');

    //Rk: Supports enter button to validate form.
    var _enterPressedOnId;
    $('#divSkuList').on('keypress', 'input', function (e) {
        // If enter pressed on an input field, click the add button
        if (e.which == $.ui.keyCode.ENTER) {
            $('#btnAddSku').click();
            _enterPressedOnId = e.target.id;
            e.preventDefault();
        }
    }).on('focus', 'input:text', function (e) {
        // Select input text when focus is received
        this.select();
    }).on('click', 'tbody tr span.ui-icon-close', function (e) {
        // Remove this sku from the request
        var $tr = $(this).closest('tr');
        var sku = [$tr.attr('data-style-editable'), $tr.attr('data-color-editable'), $tr.attr('data-dimension-editable'), $tr.attr('data-skusize-editable')].join(',');
        var pulled = $(this).attr('data-pulled-cartons');
        if (pulled > 0 && !confirm(pulled + " cartons containing SKU " + sku + " have been already pulled. They will no longer be associated with this request. Remove this SKU anyway?")) {
            return;
        }
        $.ajax({
            url: $(this).attr('data-href'),
            type: 'post',
            cache: false
        }).done(function (data, textStatus, jqXhr) {
            $('#divSkuList').html(data);
            // Reparse validation attributes of the returned form
            $.validator.unobtrusive.parse('#divSkuList');
        }).fail(function (jqXhr, textStatus, errorThrown) {
            alert(jqXhr.responseText);
        });
    }).on('click', 'tbody tr:not(.ui-state-highlight)', function (e) {
        // Select the clicked row
        $('tr.ui-state-highlight', e.delegateTarget)
            .removeClass('ui-state-highlight');
        $(this).addClass('ui-state-highlight');
        var $tr = $(this);
        $('#trAddSku input[data-source-attr]').val(function (index, value) {
            return $tr.attr($(this).attr('data-source-attr'));
        }).filter('#tbPieces').focus();
    }).on('click', '#btnAddSku:not(.ui-state-disabled)', function (e) {
        // The button will be disabled while AJAX call is in progress. Ignore clicks on disabled button.
        //Forcefully stopping the page to be posted if its invalid
        var $form = $(this).closest('form');
        if (!$form.valid()) {
            return false;
        }
        $(this).addClass('ui-state-disabled');
        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: $form.serialize(), //send serialized form data in Ajax call.
            context: e
        }).done(function (data, textStatus, jqXhr) {
            $(this.delegateTarget).html(data);
            // Set the focus back to the input on which enter was pressed
            if (_enterPressedOnId && _enterPressedOnId != 'tbPieces') {
                $('#' + _enterPressedOnId).focus();
            } else {
                $('#tbNewStyle').focus();
            }
            _enterPressedOnId = null;
        }).fail(function (jqXhr, textStatus, errorThrown) {
            alert(jqXhr.responseText);
        });
        return false;
    });
});

//Assigning cartons to the Request
$(document).ready(function () {
    $('#btnAssign').button({ icons: { primary: 'ui-icon-plus' } }).click(function (e) {
        return confirm("Are you sure, you want to assign cartons to request?");
    });

    //UnAssign cartons from the Request
    $('#btnUnAssign').button({ icons: { primary: 'ui-icon-minus' } })
        .click(function (e) {
            return confirm("Are you sure, you want to unassign cartons from request?");
        });
});

//$Id$