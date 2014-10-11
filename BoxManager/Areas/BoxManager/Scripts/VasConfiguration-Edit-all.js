$(document).ready(function () {
    $('[data-icon]').each(function () {
        $(this).button({ icons: { primary: $(this).attr('data-icon') } });
    });
    $('#ddlPatternType').on('change', function (e) {
        if ($(this).val() == 0) {
            $('#txtAlphabets').hide();
            $('#ddlPatternSubType').val('').hide();
        } else {
            $('#txtAlphabets').show();
            $('#ddlPatternSubType').val('').show();
        }
    });
    $('#ddlPatternSubType').on('change', function () {
        if ($(this).val() == 0) {
            $('#txtAlphabets').show();
        } else {
            $('#txtAlphabets').hide();
        }
    });
    if ($('#ddlPatternSubType').val() != '') {
        $('#txtAlphabets').hide();
    }
    $('#btnVerifySetting').on('click', function () {
        if ($('#cbApplyPoPattern').is(':checked') && $('#ddlPatternSubType').val() == '' && $('#txtAlphabets').val() == '') {
            $('#spnMessage').html('Enter some text ').addClass('ui-state-error').show().delay(2000).fadeOut(2000);
            return false;
        }
        return true;
    });

    //---------------------------------- Code of Comma separated autocomplete for labels -----------------------------
    function split(val) {
        return val.split(/,\s*/);
    }
    function extractLast(term) {
        return split(term).pop();
    }
    $("#txtLabels").autocomplete({
        source: function (request, response) {
            $.getJSON($("#txtLabels").attr('data-autocomplete-url'), {
                term: extractLast(request.term)
            }, response);
        },
        search: function () {
            // custom minLength 1
            var term = extractLast(this.value);
            if (term.length < 1) {
                return false;
            }
        },
        focus: function () {
            // prevent value inserted on focus
            return false;
        },
        select: function (event, ui) {
            var terms = split(this.value);
            // remove the current input
            terms.pop();
            // if item already not added, then add the selected item
            var i = $.inArray(ui.item.value, terms);
            if (terms.length == 0 || i == -1) {
                terms.push(ui.item.value);
            }
            // add placeholder to get the comma-and-space at the end
            //terms.push("");
            this.value = terms.join(", ");
            return false;
        }
    });
    //----------------------------------------------------------------------------------------------------------------
});