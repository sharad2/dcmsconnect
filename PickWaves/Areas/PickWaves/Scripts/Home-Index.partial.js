$(document).ready(function () {
    //$('ul.customerActionMenu').menu({
    //    select: function (event, ui) {
    //        alert('Hi');
    //    }
    //});
    $('#divTabs').tabs({
        heightStyle: 'auto',
        hide: true,
        show: true,
        active: $.cookie('pickwave_home_index_tab'), //Setting the active tab by cookies
        activate: function (event, ui) {
            //Remebering the active tab in cookies
            $.cookie('pickwave_home_index_tab', $(this).tabs('option', 'active'), { expires: 3 });
        },
        beforeLoad: function (event, ui) {
            ui.panel.removeClass('ui-state-error');
            ui.jqXHR.error(function (jqXHR, textStatus, errorThrown) {
                ui.panel.html(jqXHR.responseText).addClass('ui-state-error');
            });
            // This is supposed to prevent caching of remote tab content but it does not work
            ui.ajaxSettings.cache = false;
        }
    });
    $('#tbCustomer').autocomplete({
        minLength: 0,
        source: $('#tbCustomer').attr('data-url'),
        autoFocus: true
    }).on('dblclick', function (e) {
        $(this).autocomplete('search');
    });

    $('#divTabs').on('mouseleave', 'ul.customerActionMenu', function (e) {
        // Close the menu if user navigates out of it
        $(this).menu('collapseAll', e, true);
    });
});