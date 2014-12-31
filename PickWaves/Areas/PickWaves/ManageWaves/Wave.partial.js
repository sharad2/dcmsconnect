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

    //Tool tips on _buckerMoelPartial.cshtml
	
	    $(function () {
	        $("[data-toggle='tooltip']").tooltip();
	    });


});
