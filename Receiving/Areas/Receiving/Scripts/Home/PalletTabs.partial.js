/// <reference path="~/Scripts/jquery-1.6.2-vsdoc.js" />
/// <reference path="~/Areas/Receiving/Scripts/Home/ReceivingCore.partial.js" />
/// <reference path="~/Areas/Receiving/Scripts/Home/Receiving.partial.js" />

// Pallet tabs
$(document).ready(function () {
    $("#tabs").tabs({
        tabTemplate: "<li data-palletid='#{href}' data-disposition='#{label}'><a href='#Pallet_{href}' title='Pallet #{href}'>#{href}</a><span title='Remove Tab' class='ui-icon ui-icon-close'></span></li>"
    }).click(function (e) {
        // call on closing of the pallet.
        var cartonId;
        if ($(e.target).is('ul.ui-tabs-nav .ui-icon-close')) {
            var index = $("li", this).index($(e.target).parent());
            //$(this).tabs("remove", index);
            //var li = $(this).find(".ui-tabs-active").remove().attr("aria-controls");
            var li = $(this).find($("li:eq('" + index + "')", this)).remove().attr("aria-controls");
            $("#" + li).remove();
            $("#tabs").tabs("refresh");
            if ($('li', $('#tabs')).length == 0) {
                $('#tabs').hide();
                // Remove pallet and dispos from hidden field as there are no tabs.
                $('#tbPalletId').val("");
            }
        } else if ($(e.target).is('div.ui-tabs-panel .ui-icon-close')) {
            // call on unreceiving the carton.
            cartonId = $(e.target).parents('tr').find('.recv-carton').html();
            var $activeTab = getActiveTab();
            var palletId = $activeTab.attr('data-palletid');
            if (confirm("Do you want to remove carton " + cartonId + " from pallet " + palletId + "?")) {
                //remove the carton from pallet.
                $.ajax({
                    url: $(this).attr('data-remove-carton'),
                    type: 'POST',
                    data: JSON.stringify({
                        cartonId: cartonId,
                        processId: $('span.recv-processId').html(),
                        palletId: palletId
                    }),
                    context: this,
                    contentType: 'application/json; charset=utf-8',
                    statusCode: {
                        // Some error, show them in screen and play error sound.
                        203: function (data, textStatus, jqXHR) {
                            $('#spanErrorMessage').show().html(data);
                            PlaySound('error');
                        },
                        // Unreceive success
                        200: function (data, textStatus, jqXHR) {
                            var dispos = jqXHR.getResponseHeader("Disposition") || '';
                            //$('> div.ui-tabs-panel:not(.ui-tabs-hide)', this).html(data);
                            populateActiveTab(data, getActiveTab());
                            $('> ul.ui-tabs-nav > li.ui-state-active', this).attr('data-disposition', dispos);
                            //Decreasing the value of hidden field for Received Cartons Count.
                            var receivedCartonsCount = parseInt($('#hfReceivedCartonsCount').val()) - 1;
                            $('#hfReceivedCartonsCount').val(receivedCartonsCount);
                            //Updating the Over all receiving progressbar.
                            updateReceivingProgressBar(receivedCartonsCount, $('#hfExpectedCartonsCount').val());
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        $('> div.ui-tabs-panel:not(.ui-tabs-hide)', this).html(jqXHR.responseText);
                        PlaySound('error');
                    },
                    complete: function (jqXHR, textStatus) {
                        setTimeout(function () {
                            $('#scan').val('').focus();
                        }, 0);
                    }
                });
            } else {
                $(e.target).parent().removeClass('ui-state-active');
            }
        } else if ($(e.target).is('div.ui-tabs-panel .ui-icon-print')) {
            cartonId = $(e.target).parents('tr').find('.recv-carton').html();
            $('#dialog-print').data('cartonId', cartonId).dialog('open');
        } else if ($(e.target).is('a')) {
            // refresh value of hidden field for pallet when tab is changed
            var $activeTab = getActiveTab();
            var palletId = $activeTab.attr('data-palletid');
            $('#tbPalletId').val("");
            $('#tbPalletId').val(palletId);

        }
    });
});



//$Id$