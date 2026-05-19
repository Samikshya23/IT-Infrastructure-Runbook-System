$(document).ready(function () {

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

    $("#accessCustomSearch").on("keyup", function () {
        table.search(this.value).draw();
    });

    $("#accessCustomLength").on("change", function () {
        table.page.len($(this).val()).draw();
    });

    $(document).on("click", ".btnMenuAccess", function () {
        var accountId = $(this).data("account-id");

        $.ajax({
            url: "/AccessControl/AccessModal",
            type: "GET",
            data: {
                accountId: accountId
            },
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

        updateSelectAllState();
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

        if (totalMenus > 0 && totalMenus === checkedMenus) {
            $("#selectAllMenus").prop("checked", true);
        } else {
            $("#selectAllMenus").prop("checked", false);
        }
    }

});