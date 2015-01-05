$(document).ready(function () {
    //Check if any of the pickslip is selected to add
    $('.addPickslips').on('click', function (e) {
        var isCheckked = $(":checkbox[name='@MVC_PickWaves.PickWaves.CreateWave.AddPickslipsToBucketParams.pickslips']", $('form')).is(":checked")
        if (!isCheckked) {
            $('[data-toggle="popover"]').popover('show');
            return false;
        }
    }).popover({
        trigger: 'manual',
        html: true,
        //Title to add a close icon
        title: '<strong class="text-danger"><span class="text-danger glyphicon glyphicon-warning-sign"></span> Error Message</strong><a class="close text-danger" href="#">&times;</a>',
        //Error message to display on the popover
        content: "You have not selected any Pickslip, please select atleast one Pickslip to add.",
        placement: 'auto',
        container: 'body'
    });

});
//close the popover error message on click of close icon on title
$(document).on('click', '.close', function (e) {
    $('[data-toggle="popover"]').popover('hide');
});