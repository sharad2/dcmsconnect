///<reference path="~/Scripts/jquery-1.6.2-vsdoc.js" />
///<reference path="~/Areas/Receiving/Scripts/Home/ReceivingCore.partial.js" />
///Print Carton ticket dialog
$(document).ready(function () {
    $("#dialog-print").dialog({
        autoOpen: false,
        height: 250,
        width: 400,
        modal: true,
        title: "Print Carton",
        draggable: true,
        buttons: {
            Print: function () {
                if ($('#Printers').val() != '') {

                    $.ajax({
                        url: $('#frmprint').attr('data-print-url'),
                        type: 'POST',
                        context: this,
                        data: {
                            cartonId: $(this).data('cartonId'),
                            printer: $('#ddlprinters').val()
                        },
                        //dataType: 'json',
                        statusCode: {
                            200: function (data, textStatus, jqXHR) {
                                $('#spanErrorMessage').show().html(data + "  ").removeClass('ui-state-error').addClass('ui-state-highlight');
                                $(this).dialog('close');
                            },
                            203: function (data, textStatus, jqXHR) {
                                //Some error, show them in screen and play error sound.
                                $('#dlgError').show().html(data);
                                PlaySound('error');
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            alert(jqXHR.responseText);
                            PlaySound('error');
                        },
                        complete: function (jqXHR, textStatus) {
                            setTimeout(function () {
                                $('#scan').val('').focus();
                            }, 0);
                        }
                    });
                }
                else {
                    $('#dlgError').show().html('Select Printer first');
                }
            },
            Close: function () {
                setTimeout(function () {
                    $('#scan').val('').focus();
                }, 0);
                $(this).dialog("close");
            }
        },
        close: function () {
            setTimeout(function () {
                $('#scan').val('').focus();
            }, 0);
            $('#CartonToPrint').val('');
        },
        open: function () {
            var $ddlPrinters = $('#ddlprinters');
            $ddlPrinters.children('option').remove();
            $('#CartonToPrint').text($(this).data('cartonId'));
            $('#dlgError').hide().html('');
            // ajax call to fill the dropdown list at run time.
            $.ajax({
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                url: $ddlPrinters.attr('data-getprinters-url'),
                dataType: "json",
                success: function (printers, textStatus, jqXHR) {
                    var selected = jqXHR.getResponseHeader("Selected");
                    $.each(printers, function (i, printer) {
                        var x = $('<option></option>').val(printer.Name).html(printer.Name + ' : ' + printer.Description);
                        if (printer.Name === selected) {
                            x.attr('selected', 'selected');
                        }
                        $ddlPrinters.append(x);
                    });
                },
                complete: function (jqXHR, textStatus) {
                    setTimeout(function () {
                        $('#scan').val('').focus();
                    }, 0);
                }
            });
        }
    });
});




//$Id: $