﻿/*global Bloodhound:false */
/* jshint -W098 */

// If associated check box is checked, then the associated hidden fieled must have a value
// We look for cb within closest input group, and the hidden is any non typeahead textbox
$.validator.addMethod("requiredif", function (value, element) {
    //alert(false);
    var $ig = $(element).closest('div.input-group');
    return !$('input:checkbox', $ig).is(':checked') || $('input:text:not(".typeahead")', $ig).val();
    //return false;
}, 'Please enter a valid choice');

$(document).ready(function () {
    "use strict";
    // Add this rule to every typeahead input within the form
    $('form.spotcheckEditor input.typeahead').rules('add', {
        requiredif: true
    });

    // The hidden field must be immediately after the input group
    //var adapters = {};
    $('form.spotcheckEditor div.input-group[data-url]').each(function (i, elem) {
        var adapter = new Bloodhound({
            datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            remote: {
                wildcard: '~',
                url: $(elem).attr('data-url'),
                ajax: { cache: false }
            }
        });
        adapter.initialize();
        $(elem).data('ttadapter', adapter.ttAdapter());

    }).on('change', 'input:checkbox:checked', function (e) {
        // Style checkbox has been checked by the user. He wants to enter a specific style
        $('input.typeahead', e.delegateTarget).prop('disabled', false)
            .typeahead(null, {
                name: 'Styles',
                displayKey: 'label',
                source: $(e.delegateTarget).data('ttadapter'),
                templates: {
                    empty: 'No matching choices'
                }
            }).on('typeahead:selected.mytypeahead typeahead:autocompleted.mytypeahead', { "$tbHidden": $('input:text:not(".typeahead")', e.delegateTarget) }, function (e, sug, ds) {
                // Store the id of the selected style in the hdden field
                //alert('Hi');
                e.data.$tbHidden.val(sug.value);
            }).on('input.mytypeahead', { "$tbHidden": $('input:text:not(".typeahead")', e.delegateTarget) }, function (e) {
                // When user changes the style, empty the hidden field
                e.data.$tbHidden.val('');
            });
    }).on('change', 'input:checkbox:not(":checked")', function (e) {
        // Style checkbox has been unchecked by the user. He wants to use all styles
        // Destroy the typeahead
        $('input:text:not(".typeahead")', e.delegateTarget).val('');
        $('input.typeahead', e.delegateTarget).prop('disabled', true)
            .val('')
            .typeahead('destroy')
            .off('.mytypeahead')
            .valid();
    }).on('change', 'input:checkbox:checked', function (e) {
        // Pass focus to typeahead tb
        $('input.typeahead', e.delegateTarget).focus();
    });





    //$('#addSpotCheck').on('click', 'button:not([data-dismiss])', function (e) {
    //    $.ajax({
    //        url: '@Url.Action(MVC_Receiving.Receiving.Rad.SetSpotCheckPercentage())',
    //        type: 'POST',
    //        context: this,
    //        data: $('#addSpotCheck').serializeArray(),
    //        statusCode: {
    //            // Success
    //            200: function (data, textStatus, jqXHR) {
    //                //updating the list with updated data
    //                $('#divSpotCheckList').html(data);
    //                $('#btnAddSpotCheck').modal('hide');
    //            },
    //            // Error
    //            203: function (data, textStatus, jqXHR) {
    //                //alert(data);
    //                $('div.text-danger', $('#addSpotCheck')).html(jqXHR.responseText);
    //            }
    //        },
    //        error: function (jqXHR, textStatus, errorThrown) {
    //            alert(jqXHR.responseText);
    //        }
    //    });
    //    return false;
    //});


});
