$(document).ready(function () {

    loadToastMessages();

    loadSelectedProduct();

    $("#showProductId").off("change").on("change", function () {

        var productId = $(this).val();

        if (productId === "") {
            $("#setupTableContainer").html("");
            return;
        }

        loadSetupTable(productId);
    });

    $("#btnAddSetupConfiguration").off("click").on("click", function () {

        var productId = $("#showProductId").val();

        $.ajax({

            type: "GET",
            url: "/ProductSetupConfiguration/Add",
            data: {
                productId: productId,
                rootIndex: -1
            },

            success: function (data) {

                $("#setupConfigurationModalContent").html(data);
                $("#setupConfigurationModal").modal("show");

            },

            error: function (xhr) {

                if (xhr.status === 401) {

                    showToastMessage("error", "Please login first.");
                    cleanSetupConfigurationModal();
                    return;
                }

                if (xhr.status === 403) {

                    showToastMessage("error", "Access denied. You do not have permission.");
                    cleanSetupConfigurationModal();
                    return;
                }

                showToastMessage("error", "An error occurred while loading the form.");
                cleanSetupConfigurationModal();
            }

        });

    });

    $(document).off("click", ".btnEditSetupConfiguration");

    $(document).on("click", ".btnEditSetupConfiguration", function () {

        var productId = $(this).data("product-id");
        var rootIndex = $(this).data("root-index");

        $.ajax({

            type: "GET",
            url: "/ProductSetupConfiguration/Add",
            data: {
                productId: productId,
                rootIndex: rootIndex
            },

            success: function (data) {

                $("#setupConfigurationModalContent").html(data);
                $("#setupConfigurationModal").modal("show");

            },

            error: function (xhr) {

                if (xhr.status === 401) {

                    showToastMessage("error", "Please login first.");
                    cleanSetupConfigurationModal();
                    return;
                }

                if (xhr.status === 403) {

                    showToastMessage("error", "Access denied. You do not have permission.");
                    cleanSetupConfigurationModal();
                    return;
                }

                showToastMessage("error", "An error occurred while loading the form.");
                cleanSetupConfigurationModal();
            }

        });

    });

    function loadSetupTable(productId) {

        $("#setupTableContainer").html(
            '<div class="text-center text-muted py-4">Loading...</div>'
        );

        $.ajax({

            type: "GET",
            url: "/ProductSetupConfiguration/ShowTable",
            data: {
                productId: productId
            },

            success: function (data) {

                $("#setupTableContainer").html(data);

                setTimeout(function () {
                    initializeSetupDataTable();
                }, 100);
            },

            error: function (xhr) {

                $("#setupTableContainer").html("");

                if (xhr.status === 401) {

                    showToastMessage("error", "Please login first.");
                    return;
                }

                if (xhr.status === 403) {

                    showToastMessage("error", "Access denied. You do not have permission.");
                    return;
                }

                showToastMessage("error", "An error occurred while loading setup table.");
            }

        });
    }

    function initializeSetupDataTable() {

        if ($("#setupDynamicTable").length === 0) {
            return;
        }

        if (typeof $.fn.DataTable === "undefined") {
            console.log("DataTable library is not loaded.");
            return;
        }

        if ($.fn.DataTable.isDataTable("#setupDynamicTable")) {
            $("#setupDynamicTable").DataTable().destroy();
        }

        $("#setupDynamicTable").DataTable({

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
                    targets: -1
                }
            ],

            dom:
                "<'row mb-3 px-2 pt-2 align-items-center'<'col-md-6'l><'col-md-6 text-right'f>>" +
                "<'row'<'col-12'tr>>" +
                "<'row mt-3 px-2 pb-2 align-items-center'<'col-md-6'i><'col-md-6 text-right'p>>",

            language: {
                search: "",
                searchPlaceholder: "Search"
            }
        });

        $(".dataTables_filter input")
            .addClass("form-control form-control-sm")
            .css({
                "width": "220px",
                "display": "inline-block"
            });

        $(".dataTables_length select")
            .addClass("custom-select custom-select-sm form-control form-control-sm")
            .css({
                "width": "70px",
                "display": "inline-block"
            });
    }

    function loadSelectedProduct() {

        var selectedProductId = parseInt($("#selectedProductId").val() || "0");

        if (selectedProductId > 0) {
            $("#showProductId").val(selectedProductId);
            loadSetupTable(selectedProductId);
        }
    }

    function loadToastMessages() {

        var successMessage = $("#successMessage").val();
        var tempSuccessMessage = $("#tempSuccessMessage").val();
        var tempErrorMessage = $("#tempErrorMessage").val();

        if (successMessage !== "") {
            showToastMessage("success", successMessage);
        }

        if (tempSuccessMessage !== "") {
            showToastMessage("success", tempSuccessMessage);
        }

        if (tempErrorMessage !== "") {
            showToastMessage("error", tempErrorMessage);
        }
    }

    function cleanSetupConfigurationModal() {

        $("#setupConfigurationModal").modal("hide");

        $("#setupConfigurationModalContent").html("");

        $(".modal-backdrop").remove();

        $("body").removeClass("modal-open");

        $("body").css("padding-right", "");
    }

    function showToastMessage(type, message) {

        if (message === null || message === undefined || message === "") {
            return;
        }

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
            toastr.success(message, "Success");
        }

        if (type === "error") {
            toastr.error(message, "Message");
        }

        if (type === "warning") {
            toastr.warning(message, "Message");
        }

        if (type === "info") {
            toastr.info(message, "Message");
        }
    }

});