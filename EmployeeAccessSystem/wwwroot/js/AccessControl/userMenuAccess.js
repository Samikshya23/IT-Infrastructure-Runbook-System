$(document).ready(function () {

    // User permission table
    var table = $("#userMenuAccessTable").DataTable({
        pageLength: 10,
        lengthChange: false,
        searching: true,
        ordering: true,
        info: true,
        paging: true,
        autoWidth: false,
        responsive: true,
        dom: "rtip",
        columnDefs: [
            {
                targets: 4,
                orderable: false,
                searchable: false
            }
        ]
    });

    // Search
    $("#accessCustomSearch").on("keyup", function () {
        table.search(this.value).draw();
    });

    // Page length
    $("#accessCustomLength").on("change", function () {
        table.page.len($(this).val()).draw();
    });

    // Open user permission modal
    $(document).on("click", ".btnUserPermission", function () {

        var accountId = $(this).data("account-id");

        $.ajax({
            url: "/AccessControl/UserPermissionModal",
            type: "GET",
            data: { accountId: accountId },
            success: function (response) {

                if (typeof response === "string" && response.indexOf("error|") === 0) {
                    toastr.error(response.replace("error|", ""));
                    return;
                }

                $("#userPermissionModalContent").html(response);
                $("#userPermissionModal").modal("show");

                updateSelectAllPermission();
            },
            error: function (xhr) {

                if (xhr.status === 401) {
                    toastr.error("Please login first.");
                    return;
                }

                if (xhr.status === 403) {
                    toastr.error("Access denied. You do not have permission.");
                    return;
                }

                toastr.error("Unable to load user permission.");
            }
        });

    });

    // Save user permission from modal
    $(document).on("submit", "#userPermissionForm", function (e) {

        e.preventDefault();

        var form = $(this);

        $.ajax({
            url: form.attr("action"),
            type: "POST",
            data: form.serialize(),
            success: function (response) {

                if (response != null && response.success === true) {
                    $("#userPermissionModal").modal("hide");
                    $("#userPermissionModalContent").html("");

                    toastr.success(response.message);
                    return;
                }

                if (response != null && response.success === false) {
                    toastr.error(response.message);
                    return;
                }

                toastr.success("Permission saved successfully.");
                $("#userPermissionModal").modal("hide");
            },
            error: function (xhr) {

                if (xhr.status === 401) {
                    toastr.error("Please login first.");
                    return;
                }

                if (xhr.status === 403) {
                    toastr.error("Access denied. You do not have permission.");
                    return;
                }

                toastr.error("Unable to save permission.");
            }
        });

    });

    // Clear user permission modal
    $(document).on("click", ".btnClearUserAccess", function () {

        var accountId = $(this).data("account-id");
        var userName = $(this).data("user-name");

        $("#clearAccountId").val(accountId);
        $("#clearUserName").text(userName);
        $("#clearUserAccessModal").modal("show");

    });

});