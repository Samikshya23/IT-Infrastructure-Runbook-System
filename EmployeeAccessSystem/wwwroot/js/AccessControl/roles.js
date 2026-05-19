$(document).ready(function () {

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

    $("#roleCustomSearch").on("keyup", function () {
        table.search(this.value).draw();
    });

    $("#roleCustomLength").on("change", function () {
        table.page.len($(this).val()).draw();
    });

    $(document).on("click", "#btnAddRole", function () {
        $.ajax({
            url: "/AccessControl/CreateRoleModal",
            type: "GET",
            success: function (response) {
                $("#roleModalContent").html(response);
                $("#roleModal").modal("show");
            },
            error: function () {
                alert("Unable to load role form.");
            }
        });
    });

    $(document).on("click", ".btnEditRole", function () {
        var roleId = $(this).data("role-id");

        $.ajax({
            url: "/AccessControl/EditRoleModal",
            type: "GET",
            data: {
                roleId: roleId
            },
            success: function (response) {
                $("#roleModalContent").html(response);
                $("#roleModal").modal("show");
            },
            error: function () {
                alert("Unable to load role edit form.");
            }
        });
    });

    $(document).on("click", ".btnDeleteRole", function () {
        var roleId = $(this).data("role-id");
        var roleName = $(this).data("role-name");

        $("#deleteRoleId").val(roleId);
        $("#deleteRoleName").text(roleName);

        $("#deleteRoleModal").modal("show");
    });

});