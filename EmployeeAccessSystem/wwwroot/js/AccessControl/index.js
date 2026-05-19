$(document).ready(function () {

    if ($("#accessControlTable").length > 0) {
        $("#accessControlTable").DataTable({
            pageLength: 10,
            lengthChange: true,
            searching: true,
            ordering: true,
            info: true,
            paging: true,
            autoWidth: false,
            responsive: true,
            columnDefs: [
                {
                    targets: 4,
                    orderable: false,
                    searchable: false
                }
            ]
        });
    }

    $(document).on("click", "#btnManageUsers", function () {
        $("#usersSection").removeClass("d-none");

        $("html, body").animate({
            scrollTop: $("#usersSection").offset().top - 70
        }, 400);
    });

    $(document).on("click", "#btnManageRoles", function () {
        alert("Role Management will be added next.");
    });

    $(document).on("click", "#btnManageMenus", function () {
        alert("Menu Management will be added next.");
    });

    $(document).on("click", ".btnDetailsUser", function () {
        var accountId = $(this).data("account-id");

        $.ajax({
            url: "/AccessControl/DetailsModal",
            type: "GET",
            data: { accountId: accountId },
            success: function (response) {
                $("#detailsModalContent").html(response);
                $("#detailsModal").modal("show");
            },
            error: function () {
                alert("Unable to load user details.");
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
                $("#editUserModalContent").html(response);
                $("#editUserModal").modal("show");
            },
            error: function () {
                alert("Unable to load user edit form.");
            }
        });
    });

    $(document).on("click", ".btnMenuAccess", function () {
        var accountId = $(this).data("account-id");

        $.ajax({
            url: "/AccessControl/AccessModal",
            type: "GET",
            data: { accountId: accountId },
            success: function (response) {
                $("#accessModalContent").html(response);
                $("#accessModal").modal("show");
                updateSelectAllState();
            },
            error: function () {
                alert("Unable to load menu access.");
            }
        });
    });

    $(document).on("click", ".btnRemoveAccess", function () {
        var accountId = $(this).data("account-id");
        var userName = $(this).data("user-name");

        $("#removeAccountId").val(accountId);
        $("#removeUserName").text(userName);
        $("#removeAccessModal").modal("show");
    });

    $(document).on("change", "#selectAllMenus", function () {
        var isChecked = $(this).is(":checked");
        $(".menu-check").prop("checked", isChecked);
    });

    $(document).on("change", ".parent-menu", function () {
        var parentId = $(this).data("parent");
        var isChecked = $(this).is(":checked");

        $(".child-of-" + parentId).prop("checked", isChecked);
        updateSelectAllState();
    });

    $(document).on("change", ".child-menu", function () {
        var parentId = $(this).data("parent");
        var anyChildChecked = $(".child-of-" + parentId + ":checked").length > 0;

        $("#menu_" + parentId).prop("checked", anyChildChecked);
        updateSelectAllState();
    });

    function updateSelectAllState() {
        var totalMenus = $(".menu-check").length;
        var checkedMenus = $(".menu-check:checked").length;

        $("#selectAllMenus").prop("checked", totalMenus > 0 && totalMenus === checkedMenus);
    }

});