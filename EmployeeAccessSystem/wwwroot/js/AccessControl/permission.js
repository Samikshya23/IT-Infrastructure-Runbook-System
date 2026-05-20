$(document).ready(function () {

    // Select all permission
    $(document).on("change", "#selectAllPermission", function () {

        var isChecked = $(this).is(":checked");

        $(".permission-checkbox").prop("checked", isChecked);

    });

    // Update select all checkbox when single permission changes
    $(document).on("change", ".permission-checkbox", function () {

        updateSelectAllPermission();

    });

    // Change arrow icon when tree section opens
    $(document).on("shown.bs.collapse", ".collapse", function () {

        var targetId = $(this).attr("id");

        $("[data-target='#" + targetId + "']")
            .find("i")
            .removeClass("fa-angle-right")
            .addClass("fa-angle-down");

    });

    // Change arrow icon when tree section closes
    $(document).on("hidden.bs.collapse", ".collapse", function () {

        var targetId = $(this).attr("id");

        $("[data-target='#" + targetId + "']")
            .find("i")
            .removeClass("fa-angle-down")
            .addClass("fa-angle-right");

    });

    // Initial select all state
    updateSelectAllPermission();

});

function updateSelectAllPermission() {

    var total = $(".permission-checkbox").length;
    var checked = $(".permission-checkbox:checked").length;

    $("#selectAllPermission").prop("checked", total > 0 && total === checked);

}