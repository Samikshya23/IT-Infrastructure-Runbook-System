$(document).ajaxError(function (event, xhr) {

    // If user is not logged in
    if (xhr.status === 401) {

        toastr.error("Please login first.");

        $(".modal").modal("hide");
        $(".modal-backdrop").remove();
        $("body").removeClass("modal-open");
        $("body").css("padding-right", "");

        return;
    }

    // If user does not have permission
    if (xhr.status === 403) {

        toastr.error("Access denied. You do not have permission.");

        $(".modal").modal("hide");
        $(".modal-body").html("");
        $(".modal-backdrop").remove();
        $("body").removeClass("modal-open");
        $("body").css("padding-right", "");

        return;
    }
});