$(document).ready(function () {

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

    $("#customSearch").on("keyup", function () {
        table.search(this.value).draw();
    });

    $("#customLength").on("change", function () {
        table.page.len($(this).val()).draw();
    });

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
                $("#createUserModal").modal({
                    backdrop: "static",
                    keyboard: false
                });
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
                $("#editUserModal").modal({
                    backdrop: "static",
                    keyboard: false
                });
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
                $("#userPermissionModal").modal({
                    backdrop: "static",
                    keyboard: false
                });

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

    $(document).on("click", ".btnDeleteUser", function () {
        var accountId = $(this).data("account-id");
        var userName = $(this).data("user-name");

        $("#deleteAccountId").val(accountId);
        $("#deleteUserName").text(userName);

        $("#deleteUserModal").modal({
            backdrop: "static",
            keyboard: false
        });
    });

    $(document).on("click", ".btnForgotPassword", function () {
        var accountId = $(this).data("account-id");
        var userName = $(this).data("user-name");

        $("#forgotAccountId").val(accountId);
        $("#forgotUserName").text(userName);
        $("#forgotNewPassword").val("");
        $("#forgotConfirmPassword").val("");

        $("#forgotPasswordModal").modal({
            backdrop: "static",
            keyboard: false
        });
    });

});