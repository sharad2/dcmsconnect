$(document).ready(function () {
    // Function trigger on user enter on scan textbox.
    // Set initial focus on scan text box
    var $_tb = $('#scan').keypress(function (e) {
        e.stopPropagation();
        if (e.which == $.ui.keyCode.ENTER) {
            return handleScan.apply(this);
        }
    });

    // Initial focus
    setTimeout(function () {
        $_tb.focus();
    }, 0);

    $('#btnNewPallet').click(function (e) {
        // Clicking this button is equivalent to scanning a special bar code.
        // Enter the special bar code in the text box and then ask the server to handle the scan.
        $_tb.val($(this).attr('data-new-pallet'));
        handleScan.apply($_tb[0]);
    }).button();
    //Try to set the focus to scan text box
    $('body').keypress(function () {
        $_tb.focus().val('');
    });

    //Creating dialog box to show the unsuccessful receiving
    $('#dlgScanCartonsStatus').dialog({
        close: function (event, ui) {
            $('ol', this).empty();
        },
        width: 300,
        height: 300,
        autoOpen: false,
        position: ["right", "top"],
        dialogClass: 'progress-dialog'
    });

    // Called when the user scans something
    // this refers to the scan text box
    function handleScan() {
        if ($.trim($(this).val())) {
            $.sequentialAjax($(this).val());
        }
        $(this).val('');
        return false;
    }
    $('#divCartonsDialog').dialog({
        title: 'Cartons Not On Pallet',
        autoOpen: false,
        modal: true,
        closeOnEscape: true,
        buttons: [
                      {
                          text: 'OK',
                          click: function (event, ui) {
                              $('#dlgError').html('');
                              $(this).dialog('close');
                          }
                      }
        ],
        open: function () {
            var processId = $('span.recv-processId').html();
            $.ajax({
                type: 'GET',
                context: this,
                url: $('#divCartonsDialog').attr('data-list-url'),
                //data will fetch and keep the list of unpalletize cartons of a particular ProcessId.
                data: { processId: processId },
                //cache of ajax is by default true so we have to make it false to clear the cache on each call.
                cache: false,
                statusCode:
                {
                    200: function (data, textStatus, jqXHR) {
                        $('#divCartonsDialog').html(data);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert(jqXHR.responseText);
                }
            });
        }
    });

    $('#linkShowCarton').click(function () {
        $('#divCartonsDialog').dialog('open');
    });
});

//function to play the sound.
function PlaySound(file) {
    var $sound = $('#sound_' + file);
    var $embed = $('embed', $sound).removeAttr('autostart').attr('autostart', true);
    $sound.children('div:first').html($embed[0].outerHTML);
    //Explicityly setting the focus to scan text box after playing the sound
    setTimeout(function () {
        $('#scan').val('').focus();
    }, 0);
}

//Function to update the overall progress bar of receiving.
//This methods take the receivedCartonCount and expectedCartonCount to update the progress bar.
function updateReceivingProgressBar(receivedCartonCount, expectedCartonCount) {
    var percentComplete = parseInt(receivedCartonCount * 100 / expectedCartonCount);
    var $progressBar = $('#receivingProgress');
    $progressBar.attr('title', percentComplete + '% completed, ' + receivedCartonCount + ' cartons out of ' + expectedCartonCount + ' expected cartons received');
    $('div', $progressBar)
        .eq(0).css('width', (percentComplete > 100 ? 100 : percentComplete) + '%')
        .end()
        .eq(1).find('span:first').text(percentComplete + '% [' + receivedCartonCount + ' of ' + expectedCartonCount + ' cartons]');
}

//Get activetab
function getActiveTab() {
    var $activeTab = $('> ul.ui-tabs-nav > li.ui-state-active', $('#tabs'));
    return $activeTab;
}

//Pouplate data in div of activetab,takes data to display as paramatere
function populateActiveTab(data,activeTab) {
    //var $activeTab = getActiveTab();
    var tabId = activeTab.attr('aria-controls');
    $('#' + tabId, $('#tabs')).html(data);
}

//$Id$