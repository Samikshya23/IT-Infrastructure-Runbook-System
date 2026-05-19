$(document).ready(function () {

    var table = $("#menuTable").DataTable({
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
                targets: 6,
                orderable: false,
                searchable: false
            }
        ]
    });

    $("#menuCustomSearch").on("keyup", function () {
        table.search(this.value).draw();
    });

    $("#menuCustomLength").on("change", function () {
        table.page.len($(this).val()).draw();
    });

    $(document).on("click", "#btnAddMenu", function () {
        $.ajax({
            url: "/AccessControl/CreateMenuModal",
            type: "GET",
            success: function (response) {
                $("#menuModalContent").html(response);
                $("#menuModal").modal("show");
            },
            error: function () {
                alert("Unable to load menu form.");
            }
        });
    });

    $(document).on("click", ".btnEditMenu", function () {
        var menuId = $(this).data("menu-id");

        $.ajax({
            url: "/AccessControl/EditMenuModal",
            type: "GET",
            data: {
                menuId: menuId
            },
            success: function (response) {
                $("#menuModalContent").html(response);
                $("#menuModal").modal("show");
            },
            error: function () {
                alert("Unable to load menu edit form.");
            }
        });
    });

    $(document).on("click", ".btnChangeMenuStatus", function () {
        var menuId = $(this).data("menu-id");
        var menuName = $(this).data("menu-name");

        $("#statusMenuId").val(menuId);
        $("#statusMenuName").text(menuName);

        $("#menuStatusModal").modal("show");
    });

});