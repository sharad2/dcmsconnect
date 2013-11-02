// scripts used in create request view.
$(document).ready(function () {
    // Show/Hide additional filters
    $('#pullWhatOptions').on('click', 'a:first', function (e) {
        // More
        $('#fsFilters').show(500);
        $(this).hide().next().show();
    }).on('click', 'a:eq(1)', function (e) {
        // Less
        $('#fsFilters').hide(500);
        $(this).hide().prev().show();
    });

    $('#reworkOptions').on('click', 'a:first', function (e) {
        // More
        $('#fsRework').show(500);
        $(this).hide().next().show();
    }).on('click', 'a:eq(1)', function (e) {
        // Less
        $('#fsRework').hide(500);
        var visible = $('span', this).toggle().is(':visible');
        // Show rework message when rework options are hidden and this is a rework request
        $('#divReworkMessage').toggle(!visible && $('#cbIsConversionRequest').is(':checked'));
        $(this).hide().prev().show();
    });


    $('#btnCreateRequest').button({ icons: { primary: 'ui-icon-plus' } });
    $('#btnSaveRequest').button({ icons: { primary: 'ui-icon-disk' } });

    $('#btnGo').button({ icons: { secondary: 'ui-icon-search' } }).click(function (e) {
        if ($('#frmSearch input:text').val() == '') {
            $('#frmSearch div').html("Please enter the valid Request ID")
                .removeClass('validation-summary-valid')
                .addClass('validation-summary-errors');
            return false;
        }
    });


    $("#tbCartonDate").datepicker({
        showOn: "button",
        buttonImage: $("#tbCartonDate").attr('data-img-url'),
        buttonImageOnly: true,
        onSelect: function () { this.focus(); }
    });

    //Cascading drop down list for selected building. Populate areas.
    $('#ddlBuilding').change(function () {
        if ($(this).val()) {
            var sourceUrl = $(this).attr('data-source-url').replace("X", $(this).val());
            $.getJSON(sourceUrl).done(function (data) {
                InsertAreas($('#ddlSourceArea'), data, true, false);
                InsertAreas($('#ddlDestArea'), data, false, $('#cbIsConversionRequest').is(':checked'));
            }).fail(function (jqXHR) {
                alert(jqXHR.responseText);
            });
        } else {
            InsertAreas($('#ddlSourceArea'), null, true, false);
            InsertAreas($('#ddlDestArea'), null, false, $('#cbIsConversionRequest').is(':checked'));
        }
    });

    function InsertAreas($ddl, data, numbered, rework) {
        $('option[value!=""]', $ddl).remove();
        if (data) {
            // If nothing returned, the list stays empty
            $ddl.append($.map(data, function (area) {
                if (area.Numbered == numbered && area.ReworkArea == rework) {
                    if (!numbered || !data.Count) {
                        // We do not want to display numbered areas which have no cartons
                        var $option = $('<option></option>').text(area.Text).attr({
                            value: area.Value
                        });
                        return $option;
                    }
                }
            })).prop('disabled', false);
            var $options = $('option', $ddl);

            switch ($options.length) {
                case 1:
                    // No reasonable area
                    $ddl.append($('<option></option>').remove().text('No reasonable areas found').prop('selected', true).css('color', 'red')).prop('disabled', true);
                    break;

                case 2:
                    // Only one possible choice
                    $ddl.val($options[1].value);
                    break;

                default:
                    // Multiple choices
                    // Set the preferred value as the default value. Useful when editing requests.
                    var val = $ddl.attr('data-selected-areaid');
                    if (val && $('option[value=' + val + ']', $ddl).length > 0) {
                        // Use the preferred value only if it is one of the drop down choices
                        $ddl.val(val);
                    }
                    break;
            }
        }
    }


    //Call change event of building if cbIsConversionRequest is clicked.
    $('#cbIsConversionRequest').on('change', function (e) {
        $('#ddlBuilding').change();
        $('#divReworkMessage').toggle();
    });
    // If we are editing a request, force the area drop downs to get populated
    if ($('select[data-selected-areaid!=""]').length > 0) {
        $('#ddlBuilding').change();
    };

});
