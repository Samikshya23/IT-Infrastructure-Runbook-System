$(document).ready(function () {

    initializeDataTable();

    bindAddButton();

    bindEditButton();

    showTempMessage();

});

// Initialize datatable
function initializeDataTable() {

    if (!$("#configurationTable").length) {
        return;
    }

    $("#configurationTable").DataTable({

        paging: true,

        searching: true,

        ordering: true,

        info: true,

        lengthChange: true,

        pageLength: 10,

        autoWidth: false,

        columnDefs: [
            {
                orderable: false,
                targets: 2
            }
        ],

        dom:
            '<"row px-2 py-2 align-items-center"' +
            '<"col-md-6"l>' +
            '<"col-md-6 text-right"f>' +
            '>' +
            'rt' +
            '<"row px-2 py-2"' +
            '<"col-md-6"i>' +
            '<"col-md-6"p>' +
            '>',

        language: {

            search: "",

            searchPlaceholder: "Search"
        }
    });
}

// Add configuration
function bindAddButton() {

    $("#btnAddConfiguration")
        .off("click")
        .on("click", function () {

            loadConfigurationModal("/FormConfiguration/Add");

        });
}

// Edit configuration
function bindEditButton() {

    $(document)
        .off("click", ".btnEditConfiguration");

    $(document)
        .on("click", ".btnEditConfiguration", function () {

            var categoryId = $(this).attr("data-category-id");

            loadConfigurationModal("/FormConfiguration/Add?categoryId=" + categoryId);

        });
}

// Load configuration modal after permission check
function loadConfigurationModal(url) {

    $.ajax({

        type: "GET",
        url: url,

        success: function (data) {

            $("#configurationModalContent").html(data);

            $("#configurationModal").modal("show");

        },

        error: function (xhr) {

            if (xhr.status === 401) {

                showToastMessage("error", "Please login first.");

                cleanConfigurationModal();

                return;
            }

            if (xhr.status === 403) {

                showToastMessage("error", "Access denied. You do not have permission.");

                cleanConfigurationModal();

                return;
            }

            showToastMessage("error", "An error occurred while loading the form.");

            cleanConfigurationModal();
        }

    });
}

// Clean modal
function cleanConfigurationModal() {

    $("#configurationModal").modal("hide");

    $("#configurationModalContent").html("");

    $(".modal-backdrop").remove();

    $("body").removeClass("modal-open");

    $("body").css("padding-right", "");
}

// Show tempdata messages
function showTempMessage() {

    var successMessage = $("#SuccessMessage").val();

    var errorMessage = $("#ErrorMessage").val();

    if (successMessage !== "") {

        showToastMessage("success", successMessage);
    }

    if (errorMessage !== "") {

        showToastMessage("error", errorMessage);
    }
}

// Toast message
function showToastMessage(type, message) {

    if (typeof toastr === "undefined") {

        alert(message);

        return;
    }

    toastr.options = {

        closeButton: true,

        progressBar: true,

        newestOnTop: true,

        positionClass: "toast-top-right",

        preventDuplicates: true,

        timeOut: "3000"
    };

    if (type === "success") {

        toastr.success(message);
    }

    if (type === "error") {

        toastr.error(message);
    }

    if (type === "warning") {

        toastr.warning(message);
    }

    if (type === "info") {

        toastr.info(message);
    }
}