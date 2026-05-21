$(document).ready(function () {

    initializePermissionState();

    $(document).on("change", "#selectAllPermission", function () {
        var isChecked = $(this).is(":checked");

        $(".permission-checkbox")
            .prop("checked", isChecked)
            .prop("disabled", false);

        if (!isChecked) {
            $(".permission-checkbox").prop("disabled", false);
        }

        updateSelectAllPermission();
    });

    $(document).on("change", ".permission-checkbox", function () {
        var menuId = $(this).data("menu-id");
        var isChecked = $(this).is(":checked");

        if (isChecked) {
            checkParentMenus($(this));
            enableChildMenus(menuId);
        }
        else {
            uncheckAndDisableChildMenus(menuId);
        }

        updateSelectAllPermission();
    });

    $(document).on("shown.bs.collapse", ".collapse", function () {
        var targetId = $(this).attr("id");

        $("[data-target='#" + targetId + "']")
            .children("i")
            .removeClass("fa-angle-right")
            .addClass("fa-angle-down");
    });

    $(document).on("hidden.bs.collapse", ".collapse", function () {
        var targetId = $(this).attr("id");

        $("[data-target='#" + targetId + "']")
            .children("i")
            .removeClass("fa-angle-down")
            .addClass("fa-angle-right");
    });

});

function initializePermissionState() {

    $(".permission-checkbox:checked").each(function () {
        checkParentMenus($(this));
    });

    $(".permission-checkbox").each(function () {
        var parentMenuId = $(this).data("parent-menu-id");

        if (parentMenuId !== "" &&
            parentMenuId !== null &&
            parentMenuId !== undefined) {

            var parentCheckbox =
                $(".permission-checkbox[data-menu-id='" + parentMenuId + "']");

            if (parentCheckbox.length > 0 &&
                !parentCheckbox.is(":checked")) {

                $(this).prop("checked", false);
                $(this).prop("disabled", true);
            }
        }
    });

    $(".permission-checkbox:checked").each(function () {
        var menuId = $(this).data("menu-id");
        enableChildMenus(menuId);
    });

    updateSelectAllPermission();
}

function checkParentMenus(childCheckbox) {

    var parentMenuId = childCheckbox.data("parent-menu-id");

    if (parentMenuId === "" ||
        parentMenuId === null ||
        parentMenuId === undefined) {
        return;
    }

    var parentCheckbox =
        $(".permission-checkbox[data-menu-id='" + parentMenuId + "']");

    if (parentCheckbox.length > 0) {
        parentCheckbox.prop("disabled", false);
        parentCheckbox.prop("checked", true);

        checkParentMenus(parentCheckbox);
    }
}

function enableChildMenus(parentMenuId) {

    $(".permission-checkbox[data-parent-menu-id='" + parentMenuId + "']")
        .each(function () {
            $(this).prop("disabled", false);

            if ($(this).is(":checked")) {
                enableChildMenus($(this).data("menu-id"));
            }
        });
}

function uncheckAndDisableChildMenus(parentMenuId) {

    $(".permission-checkbox[data-parent-menu-id='" + parentMenuId + "']")
        .each(function () {
            var childMenuId = $(this).data("menu-id");

            $(this).prop("checked", false);
            $(this).prop("disabled", true);

            uncheckAndDisableChildMenus(childMenuId);
        });
}

function updateSelectAllPermission() {
    var total = $(".permission-checkbox").length;
    var checked = $(".permission-checkbox:checked").length;

    $("#selectAllPermission").prop("checked", total > 0 && total === checked);
}