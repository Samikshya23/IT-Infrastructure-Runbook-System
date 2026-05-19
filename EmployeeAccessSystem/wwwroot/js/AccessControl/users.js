$(document).ready(function () {

    var table = $("#accessControlTable").DataTable({
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

    $(document).on("click", ".btnDetailsUser", function () {
        var accountId = $(this).data("account-id");

        $.ajax({
            url: "/AccessControl/DetailsModal",
            type: "GET",
            data: {
                accountId: accountId
            },
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
            data: {
                accountId: accountId
            },
            success: function (response) {
                $("#editUserModalContent").html(response);
                $("#editUserModal").modal("show");
            },
            error: function () {
                alert("Unable to load user edit form.");
            }
        });
    });

    $(document).on("click", ".btnRemoveUser", function () {
        alert("User delete/deactivate will be added later.");
    });

});