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

            $.get("/ProductConfiguration/Add", function (data) {

                $("#configurationModalContent").html(data);

                $("#configurationModal").modal("show");

            });
        });
}

// Edit configuration
function bindEditButton() {

    $(document)
        .off("click", ".btnEditConfiguration");

    $(document)
        .on("click", ".btnEditConfiguration", function () {

            var productId =
                $(this).attr("data-product-id");

            $.get("/ProductConfiguration/Add?productId=" + productId,
                function (data) {

                    $("#configurationModalContent").html(data);

                    $("#configurationModal").modal("show");

                });
        });
}

// Show tempdata messages
function showTempMessage() {

    var successMessage =
        $("#SuccessMessage").val();

    var errorMessage =
        $("#ErrorMessage").val();

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