$(document).ready(function () {

    // User table
    var table = $("#usersTable").DataTable({
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

    // Custom search
    $("#customSearch").on("keyup", function () {
        table.search(this.value).draw();
    });

    // Custom page length
    $("#customLength").on("change", function () {
        table.page.len($(this).val()).draw();
    });

    // Create user modal
    $(document).on("click", "#btnCreateUser", function () {

        $.ajax({
            url: "/AccessControl/CreateUserModal",
            type: "GET",
            success: function (response) {

                if (typeof response === "string" && response.indexOf("error|") === 0) {
                    toastr.error(response.replace("error|", ""));
                    return;
                }

                $("#createUserModalContent").html(response);
                $("#createUserModal").modal("show");
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

                toastr.error("Unable to load add user form.");
            }
        });

    });

    // Details modal
    $(document).on("click", ".btnDetailsUser", function () {

        var accountId = $(this).data("account-id");

        $.ajax({
            url: "/AccessControl/DetailsModal",
            type: "GET",
            data: { accountId: accountId },
            success: function (response) {

                if (typeof response === "string" && response.indexOf("error|") === 0) {
                    toastr.error(response.replace("error|", ""));
                    return;
                }

                $("#detailsModalContent").html(response);
                $("#detailsModal").modal("show");
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

                toastr.error("Unable to load user details.");
            }
        });

    });

    // Edit modal
    $(document).on("click", ".btnEditUser", function () {

        var accountId = $(this).data("account-id");

        $.ajax({
            url: "/AccessControl/EditUserModal",
            type: "GET",
            data: { accountId: accountId },
            success: function (response) {

                if (typeof response === "string" && response.indexOf("error|") === 0) {
                    toastr.error(response.replace("error|", ""));
                    return;
                }

                $("#editUserModalContent").html(response);
                $("#editUserModal").modal("show");
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

                toastr.error("Unable to load user edit form.");
            }
        });

    });

    // User permission modal
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

                $("#userPermissionModal").modal("hide");
                $("#userPermissionModalContent").html("");

                toastr.success("Permission saved successfully.");
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

    // Delete user modal
    $(document).on("click", ".btnDeleteUser", function () {

        var accountId = $(this).data("account-id");
        var userName = $(this).data("user-name");

        $("#deleteAccountId").val(accountId);
        $("#deleteUserName").text(userName);

        $("#deleteUserModal").modal("show");

    });

});