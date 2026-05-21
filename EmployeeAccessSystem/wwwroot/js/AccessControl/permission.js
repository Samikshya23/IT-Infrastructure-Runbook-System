$(document).ready(function () {

    // Initial permission state
    initializePermissionState();

    // Select all permission
    $(document).on("change", "#selectAllPermission", function () {

        var isChecked = $(this).is(":checked");

        if (isChecked) {
            $(".permission-page-checkbox").prop("checked", true);
            $(".permission-action-checkbox").prop("disabled", false).prop("checked", true);
        }
        else {
            $(".permission-checkbox").prop("checked", false);
            $(".permission-action-checkbox").prop("disabled", true);
        }

        updateSelectAllPermission();

    });

    // Parent page permission change
    $(document).on("change", ".permission-page-checkbox", function () {

        var pageMenuId = $(this).data("page-menu-id");
        var isChecked = $(this).is(":checked");

        var actionCheckboxes = $(".permission-action-checkbox[data-page-menu-id='" + pageMenuId + "']");

        if (isChecked) {
            actionCheckboxes.prop("disabled", false);
        }
        else {
            actionCheckboxes.prop("checked", false);
            actionCheckboxes.prop("disabled", true);
        }

        updateSelectAllPermission();

    });

    // Child action permission change
    $(document).on("change", ".permission-action-checkbox", function () {

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

});


// Set child action enabled/disabled according to parent page permission
function initializePermissionState() {

    $(".permission-page-checkbox").each(function () {

        var pageMenuId = $(this).data("page-menu-id");
        var isChecked = $(this).is(":checked");

        var actionCheckboxes = $(".permission-action-checkbox[data-page-menu-id='" + pageMenuId + "']");

        if (isChecked) {
            actionCheckboxes.prop("disabled", false);
        }
        else {
            actionCheckboxes.prop("checked", false);
            actionCheckboxes.prop("disabled", true);
        }

    });

    updateSelectAllPermission();

}


// Update select all checkbox state
function updateSelectAllPermission() {

    var total = $(".permission-checkbox").length;
    var checked = $(".permission-checkbox:checked").length;

    $("#selectAllPermission").prop("checked", total > 0 && total === checked);

}