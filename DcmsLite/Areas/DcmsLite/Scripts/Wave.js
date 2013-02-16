$(document).ready(function () {
    $('#btnSearch').button({ icons: { primary: 'ui-icon-search' } }).on('click', function (e) {
        var $form = $(this).closest('form');
        $.ajax({
            url: $form.attr('action'),
            data: $form.serializeArray(),
            type: 'GET',
            cache: false,
            statusCode: {
                200: function (data, textStatus, jqXHR) {
                    //alert('200 ' + data);
                    window.location.href = data;
                    return true;
                },
                203: function (data, textStatus, jqXHR) {
                    $('#layoutError').text(data).show();
                }
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR.responseText);
        });
        return false;
    });
});

$(document).ready(function () {
    $('#printerlist').on('change', 'input:checkbox', function (e) {
        var count = $('input:checkbox:checked', e.delegateTarget).length;
        if (count) {
            $('#btnPrintNow').button('enable');
            $('#printercount').text(count);
        } else {
            $('#btnPrintNow').button('disable');
            $('#printercount').empty();
        }
    });
    $('#tabs').tabs();
    $('#tbBatchSize').spinner({
        min: 0,
        max:10000,
        step: 50,
        change: function (event, ui) {
            if (ui.value > 10000) {
                return false;
            }
            return true;
        }
    });
    // $('button').button();
    //$("#slider").slider({
    //    value: 100,
    //    min: 50,
    //    max: 500,
    //    step: 50,
    //    create: function (event, ui) {
    //        $("input", this).val($(this).slider('value'));
    //    },
    //    slide: function (event, ui) {
    //        $(this).attr('title', ui.value);
    //        $("input", this).val(ui.value);
    //    }
    //});
    $('#btnPrintNow').button({ icons: { primary: 'ui-icon-print' } })
        .on('click', function (e) {
            if ($(this).queue().length > 0) {
                $(this).clearQueue()
                    //.button('option', 'label', 'Stopping')
                    .button('disable');
                $('#stopPrinting').text('Stopping');
                return false;
            }
            //$(this).button('option', 'label', 'Stop');
            $('#startPrinting').hide();
            $('#stopPrinting').show().text('Stop');
            // Hide all progress buttons
            $('#printerlist').find('span.printing,span.printed').css('visibility', 'hidden')
                .end()
                .find('input:checked')
                .each($.proxy(function (i, elem) {
                    $(this).queue($.proxy(function (next) {
                        var data = {};
                        data[$('#bucketId').attr('name')] = $('#bucketId').val();
                        data[$(this.checkbox).attr('name')] = $(this.checkbox).val();
                        data[$('#tbBatchSize').attr('name')] = $('#tbBatchSize').val();
                        data[$(this.button).attr('data-clearcookie')] = this.index == 0;
                        $.ajax({
                            url: $('#frmPrintNewBatch').attr('action'),
                            data: data,
                            type: 'POST',
                            cache: false,
                            context: this,
                            beforeSend: function () {
                                $(this.checkbox).parent().nextAll('span.printing').css('visibility', 'visible');
                            }
                        }).done(function (data, textStatus, jqXHR) {
                            var $a = $(this.checkbox).parent().nextAll('span.printing').hide()
                                .end()
                                .nextAll('span.printed').css('visibility', 'visible')
                                .find('a');
                            if (data) {
                                var href = $a.attr('href');
                                $a.attr('href', href.substr(0, href.indexOf('=') + 1) + data).text(data);
                                next();
                            } else {
                                $a.removeAttr('href').text('Nothing Printed');
                                $(this.button).clearQueue();
                            }
                        }).fail(function (jqXHR, textStatus, errorThrown) {
                            // Do not run any more print statements
                            $(this.button).clearQueue();
                            alert(jqXHR.responseText);
                        }).always(function (jqXHR, textStatus) {
                            if ($(this.button).queue().length == 0) {
                                // No more ajax calls left.
                                $(this.button).hide();
                                $('#aRefresh').show();
                            }
                            $(this.checkbox).parent().nextAll('span.printing').css('visibility', 'hidden');
                        });
                    }, { checkbox: elem, button: this, index: i }));
                }, this));
            return false;
        });
});
