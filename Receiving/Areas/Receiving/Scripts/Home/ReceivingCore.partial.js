$(document).ready(function () {
    $('#scan').keypress(function (e) {
        // Won't let the body event handler receive this event
        e.stopPropagation();
    }).focus();

    $('body').keypress(function () {
        $('#scan').focus().val('');
    });

    $('#dlgScanCartonsStatus').dialog({
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close", $(this).parent()).hide();
        },
        title: "Unsuccessful receiving",
        width: 300,
        height: 300,
        autoOpen: true,
        position: ["right", "top"],
        closeOnEscape: false
    }).dialog('show');
});

//function to play the sound.
function PlaySound(file) {
    var $sound = $('#sound_' + file);
    var $embed = $('embed', $sound).removeAttr('autostart').attr('autostart', true);
    $sound.children('div:first').html($embed[0].outerHTML);
}



//$Id: ReceivingCore.partial.js 12115 2012-02-07 11:46:15Z bkumar $