$(document).ready(function () {
    $('#matrixPartial').pickslipmatrix({
        selected: function (event, ui) {
            $('#frmPickslipMatrix').submit();
        }
    });
    $('#bucketModelPartial').bucketmodel();
});