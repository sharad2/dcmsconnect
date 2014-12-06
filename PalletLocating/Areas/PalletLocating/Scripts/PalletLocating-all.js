

$(document).ready(function () {
    // Function trigger on user enter on scan textbox.
    // Set initial focus on scan text box
    $('#scan').keypress(function (e) {
        if (e.which == $.ui.keyCode.ENTER) {
            return handleScan.apply(this);
        }
    });
    function handleScan() {
        if ($('#scan').val() == '') {
            return false;
        }
        // Let the action tell us what to do with this scan
        $.ajax({
            url: $('form').attr('action'),
            type: 'POST',
            data: $('form').serialize(), //send serialized form data in ajax call.
            success: function (data, textStatus, jqXHR) {
                $('#message').html(data);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $('#message').html(jqXHR.responseText);
            }
        });
        return false;
    }
});



/*
$Id$ 
$Revision$
$URL$
$Header$
$Author$
$Date$
*/