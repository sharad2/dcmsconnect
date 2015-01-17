///// <reference path="../../../Scripts/jquery-1.6.2-vsdoc.js" />
//$(document).ready(function () {
//    alert('I am in there');
//    $('#btnGo').button({ icons: { secondary: 'ui-icon-search'} }).click(function (e) {
//        var RE = /^\d+$/;    //Regular expression to validate the number for ctnresvId 
//        var id = $('#frmSearch input:text').val();
//        //validating the ctnresvId
//        if (id == '' || !RE.test(id)) {
//            $('#divError').html("Please enter the valid Request ID")
//                               .removeClass('validation-summary-valid')
//                                .addClass('validation-summary-errors');
//            return false;
//        }
//    });
//    $('div[data-delete-ajax-url]').mouseover(function (e) {
//        $(e.target).closest('tbody>tr').addClass('ui-state-hover');
//    });
//    $('div[data-delete-ajax-url]').mouseout(function (e) {
//        $(e.target).closest('tbody>tr').removeClass('ui-state-hover');
//    });
//    $('td.edit-icon a').addClass('ui-icon ui-icon-pencil').removeClass('ui-state-disabled').removeAttr('disabled', 'disabled');
//    $('td.edit-icon-disabled a').addClass('ui-icon ui-icon-pencil ui-state-disabled').attr('disabled', 'disabled');
//    $('td.delete-icon-disabled div').addClass('ui-icon ui-icon-close ui-state-disabled').attr('disabled', 'disabled');
//});
////deleting request from the request and updating the list
//$('div[data-delete-ajax-url]').click(function (e) {
//    if ($(e.target).is('.ui-icon-close')) {
//        if ($(e.target).is('.ui-state-disabled'))
//        { return false; }
//        var dialogData = new Object();
//        var resvId = $(e.target).closest('tr').find('td.ui-helper-hidden > Input').val();
//        var reqId = $(e.target).closest('tr').find('td.data-reqId a').text();
//        dialogData["resvId"] = resvId;
//        var result = confirm("Are you sure, you want to delete Request# " + reqId + " ?");
//        if (result) {
//            $.ajax({
//                url: $(this).attr('data-delete-ajax-url'),
//                type: 'POST',
//                context: this,
//                data: dialogData,
//                statusCode: {
//                    // Success
//                    200: function (data, textStatus, jqXHR) {
//                        //updating the list with updated data
//                        $(this).html(data);
//                        $('td.edit-icon a').addClass('ui-icon ui-icon-pencil').removeClass('ui-state-disabled').removeAttr('disabled', 'disabled');
//                        $('td.edit-icon-disabled a').addClass('ui-state-disabled').attr('disabled', 'disabled');
//                        $('td.delete-icon-disabled div').addClass('ui-state-disabled').attr('disabled', 'disabled');
//                    },
//                    // Error
//                    203: function (data, textStatus, jqXHR) {
//                        $('#ajaxError').html(data)
//                               .removeClass('validation-summary-valid')
//                                .addClass('validation-summary-errors');
//                    }
//                },
//                error: function (jqXHR, textStatus, errorThrown) {
//                    alert(jqXHR.responseText);
//                }
//            });
//        }
//    }
//});
////$Id$