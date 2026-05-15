$(document).ready(function () {

    loadInitialData();

    $("#ProductId").off("change").on("change", function () {
        var productId = $(this).val();

        if (productId === "") {
            $("#btnAddRoot").hide();
            $("#nodeContainer").html("");
            return;
        }

        $.get("/ProductConfiguration/Add", { productId: productId }, function (data) {
            $("#configurationModalContent").html(data);
        });
    });

    $("#btnAddRoot").off("click").on("click", function () {
        if ($("#ProductId").val() === "") {
            showToastMessage("error", "Please select product first.");
            return;
        }

        if (!canAddRootRow()) {
            showToastMessage("error", "Please add at least one child level before adding new root level.");
            return;
        }

        addRow($("#nodeContainer"), 0);
        refreshHeading();
        refreshDeleteButtons();
    });

    $("#btnSave").off("click").on("click", function () {
        saveData();
    });

    $("#btnFooterClose").off("click").on("click", function (e) {
        e.preventDefault();
        $("#confirmCloseModal").modal("show");
    });

    $("#btnContinueEditing").off("click").on("click", function () {
        $("#confirmCloseModal").modal("hide");
    });

    $("#btnConfirmClose").off("click").on("click", function () {
        $("#confirmCloseModal").modal("hide");

        setTimeout(function () {
            $("#configurationModal").modal("hide");
        }, 200);
    });

    function loadInitialData() {
        if (selectedProductId <= 0) {
            return;
        }

        $("#ProductId").val(selectedProductId);
        $("#btnAddRoot").css("display", "inline-block");

        var structureData = [];

        if (existingData != null && existingData.structure != null) {
            structureData = existingData.structure;
        }
        else if (existingData != null && Array.isArray(existingData)) {
            structureData = existingData;
        }

        if (structureData.length === 0) {
            return;
        }

        for (var i = 0; i < structureData.length; i++) {
            addExistingRow($("#nodeContainer"), structureData[i], 0);
        }

        refreshHeading();
        refreshDeleteButtons();
    }

    function addExistingRow(container, node, level) {
        addRow(container, level);

        var current = container.children(".node-row").last();
        var label = node.label || node.nodeName || "";
        var valueType = node.valueType || node.inputType || "";
        var heading = node.heading || "";

        current.find("> .row .node-name").val(label);
        current.find("> .row .input-type").val(valueType);

        if (heading !== "") {
            current.find("> .row .heading-label").text(heading);
        }

        if (node.children == null || node.children.length === 0) {
            return;
        }

        for (var i = 0; i < node.children.length; i++) {
            addExistingRow(current.children(".child-area"), node.children[i], level + 1);
        }
    }

    function addRow(container, level) {
        var marginValue = level * 30;
        var html = "";

        html += '<div class="node-row mb-2" data-level="' + level + '" style="margin-left:' + marginValue + 'px;">';
        html += '<div class="row align-items-center">';

        html += '<div class="col-md-1 text-center">';
        html += '<span class="heading-label text-dark font-weight-bold"></span>';
        html += '</div>';

        html += '<div class="col-md-5">';
        html += '<input type="text" maxlength="50" class="form-control form-control-sm node-name" placeholder="Type label/level name">';
        html += '</div>';

        html += '<div class="col-md-4">';
        html += '<select class="form-control form-control-sm input-type"></select>';
        html += '</div>';

        html += '<div class="col-md-2 text-right">';
        html += '<button type="button" class="btn btn-outline-primary btn-sm px-2 py-0 btn-plus mr-1" title="Add Child">';
        html += '<i class="fas fa-plus"></i>';
        html += '</button>';

        html += '<button type="button" class="btn btn-outline-danger btn-sm px-2 py-0 btn-minus" title="Remove">';
        html += '<i class="fas fa-minus"></i>';
        html += '</button>';
        html += '</div>';

        html += '</div>';
        html += '<div class="child-area mt-2"></div>';
        html += '</div>';

        container.append(html);

        var current = container.children(".node-row").last();

        setInputTypeDropdown(current, level);
        bindRowEvents(current, level);
    }

    function bindRowEvents(current, level) {
        current.find(".btn-plus").off("click").on("click", function () {
            var nodeName = current.find("> .row .node-name").val();

            if ($.trim(nodeName) === "") {
                showToastMessage("error", "Please type label/level name first.");
                return;
            }

            if (!canAddChildRow(current.children(".child-area"))) {
                showToastMessage("error", "Please complete existing child level first.");
                return;
            }

            addRow(current.children(".child-area"), level + 1);
            refreshHeading();
            refreshDeleteButtons();
        });

        current.find(".btn-minus").off("click").on("click", function () {
            current.remove();

            refreshHeading();
            refreshDeleteButtons();
        });
    }

    function setInputTypeDropdown(row, level) {
        var dropdown = row.find("> .row .input-type");

        dropdown.empty();

        if (level === 0) {
            dropdown.append('<option value="Text">Text</option>');
            dropdown.append('<option value="Date">Date</option>');
        }
        else {
            dropdown.append('<option value="Single">Single</option>');
            dropdown.append('<option value="Multiple">Multiple</option>');
        }
    }

    function canAddChildRow(container) {
        var valid = true;

        container.children(".node-row").each(function () {
            var row = $(this);
            var nodeName = row.find("> .row .node-name").val();

            if ($.trim(nodeName) === "") {
                valid = false;
                return false;
            }
        });

        return valid;
    }

    function canAddRootRow() {
        var valid = true;

        $("#nodeContainer").children(".node-row").each(function () {
            var row = $(this);
            var nodeName = row.find("> .row .node-name").val();
            var childCount = row.children(".child-area").children(".node-row").length;

            if ($.trim(nodeName) === "") {
                valid = false;
                return false;
            }

            if (childCount === 0) {
                valid = false;
                return false;
            }

            if (!canAddChildRow(row.children(".child-area"))) {
                valid = false;
                return false;
            }
        });

        return valid;
    }

    function validateStructure() {
        var valid = true;

        $("#nodeContainer").find(".node-row").each(function () {
            var row = $(this);
            var nodeName = row.find("> .row .node-name").val();

            if ($.trim(nodeName) === "") {
                valid = false;
                return false;
            }
        });

        return valid;
    }

    function refreshHeading() {
        $("#nodeContainer").children(".node-row").each(function (i) {
            setHeading($(this), (i + 1).toString());
        });
    }

    function setHeading(row, number) {
        row.find("> .row .heading-label").text("H" + number);

        row.children(".child-area").children(".node-row").each(function (i) {
            setHeading($(this), number + "." + (i + 1));
        });
    }

    function refreshDeleteButtons() {
        $("#nodeContainer").find(".node-row").each(function () {
            var row = $(this);
            var childCount = row.children(".child-area").children(".node-row").length;

            if (childCount > 0) {
                row.find("> .row .btn-minus").hide();
            }
            else {
                row.find("> .row .btn-minus").show();
            }
        });
    }

    function saveData() {
        var productId = $("#ProductId").val();

        if (productId === "") {
            showToastMessage("error", "Please select product.");
            return;
        }

        if ($("#nodeContainer").children(".node-row").length === 0) {
            showToastMessage("error", "Please click plus and add structure before saving.");
            return;
        }

        if (!validateStructure()) {
            showToastMessage("error", "Please fill all label/level names.");
            return;
        }

        var nodes = collectRows($("#nodeContainer"));

        $.ajax({
            url: "/ProductConfiguration/SaveStructure",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                productId: parseInt(productId),
                nodes: nodes
            }),
            success: function (response) {
                if (response.success) {
                    window.location.href = "/ProductConfiguration?selectedProductId=" + productId + "&successMessage=" + encodeURIComponent(response.message);
                }
                else {
                    showToastMessage("error", response.message);
                }
            },
            error: function () {
                showToastMessage("error", "Error while saving structure.");
            }
        });
    }

    function collectRows(container) {
        var data = [];

        container.children(".node-row").each(function () {
            var row = $(this);
            var heading = row.find("> .row .heading-label").text();
            var nodeName = row.find("> .row .node-name").val();
            var inputType = row.find("> .row .input-type").val();

            data.push({
                heading: $.trim(heading),
                nodeName: $.trim(nodeName),
                inputType: $.trim(inputType),
                children: collectRows(row.children(".child-area"))
            });
        });

        return data;
    }
});