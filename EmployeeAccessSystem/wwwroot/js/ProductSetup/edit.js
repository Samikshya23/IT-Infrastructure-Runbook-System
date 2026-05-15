$(document).ready(function () {

    // Update active/inactive label
    function updateEditStatus() {

        if ($('#EditIsActive').is(':checked')) {
            $('#editStatusLabel').text('Active');
        }
        else {
            $('#editStatusLabel').text('Inactive');
        }

    }

    // Load status label initially
    updateEditStatus();

    // Update label on switch change
    $('#EditIsActive').change(function () {
        updateEditStatus();
    });

});