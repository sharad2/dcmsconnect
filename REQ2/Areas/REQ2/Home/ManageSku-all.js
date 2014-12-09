/// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />

//Ajax call to add an SKU to the Request and updating the list
$(document).ready(function () {

    $('#tbNewStyle').autocomplete({
        minLenght: 0,
        source: function (request, response) {
            $.ajax({
                url: this.element.attr('data-ac-list-url'),
                dataType: 'json',
                context: this.element,
                beforeSend: function (jqXHR, settings) {
                    this.addClass('ui-autocomplete-loading');
                },
                data: { term: request.term },
                success: function (data, textStatus, jqXHR) {
                    response(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    // Error encountered during the remote call. Just show some diagnostic. If this happens, system becomes unstable
                    alert(jqXHR.responseText);
                },
                complete: function (jqXHR, textStatus) {
                    this.removeClass('ui-autocomplete-loading');
                }
            });
        },
        select: function (event, ui) {
            $('#tbTargetStyle').val(ui.item.value);
            $('#tbNewColor,#tbTargetColor').val(ui.item.color);
            $('#tbNewDimension,#tbTargetDimension').val(ui.item.dimension);
            $('#tbNewSkuSize,#tbTargetSkuSize').val(ui.item.skusize);
        },
        delay: 2000,
        change: function (event, ui) {
            $(this).change();
        }
    });


    $('#tbTargetStyle').autocomplete({
        minLenght: 0,
        source: function (request, response) {
            $.ajax({
                url: this.element.attr('data-ac-list-url'),
                dataType: 'json',
                context: this.element,
                beforeSend: function (jqXHR, settings) {
                    this.addClass('ui-autocomplete-loading');
                },
                data: { term: request.term },
                success: function (data, textStatus, jqXHR) {
                    response(data);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    // Error encountered during the remote call. Just show some diagnostic. If this happens, system becomes unstable
                    alert(jqXHR.responseText);
                },
                complete: function (jqXHR, textStatus) {
                    this.removeClass('ui-autocomplete-loading');
                }
            });
        },
        select: function (event, ui) {
            $('#tbTargetColor').val(ui.item.color);
            $('#tbTargetDimension').val(ui.item.dimension);
            $('#tbTargetSkuSize').val(ui.item.skusize);
        },
        delay: 2000,
        change: function (event, ui) {
            $(this).change();
        }
    });

    $('#tbNewStyle,#tbTargetStyle').dblclick(function (e) {
        // Double clicking will unconditionally open the drop down
        var oldMinLength = $(this).autocomplete('option', 'minLength');
        $(this).autocomplete('option', 'minLength', 0)
               .autocomplete('search');
        $(this).autocomplete('option', 'minLength', oldMinLength)
    });

    //Rk: Supports enter button to validate form.
    $('#frmAddSku').keypress(function (e) {
        if (e.which == $.ui.keyCode.ENTER) {
            if (!$('#frmAddSku').valid()) {
                return false;
            }
            $('#btnAddSku').click();
            e.preventDefault();
        }
    });
    $('#btnAddSku').button({ icons: { primary: 'ui-icon-plusthick' } }).click(function (e) {
        //Forcefully stopping the page to be posted if its invalid
        if (!$('form').valid()) {
            return false;
        }
        $.ajax({
            url: $('#frmAddSku').attr('action'),
            type: 'POST',
            data: $('#frmAddSku').serialize(), //send serialized form data in Ajax call.
            statusCode: {
                // Success
                200: function (data, textStatus, jqXHR) {
                    $('#divSkuList').html(data);
                    $('#frmAddSku div[data-valmsg-summary]')
                        .removeClass('validation-summary-errors')
                        .addClass('validation-summary-valid');
                    $("#btnAssign").button("option", "disabled", false);
                    $('#tbNewStyle').focus();
                },
                // Error
                203: function (data, textStatus, jqXHR) {
                    $('#frmAddSku').find('div[data-valmsg-summary]').html(data)
                        .removeClass('validation-summary-valid')
                        .addClass('validation-summary-errors');
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR.responseText);
            }
        });
        return false;
    });
});

//deleting SKU from the request and updating the list
$(document).ready(function () {
    $('div[data-delete-ajax-url]').click(function (e) {
        if ($(e.target).is('.ui-icon-close')) {
            if ($(e.target).is('.ui-state-disabled'))
            { return false; }
            var dialogData = new Object();
            var resvId = $('#resvId').val();
            var skuId = $(e.target).closest('tr').find('td.ui-helper-hidden > Input').val();
            dialogData["skuId"] = skuId;
            dialogData["resvId"] = resvId;
            var result = confirm("Are you sure, you want to delete?");
            if (result) {
                $.ajax({
                    url: $(this).attr('data-delete-ajax-url'),
                    type: 'POST',
                    context: $('#divTabs'),
                    data: dialogData,
                    statusCode: {
                        // Success
                        200: function (data, textStatus, jqXHR) {
                            //updating the list with updated data
                            $('#divSkuList').html(data);
                            if ($('#divSkuList').find('table').length == 0) {
                                $('#btnAssign').button('disable');
                            } else {
                                $("#btnAssign").button("option", "disabled", false);
                            }
                        },
                        // Error
                        203: function (data, textStatus, jqXHR) {
                            alert(data);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(jqXHR.responseText);
                    }
                });
            }
        }
    });
});
//Assigning cartons to the Request
$(document).ready(function () {
    $('#btnAssign').button({ icons: { primary: 'ui-icon-locked' } }).click(function (e) {
        var result = confirm("Are you sure, you want to assign cartons to request?");
        if (result) {
            $('#frmAssignCarton').submit();
        }
        else {
            return false;
        }
    });
});

//UnAssign cartons from the Request
$(document).ready(function () {
    $('button[data-name]').each(function () {
        $(this).button({ icons: { primary: 'ui-icon-unlocked' } }).click(function (e) {
            var result = confirm("Are you sure, you want to unassigned cartons from request?");
            if (result) {
                $('#frmUnAssignCarton').submit();
            } else {
                return false;
            }
        });
    });
});

$(document).ready(function () {
    $('#btnReset').button({ icons: { primary: 'ui-icon-refresh' } });
    $('#divTabs').tabs({
        create: function (event, ui) {
            $(this).tabs('option', 'active', parseInt($(this).attr('data-selected-index')));
        }
    });
    //Checking if request is already assigned, all controls must be disabled on the page
    if ($('#divAssignedCartonsInfo').find('table').length == 1) {
        $('#divSkuList div.ui-icon-close').addClass('ui-state-disabled');
    }
    if ($('#divSkuList').find('table').length == 0) {
        $('#btnAssign').button('disable');
    } else {
        $("#btnAssign").button("option", "disabled", false);
    }
});

//$Id$