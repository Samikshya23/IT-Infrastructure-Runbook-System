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

    // Open modal
    $(document).on('click', '.open-modal-btn', function (e) {

        e.preventDefault();

        var url = $(this).attr('href');
        var title = $(this).data('title');

        if (title) {

            $('#mainActionModal .modal-header').show();
            $('#mainActionModalTitle').text(title);

        }
        else {

            $('#mainActionModal .modal-header').hide();

        }

        $('#mainActionModalBody').html(
            '<div class="text-center py-3"><i class="fas fa-spinner fa-spin mr-2"></i> Loading...</div>'
        );

        $('#mainActionModalBody').load(url, function () {

            $('#mainActionModal').modal('show');

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

                // Ajax error
                error: function () {

                    alert('An error occurred while processing your request.');

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