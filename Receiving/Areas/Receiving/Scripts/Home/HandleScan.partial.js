/// <reference path="~/Scripts/jquery-1.6.2-vsdoc.js" />
/// <reference path="~/Areas/Receiving/Scripts/Home/Receiving.partial.js" />
/// Contract with home controller.
/// The outcome is indicated by an appropriate status code within the 200 range
/// 201 (Created): Carton Received. Pallet Html is provided as data. CartonId and Disposition in header. 
/// 202 (Accepted): Pallet scan. Pallet HTML is provided as data. PalletId and pallet disposition provided as header.
/// 203 (error): Indicates that some error has occured, show them in screen and play error sound.
/// 250 (custom): Carton not received due to disposition mismatch. Disposition is the data.
/// 251 (custom): Carton has already been received, Pallet ID on which carton was put is data, and error message will be provided in header against ErrorMsg
// Customized error message will be provided as ErrorMsg in header

(function ($) {

    // Array containing pallet dispositions. The pallet change sound corresponding to the index of the disposition is played.
    // Returns the number of the sound for the passed dispos. -1 if not found.
    // We play different sound for each area + vwh combination
    function GetSoundNumber(dispos) {
        var hash = 0;
        if (!dispos) return hash;
        for (i = 0; i < dispos.length; i++) {
            var c = dispos.charCodeAt(i);
            hash = ((hash << 5) - hash) + c;
            hash = hash & hash; // Convert to 32bit integer
        }
        hash = (Math.abs(hash) % 10) + 1;        // We only have ten sounds
        return hash;
    }
   
    var $_errorSpan;
    var $_tabs;

    // Status list contains multiple li's. Each li always has two spans. The first span displays progress. Second span displays message.
    // The status list contains errors plus pending ajax calls.
    var $_statusList;

    var _dialogOpenTimer;

    /**************** User Pause Handling *************/
    // This starts out null. Becomes non null when the user scans something. Becomes null again after 1 second pause from the user.
    var _idleTimer;

    // The data needed to update the screen after a carton is successfully received.
    var _successData = null;
    // Whenever the user pauses for a second, this function is called. It updates the screen based on the success data set by the ajax call.
    function UpdateSuccessDisplay() {
        if (_successData != null) {
            $('#scan').attr('disabled', 'disabled');
            PlaySound('success');  //successful pallet scan.
            //$('> div.ui-tabs-panel:not(.ui-tabs-hide)', $_tabs).html(_successData.data);           
            populateActiveTab(_successData.data, getActiveTab());
            var $li = $('ul.ui-tabs-nav li.ui-state-active', $_tabs).attr('data-disposition', _successData.cartonDisposition);
            var i = GetSoundNumber(_successData.cartonDisposition);
            if (i != -1) {
                $('a', $li).text(i.toString() + '. ' + $li.attr('data-palletid'));
            }
            _successData = null;
            $('#scan').removeAttr('disabled');
        }
        if (_idleTimer) {
            clearTimeout(_idleTimer);
        }
        _idleTimer = null;
    }

    /**************** End User Pause Handling *************/

    $.sequentialAjax = function (val) {
        if (_idleTimer) {
            clearTimeout(_idleTimer);
        }
        // After four seconds, update display
        _idleTimer = setTimeout(UpdateSuccessDisplay, 2000);
        if (!$_errorSpan) {
            // Remember these frequently used elements
            $_errorSpan = $('#spanErrorMessage');
            $_tabs = $('#tabs');
            //$_tbPalletDispos = $('#tbPalletDispos');
            $_statusList = $('#statusList');
        }
        $_errorSpan.hide();

        // If the ajax call takes too long, we want to open the dialog so that animated gif becomes visible
        // This timer is cleared after every ajax call completion.
        // In case of an error, the ajax error handler immediately opens the dialog.
        // As a result, when response is fast and no error are occuring, this dialog should never show.
        if (_dialogOpenTimer) {
            clearTimeout(_dialogOpenTimer);
        }
        // If ajax call does not complete within 2 sec, the dialog will popup
        _dialogOpenTimer = setTimeout(function () {
            $('#dlgScanCartonsStatus').dialog('open');
        }, 2000);

        // Add new tab when new pallet is scanned.
        // Activate newly added tab also populate hidden field for pallet
        function addTab(palletId) {
            //li = $(tabTemplate.replace(/#\{href\}/g, "#" + palletId).replace(/#\{label\}/g, palletId));
            li = "<li data-palletid='" + palletId + "' data-disposition='" + palletId + "'><a href='#Pallet_" + palletId + "' title='Pallet " + palletId + "'>" + palletId + "</a><span title='Remove Pallet' class='ui-icon ui-icon-close'></span></li>";
            $_tabs.find("ul.ui-tabs-nav").append(li);
            $("<div id='Pallet_" + palletId + "'></div>").appendTo($_tabs);
            $_tabs.tabs("refresh");
            index = $('li', $_tabs).length - 1;
            $_tabs.tabs('option', 'active', index);
            var $activeTab = getActiveTab()           
            $activeTab.effect('pulsate')          
            var x = $activeTab.attr('data-palletid');
            $('#tbPalletId').val(x);
            $_tabs.show();
        }

        $_statusList.prepend("<li><span></span><span>" + val + "</span></li>")
            .queue(function (next) {
                // If the queue is empty, this function is immediately invoked.
                // For non empty queues, this function is invoked by next(). next() is called when an ajax request completes.
                $('li:not(:has(span.ui-state-error)) span', $_statusList)
                    .eq(0)
                    .addClass('processing-state')
                    .end()
                    .eq(1)
                    .addClass('ui-state-active');
                $.ajax({
                    url: $('#scan').attr('data-ajax-url'),
                    type: 'POST',
                    data: $('#frmScan').serialize() + "&ScanModel.ScanText=" + val, //send serailzed form data in ajax call. TODO: remove the hardwired name in serialization of form data
                    statusCode: {
                        // Carton received
                        201: function (data, textStatus, jqXHR) {
                            _successData = { data: data, cartonDisposition: jqXHR.getResponseHeader('Disposition') || '' };
                            // Updating the overall receiving progress bar 
                            updateReceivingProgressBar(jqXHR.getResponseHeader('ReceivedCartonCount'), jqXHR.getResponseHeader('ExpectedCartonCount'));
                            // Updating the received cartons count value
                            $('#hfReceivedCartonsCount').val(jqXHR.getResponseHeader('ReceivedCartonCount'));
                            if (!_idleTimer) {
                                // User is not busy. Update now.
                                UpdateSuccessDisplay();
                            }
                        },
                        // Pallet scan. data is html
                        202: function (data, textStatus, jqXHR) {
                            var palletId = jqXHR.getResponseHeader("PalletId") || '';
                            var $li = $('> ul.ui-tabs-nav > li[data-palletid="' + palletId + '"]', $_tabs);
                            var index = $li.index();
                            var curPalletIndex = $_tabs.tabs('option', 'active');
                            if (index == -1) {
                                // dispos of the scanned pallet
                                var dispos = jqXHR.getResponseHeader("Disposition") || '';
                                // Disallow this pallet, if we already have another pallet of the same disposition
                                $li = $('> ul.ui-tabs-nav > li[data-disposition=' + dispos + ']', $_tabs);
                                var $palletId = $li.attr('data-palletid'); // finding pallet for this dispostion.
                                if ($palletId) {
                                    $_errorSpan.show().html('Pallet ' + $palletId + ' has the same disposition. Ignoring pallet scan.');
                                    PlaySound('warning');  //if we already have open pallet for this dispostion, play the warning sound.
                                    $li.effect('pulsate');
                                    return;
                                } else {
                                    // New pallet
                                    //$_tabs.tabs('add', palletId, dispos);
                                    //index = $_tabs.tabs('length') - 1;
                                    //$_tabs.tabs('select', index);
                                    //$li = $('> ul.ui-tabs-nav > li:last', $_tabs);
                                    addTab(palletId);
                                }
                            } else if (index == curPalletIndex) {
                                // Pallet is already active                              
                                $_errorSpan.show().html('Pallet ' + palletId + ' is already active. Please scan a carton.');
                                PlaySound('warning');
                            } else {
                                // Activate the pallet
                                //$_tabs.tabs('select', index);
                                $_tabs.tabs('option', 'active', index);
                            }
                            PlaySound('palletscan');  //successful pallet scan.
                            //$('> div.ui-tabs-panel:not(.ui-tabs-hide)', $_tabs).html(data);                                                    
                            //$li.effect('pulsate');
                            populateActiveTab(data, getActiveTab());
                        },
                        // Some error, show them in screen and play error sound.
                        203: function (data, textStatus, jqXHR) {
                            PlaySound('error');
                        },
                        // Carton disposition does not match active pallet disposition
                        250: function (data, textStatus, jqXHR) {
                            //var building = jqXHR.getResponseHeader("Building") || '';
                            var $li = $('> ul.ui-tabs-nav > li[data-disposition=' + data + ']', $_tabs);
                            var palletId = $li.attr('data-palletid');
                            if (!palletId) {
                                // See whether a null disposition pallet exists
                                $li = $('> ul.ui-tabs-nav > li[data-disposition=""]', $_tabs);
                                palletId = $li.attr('data-palletid');
                            }
                            var i = GetSoundNumber(data);
                            if (i == -1) {
                                i = GetSoundNumber(data);
                            }
                            if (palletId) {
                                $_errorSpan.show().html('Scan Pallet ' + palletId);
                                $li.each(function () {
                                    $(this).find('a').text(i.toString() + '. ' + $(this).attr('data-palletid'));
                                }).effect('pulsate');
                            }
                            else {
                                $_errorSpan.show().html('Scan New Pallet');
                            }

                            PlaySound(i.toString());
                        },
                        // Carton has already been received, pulsate the pallet if already opened and show the message to scan that pallet. Play the warninig sound.
                        251: function (data, textStatus, jqXHR) {
                            var $li = $('> ul.ui-tabs-nav > li[data-palletid=' + data + ']', $_tabs);
                            if ($li) {
                                $_errorSpan.show().html('Scan Pallet ' + data);
                                $li.effect('pulsate');
                            }
                            PlaySound('warning');
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(jqXHR.responseText);
                        PlaySound('error');
                        // This error is catastrophic. Clear the queue
                        $_statusList.queue([]);
                    }
                })
            .success(function (data, textStatus, jqXHR) {
                // If status = 203 or 250. Change class to ui-state-error of li which has class ui-state-active. Extract and append error message to li
                // Else remove li.
                var errMsg;
                if (jqXHR.status == 203) {
                    // Bad scan.
                    // Remove animation from the li having active class. Change active class to error class.
                    errMsg = data;
                }
                else if (jqXHR.status == 250) {
                    errMsg = jqXHR.getResponseHeader('ErrorMsg');
                } else if (jqXHR.status == 251) {
                    errMsg = jqXHR.getResponseHeader('ErrorMsg');
                }

                var $activeLi = $('li:has(span.ui-state-active)', $_statusList);
                if (_dialogOpenTimer) {
                    clearTimeout(_dialogOpenTimer);
                    _dialogOpenTimer = null;
                }
                if (errMsg) {
                    // Got error msg, show this  with time stammp and stop the animation
                    var date = new Date();
                    var h = date.getHours();
                    var m = date.getMinutes();
                    var s = date.getSeconds();
                    var time = h + ':' + m + ':' + s;
                    if (h > 11) {
                        time += ' PM';
                    } else {
                        time = time + ' AM';
                    }
                    $('span', $activeLi)
                        .eq(0)
                        .removeClass('processing-state')
                    .text(time + ' - ')
                    .addClass('ui-state-hover')
                        .end()
                        .eq(1)
                        .text(errMsg)
                        .removeClass('ui-state-active')
                        .addClass('ui-state-error');
                    $('#dlgScanCartonsStatus').dialog('open');
                } else {
                    // Success
                    $activeLi.remove();
                }
                next();
            });
            });
    };
})(jQuery);

//$Id$