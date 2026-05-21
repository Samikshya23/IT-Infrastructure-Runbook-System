$(document).ready(function () {

    // Role table
    var table = $("#roleTable").DataTable({
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
                targets: 2,
                orderable: false,
                searchable: false
            }
        ]
    });

    // Search
    $("#roleCustomSearch").on("keyup", function () {
        table.search(this.value).draw();
    });

    // Page length
    $("#roleCustomLength").on("change", function () {
        table.page.len($(this).val()).draw();
    });

    // Add role modal
    $(document).on("click", "#btnAddRole", function () {

        $.ajax({
            url: "/AccessControl/CreateRoleModal",
            type: "GET",
            success: function (response) {

                if (typeof response === "string" && response.indexOf("error|") === 0) {
                    toastr.error(response.replace("error|", ""));
                    return;
                }

                $("#roleModalContent").html(response);
                $("#roleModal").modal("show");
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

                toastr.error("Unable to load role form.");
            }
        });

    });

    // Edit role modal
    $(document).on("click", ".btnEditRole", function () {

        var roleId = $(this).data("role-id");

        $.ajax({
            url: "/AccessControl/EditRoleModal",
            type: "GET",
            data: { roleId: roleId },
            success: function (response) {

                if (typeof response === "string" && response.indexOf("error|") === 0) {
                    toastr.error(response.replace("error|", ""));
                    return;
                }

                $("#roleModalContent").html(response);
                $("#roleModal").modal("show");
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

                toastr.error("Unable to load role edit form.");
            }
        });

    });

    // Delete role modal
    $(document).on("click", ".btnDeleteRole", function () {

        var roleId = $(this).data("role-id");
        var roleName = $(this).data("role-name");

        $("#deleteRoleId").val(roleId);
        $("#deleteRoleName").text(roleName);

        $("#deleteRoleModal").modal("show");

    });

});