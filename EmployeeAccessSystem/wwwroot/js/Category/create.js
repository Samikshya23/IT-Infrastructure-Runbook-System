$(document).ready(function () {

    // Update active/inactive label
    function updateCreateStatus() {

        if ($('#CreateIsActive').is(':checked')) {
            $('#createStatusLabel').text('Active');
        }
        else {
            $('#createStatusLabel').text('Inactive');
        }

    }

    // Load status label initially
    updateCreateStatus();

    // Update label on switch change
    $('#CreateIsActive').change(function () {
        updateCreateStatus();
    });

});