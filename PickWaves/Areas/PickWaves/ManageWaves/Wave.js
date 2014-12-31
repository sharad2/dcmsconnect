﻿///#source 1 1 /Areas/PickWaves/ManageWaves/Wave.partial.js
$(document).ready(function (e) {
	"use strict";
	// Make the first tab active. Load tab content when it becomes active
	$('#tabs').on('show.bs.tab', function (e) {
		// Load AJAX content when the tab is shown
		$('img', e.target).removeClass('hidden');
		//alert(x.length);
		$.ajax($(e.target).data('href'), {
			type: 'GET',
			cache: false
		}).done(function (data, textStatus, jqXHR) {
			//success
			$($(e.target).attr('href')).html(data);
		}).fail(function (jqXHR, textStatus, errorThrown) {
			//error
			alert(jqXHR.responseText);
		}).always(function () {
			$('img', e.target).addClass('hidden');
		});
	}).on('click', 'button[data-pickslip-remove-url]', function (e) {
		// Remove Pickslip

		$.post($(e.target).data('pickslip-remove-url')).done(function (data, textStatus, jqXHR) {
			//success
			$(e.target).closest('li.list-group-item').addClass('list-group-item-warning')
			  .find('a.pickslip').css('text-decoration', 'line-through');
		}).error(function (jqXHR, textStatus, errorThrown) {
			//error
			alert.html(jqXHR.responseText);
		});
	}).find('> ul a:first').tab('show');


});

///#source 1 1 /Areas/PickWaves/SharedViews/_bucketModel.partial.js
$(document).ready(function () {
    "use strict";
    // Handle priority up and down buttons
    $(document).on('click', 'button[data-priority-url]', function (e) {
        $.post($(e.target).data('priority-url')).done(function (data, textStatus, jqXHR) {
           // alert(data);
            $(e.target).closest('div.input-group').find('input:text').val(data);
        }).error(function () {
            alert('error');
        });
    });
});


//Tool tips on _buckerMoelPartial.cshtml
$(document).ready(function () {
    $(function () {
        $("[data-toggle='tooltip']").tooltip();
    });
});

