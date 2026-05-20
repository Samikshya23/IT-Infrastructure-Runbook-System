$(document).ready(function () {

    // Initialize data table
    var table = $('#productSetupTable').DataTable({

        paging: true,
        searching: true,
        ordering: true,
        info: true,
        lengthChange: false,
        pageLength: 10,
        autoWidth: false,

        columnDefs: [
            {
                orderable: false,
                targets: 2
            }
        ],

        dom: 'rtip'

    });

    // Search functionality
    $('#customSearch').on('keyup', function () {

        table.search(this.value).draw();

    });

    // Page length change
    $('#customLength').on('change', function () {

        table.page.len(this.value).draw();

    });

    // Open modal for Add, Edit and Delete
    $(document).on('click', '.open-modal-btn', function (e) {

        e.preventDefault();

        var url = $(this).attr('href');
        var title = $(this).data('title');

        // Important:
        // Do not open modal before permission is checked.
        // First call the action through AJAX.
        $.ajax({

            type: 'GET',
            url: url,

            success: function (response) {

                // Set modal title only after successful permission check
                if (title) {

                    $('#mainActionModal .modal-header').show();
                    $('#mainActionModalTitle').text(title);

                }
                else {

                    $('#mainActionModal .modal-header').hide();

                }

                // Load response into modal body
                $('#mainActionModalBody').html(response);

                // Open modal only after successful response
                $('#mainActionModal').modal('show');

            },

            error: function (xhr) {

                // If user is not logged in
                if (xhr.status === 401) {

                    toastr.error('Please login first.');
                    cleanMainActionModal();
                    return;

                }

                // If user does not have permission
                if (xhr.status === 403) {

                    toastr.error('Access denied. You do not have permission.');
                    cleanMainActionModal();
                    return;

                }

                // Other error
                toastr.error('An error occurred while loading the form.');
                cleanMainActionModal();

            }

        });

    });

    // Handle create, edit and delete form submit
    $(document).on('submit',
        '#createProductForm, #editProductForm, #deleteProductForm',
        function (e) {

            e.preventDefault();

            var form = $(this);
            var actionUrl = form.attr('action');
            var formData = form.serialize();

            $.ajax({

                type: 'POST',
                url: actionUrl,
                data: formData,

                success: function (response) {

                    // Success response
                    if (typeof response === 'string'
                        && response.indexOf('success|') === 0) {

                        var successMessage = response.substring(8);

                        $('#mainActionModal').modal('hide');

                        window.location.href =
                            '/ProductSetup/Index?successMessage='
                            + encodeURIComponent(successMessage);

                        return;
                    }

                    // Error response
                    if (typeof response === 'string'
                        && response.indexOf('error|') === 0) {

                        var errorMessage = response.substring(6);

                        $('#mainActionModal').modal('hide');

                        window.location.href =
                            '/ProductSetup/Index?errorMessage='
                            + encodeURIComponent(errorMessage);

                        return;
                    }

                    var html = $('<div>').html(response);

                    var hasForm =
                        html.find('#createProductForm').length > 0 ||
                        html.find('#editProductForm').length > 0 ||
                        html.find('#deleteProductForm').length > 0;

                    // Reload form with validation errors
                    if (hasForm) {

                        $('#mainActionModalBody').html(response);

                    }
                    else {

                        $('#mainActionModal').modal('hide');

                        window.location.href = '/ProductSetup/Index';

                    }

                },

                error: function (xhr) {

                    // If user is not logged in
                    if (xhr.status === 401) {

                        toastr.error('Please login first.');
                        cleanMainActionModal();
                        return;

                    }

                    // If user does not have permission
                    if (xhr.status === 403) {

                        toastr.error('Access denied. You do not have permission.');
                        cleanMainActionModal();
                        return;

                    }

                    // Other error
                    toastr.error('An error occurred while processing your request.');

                }

            });

        });

    // Reset modal
    $('#mainActionModal').on('hidden.bs.modal', function () {

        $('#mainActionModalBody').html('');

        $('#mainActionModal .modal-header').show();

        $('#mainActionModalTitle').text('Product Setup');

    });

    // Show success toast
    if ($('#successToast').length) {

        $('#successToast').toast('show');

    }

    // Show error toast
    if ($('#errorToast').length) {

        $('#errorToast').toast('show');

    }

});

// Common function to fully close and clean modal
function cleanMainActionModal() {

    $('#mainActionModal').modal('hide');
    $('#mainActionModalBody').html('');

    $('.modal-backdrop').remove();
    $('body').removeClass('modal-open');
    $('body').css('padding-right', '');

}