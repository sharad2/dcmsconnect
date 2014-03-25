$(document).ready(function () {
    $('#tb').autocomplete({
        source: $('a[data-shortname]').map(function () {
            return {
                label: $(this).attr('data-shortname').toUpperCase() + ': ' + $(this).text(),
                value: $(this).attr('data-shortname')
            }
        }).get().sort(function (a, b) {
            return (a.label < b.label ? -1 : 0);
        })   // BUG: sort() does not seem to be working
    }).on('keypress', function (e) {
        if (e.keyCode == $.ui.keyCode.ENTER) {
            var val = $(this).val();
            if (val) {
                // If the user entered 2a, we want to find an anchor tag whose id choice_2a or shose data-shortname attr is 2a
                var $a = $('a[data-seq=' + val + '],a[data-shortname=' + val + ']');
                if ($a.length == 0) {
                    // Just post the form and let the server handle the choice
                    $('#frmChoice').submit();
                } else {
                    // Redirect to link of the id entered by the user
                    window.location = $a.attr('href');
                }
            }
            return false;  // Prevent the default action
        }
    });
});
