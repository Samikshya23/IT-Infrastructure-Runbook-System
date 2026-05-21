// Category Setup Add/Edit Modal Script

$(document).ready(function () {

    var valueTypeList = window.categorySetupValueTypes || [];
    var selectedCategoryId = parseInt($("#SelectedCategoryId").val() || "0");
    var editRootIndex = parseInt($("#RootIndex").val() || "-1");
    var isFormChanged = false;

    function getAntiForgeryToken() {
        return $('input[name="__RequestVerificationToken"]').val();
    }

    function generateSetupNodeId() {
        return "node_" + Date.now() + "_" + Math.floor(Math.random() * 100000);
    }

    function encodeHtml(value) {
        if (value == null) {
            return "";
        }

        return $("<div/>").text(value).html();
    }

    function showToastMessage(type, message) {
        if (message == null || message === "") {
            return;
        }

        if (typeof toastr === "undefined") {
            alert(message);
            return;
        }

        toastr.options = {
            closeButton: true,
            progressBar: true,
            newestOnTop: true,
            positionClass: "toast-top-right",
            preventDuplicates: true,
            timeOut: "3000"
        };

        if (type === "success") {
            toastr.success(message, "Success");
        }

        if (type === "error") {
            toastr.error(message, "Message");
        }

        if (type === "warning") {
            toastr.warning(message, "Message");
        }

        if (type === "info") {
            toastr.info(message, "Message");
        }
    }

    if (selectedCategoryId > 0) {
        $("#CategoryId").val(selectedCategoryId);

        if (editRootIndex >= 0) {
            loadRootForEdit(selectedCategoryId, editRootIndex);
        }
        else {
            loadGeneratedForm(selectedCategoryId);
        }
    }

    $(document).off("input change", ".node-value, .field-type");
    $(document).on("input change", ".node-value, .field-type", function () {
        isFormChanged = true;
    });

    $("#CategoryId").off("change").on("change", function () {
        var categoryId = $(this).val();

        $("#nodeContainer").html("");
        $("#generatedSetupArea").hide();

        if (categoryId === "") {
            return;
        }

        loadGeneratedForm(categoryId);
    });

    function loadGeneratedForm(categoryId) {
        $("#generatedSetupArea").show();
        $("#nodeContainer").html('<div class="text-muted text-center py-3">Loading...</div>');

        $.get("/CategorySetup/GetRootLevels", { categoryId: categoryId }, function (response) {

            if (!response.success) {
                showToastMessage("error", response.message);
                $("#nodeContainer").html("");
                return;
            }

            $("#nodeContainer").html("");

            if (response.data == null || response.data.length === 0) {
                $("#nodeContainer").html('<div class="text-muted text-center py-3">No setup structure found.</div>');
                return;
            }

            for (var i = 0; i < response.data.length; i++) {
                addConfiguredRow($("#nodeContainer"), 0, response.data[i], true);
            }

            refreshActionButtons();
            isFormChanged = false;
        })
            .fail(function (xhr) {
                $("#nodeContainer").html("");

                if (xhr.status === 403) {
                    showToastMessage("error", "Access denied. You do not have permission.");
                    return;
                }

                if (xhr.status === 401) {
                    showToastMessage("error", "Please login first.");
                    return;
                }

                showToastMessage("error", "Error while loading setup structure.");
            });
    }

    function loadRootForEdit(categoryId, rootIndex) {
        $("#generatedSetupArea").show();
        $("#nodeContainer").html('<div class="text-muted text-center py-3">Loading...</div>');

        $.get("/CategorySetup/GetRootForEdit", { categoryId: categoryId, rootIndex: rootIndex }, function (response) {

            if (!response.success) {
                showToastMessage("error", response.message);
                $("#nodeContainer").html("");
                return;
            }

            $("#nodeContainer").html("");
            addSavedRow($("#nodeContainer"), 0, response.data, true);
            refreshActionButtons();
            isFormChanged = false;
        })
            .fail(function (xhr) {
                $("#nodeContainer").html("");

                if (xhr.status === 403) {
                    showToastMessage("error", "Access denied. You do not have permission.");
                    return;
                }

                if (xhr.status === 401) {
                    showToastMessage("error", "Please login first.");
                    return;
                }

                showToastMessage("error", "Error while loading setup data.");
            });
    }

    function addConfiguredRow(container, level, configNode, isRootLevel) {
        var margin = level * 30;
        var setupNodeId = generateSetupNodeId();
        var html = "";

        html += '<div class="node-row mb-2"';
        html += ' data-setup-node-id="' + setupNodeId + '"';
        html += ' data-level="' + level + '"';
        html += ' data-config-id="' + configNode.id + '"';
        html += ' data-heading="' + encodeHtml(configNode.heading) + '"';
        html += ' data-label-name="' + encodeHtml(configNode.name) + '"';
        html += ' data-input-type="' + encodeHtml(configNode.inputType) + '"';
        html += ' data-root="' + isRootLevel + '"';
        html += ' style="margin-left:' + margin + 'px;">';

        html += '<div class="row align-items-center">';

        html += '<div class="col-md-3">';
        html += '<label class="mb-0 font-weight-bold">' + encodeHtml(configNode.name) + '</label>';
        html += '</div>';

        html += '<div class="col-md-5">';
        html += '<input type="text" maxlength="100" class="form-control form-control-sm node-value" placeholder="Enter value">';
        html += '</div>';

        if (isRootLevel === true) {
            html += getFieldTypeDropdown("", "");
        }
        else {
            html += '<div class="col-md-2"></div>';
        }

        html += getActionButtons();

        html += '</div>';

        html += '<div class="child-area mt-2"></div>';
        html += '</div>';

        container.append(html);

        var currentRow = container.children(".node-row").last();

        loadDefaultChildren(currentRow);
    }

    function addSavedRow(container, level, savedNode, isRootLevel) {
        var margin = level * 30;
        var setupNodeId = savedNode.id || savedNode.Id;

        if (setupNodeId == null || setupNodeId === "") {
            setupNodeId = generateSetupNodeId();
        }

        var fieldType = savedNode.fieldType || savedNode.FieldType || "";
        var fieldTypeId = savedNode.fieldTypeId || savedNode.FieldTypeId || "";
        var valueType = savedNode.valueType || savedNode.ValueType || "";
        var configurationNodeId = savedNode.configurationNodeId || savedNode.ConfigurationNodeId || "";
        var heading = savedNode.heading || savedNode.Heading || "";
        var label = savedNode.label || savedNode.Label || "";
        var value = savedNode.value || savedNode.Value || "";

        var html = "";

        html += '<div class="node-row mb-2"';
        html += ' data-setup-node-id="' + setupNodeId + '"';
        html += ' data-level="' + level + '"';
        html += ' data-config-id="' + configurationNodeId + '"';
        html += ' data-heading="' + encodeHtml(heading) + '"';
        html += ' data-label-name="' + encodeHtml(label) + '"';
        html += ' data-input-type="' + encodeHtml(valueType) + '"';
        html += ' data-root="' + isRootLevel + '"';
        html += ' style="margin-left:' + margin + 'px;">';

        html += '<div class="row align-items-center">';

        html += '<div class="col-md-3">';
        html += '<label class="mb-0 font-weight-bold">' + encodeHtml(label) + '</label>';
        html += '</div>';

        html += '<div class="col-md-5">';
        html += '<input type="text" maxlength="100" class="form-control form-control-sm node-value" value="' + encodeHtml(value) + '" placeholder="Enter value">';
        html += '</div>';

        if (isRootLevel === true) {
            html += getFieldTypeDropdown(fieldType, fieldTypeId);
        }
        else {
            html += '<div class="col-md-2"></div>';
        }

        html += getActionButtons();

        html += '</div>';

        html += '<div class="child-area mt-2"></div>';
        html += '</div>';

        container.append(html);

        var currentRow = container.children(".node-row").last();
        var children = savedNode.children || savedNode.Children || [];

        if (children != null && children.length > 0) {
            for (var i = 0; i < children.length; i++) {
                addSavedRow(currentRow.children(".child-area"), level + 1, children[i], false);
            }
        }
    }

    function getFieldTypeDropdown(selectedValue, selectedId) {
        var html = "";

        html += '<div class="col-md-2">';
        html += '<select class="form-control form-control-sm field-type">';
        html += '<option value="">Select</option>';

        if (valueTypeList != null) {
            for (var i = 0; i < valueTypeList.length; i++) {
                var item = valueTypeList[i];

                var id =
                    item.DropdownItemId ||
                    item.dropdownItemId ||
                    item.Id ||
                    item.id;

                var text =
                    item.ItemName ||
                    item.itemName ||
                    item.Name ||
                    item.name;

                html += getFieldTypeOption(id, text, selectedValue, selectedId);
            }
        }

        html += '</select>';
        html += '</div>';

        return html;
    }

    function getFieldTypeOption(id, text, selectedValue, selectedId) {
        var selected = "";

        if (id == selectedId || text === selectedValue) {
            selected = " selected";
        }

        return '<option value="' + id + '" data-text="' + encodeHtml(text) + '"' + selected + '>' + encodeHtml(text) + '</option>';
    }

    function getActionButtons() {
        var html = "";

        html += '<div class="col-md-2 text-right">';

        html += '<button type="button" class="btn btn-outline-primary btn-sm btn-add-same mr-1" title="Add">';
        html += '<i class="fas fa-plus"></i>';
        html += '</button>';

        html += '<button type="button" class="btn btn-outline-danger btn-sm btn-remove-row" title="Remove">';
        html += '<i class="fas fa-minus"></i>';
        html += '</button>';

        html += '</div>';

        return html;
    }

    function loadDefaultChildren(parentRow) {
        var categoryId = $("#CategoryId").val();
        var configId = parentRow.attr("data-config-id");
        var childArea = parentRow.children(".child-area");
        var level = parseInt(parentRow.attr("data-level"));

        $.get("/CategorySetup/GetChildLevels", { categoryId: categoryId, parentConfigurationNodeId: configId }, function (response) {
            if (!response.success) {
                return;
            }

            if (response.data == null || response.data.length === 0) {
                refreshActionButtons();
                return;
            }

            for (var i = 0; i < response.data.length; i++) {
                addConfiguredRow(childArea, level + 1, response.data[i], false);
            }

            refreshActionButtons();
        });
    }

    $(document).off("click", ".btn-add-same");
    $(document).on("click", ".btn-add-same", function () {
        var currentRow = $(this).closest(".node-row");
        var currentValue = currentRow.find("> .row .node-value").val();

        if ($.trim(currentValue) === "") {
            showToastMessage("error", "Please enter value first.");
            return;
        }

        if (currentRow.find("> .row .field-type").length > 0) {
            var currentType = currentRow.find("> .row .field-type").val();

            if (currentType === "") {
                showToastMessage("error", "Please select value type first.");
                return;
            }
        }

        var parentContainer = currentRow.parent();

        var configNode = {
            id: currentRow.attr("data-config-id"),
            heading: currentRow.attr("data-heading"),
            name: currentRow.attr("data-label-name"),
            inputType: currentRow.attr("data-input-type")
        };

        var level = parseInt(currentRow.attr("data-level"));
        var isRootLevel = currentRow.attr("data-root") === "true";

        // Original concept: plus button adds same-level row
        addConfiguredRow(parentContainer, level, configNode, isRootLevel);

        isFormChanged = true;

        refreshActionButtons();
    });

    $(document).off("click", ".btn-remove-row");
    $(document).on("click", ".btn-remove-row", function () {
        var currentRow = $(this).closest(".node-row");

        var childCount = currentRow.children(".child-area").children(".node-row").length;

        if (childCount > 0) {
            showToastMessage("error", "Please remove child items first.");
            return;
        }

        var isRootLevel = currentRow.attr("data-root") === "true";

        if (isRootLevel === true) {
            showToastMessage("error", "Parent row cannot be removed.");
            return;
        }

        var siblingCount = currentRow.parent().children(".node-row").length;

        if (siblingCount <= 1) {
            showToastMessage("error", "At least one child item is required.");
            return;
        }

        currentRow.remove();

        isFormChanged = true;

        refreshActionButtons();
    });

    function refreshActionButtons() {
        $("#nodeContainer").find(".node-row").each(function () {
            var row = $(this);
            var inputType = row.attr("data-input-type");
            var isRootLevel = row.attr("data-root") === "true";

            row.find("> .row .btn-add-same").hide();
            row.find("> .row .btn-remove-row").hide();

            if (isRootLevel === true) {
                var rootRows = row.parent().children(".node-row");
                var isLastRoot = row.is(rootRows.last());

                if (isLastRoot) {
                    row.find("> .row .btn-add-same").show();
                }

                // Parent delete icon hidden
                row.find("> .row .btn-remove-row").hide();
                return;
            }

            if (inputType === "Single") {
                row.find("> .row .btn-remove-row").show();
                return;
            }

            if (inputType === "Multiple") {
                var siblingRows = row.parent().children(".node-row");
                var isLastRow = row.is(siblingRows.last());

                if (isLastRow) {
                    row.find("> .row .btn-add-same").show();
                }

                row.find("> .row .btn-remove-row").show();
            }
        });
    }

    function validateRows(container) {
        var valid = true;

        container.find(".node-row").each(function () {
            var row = $(this);
            var nodeValue = row.find("> .row .node-value").val();

            if ($.trim(nodeValue) === "") {
                showToastMessage("error", "Please fill all required values.");
                valid = false;
                return false;
            }

            if (row.find("> .row .field-type").length > 0) {
                var fieldType = row.find("> .row .field-type").val();

                if (fieldType === "") {
                    showToastMessage("error", "Please select value type.");
                    valid = false;
                    return false;
                }
            }

            var isRootLevel = row.attr("data-root") === "true";

            if (isRootLevel === true) {
                var rootChildCount = row.children(".child-area").children(".node-row").length;

                if (rootChildCount === 0) {
                    showToastMessage("error", "At least one child item is required.");
                    valid = false;
                    return false;
                }
            }
        });

        return valid;
    }

    function collectRows(container) {
        var data = [];

        container.children(".node-row").each(function () {
            var row = $(this);
            var nodeValue = row.find("> .row .node-value").val();
            var configurationNodeId = row.attr("data-config-id");
            var fieldTypeId = null;
            var fieldTypeText = null;

            if (row.find("> .row .field-type").length > 0) {
                fieldTypeId = row.find("> .row .field-type").val();
                fieldTypeText = row.find("> .row .field-type option:selected").text();
            }

            data.push({
                id: row.attr("data-setup-node-id"),
                heading: row.attr("data-heading"),
                label: row.attr("data-label-name"),
                valueType: row.attr("data-input-type"),
                value: $.trim(nodeValue),
                fieldTypeId: fieldTypeId,
                fieldType: fieldTypeText,
                configurationNodeId: parseInt(configurationNodeId),
                children: collectRows(row.children(".child-area"))
            });
        });

        return data;
    }

    $("#btnSave").off("click").on("click", function () {
        var categoryId = $("#CategoryId").val();

        if (categoryId === "") {
            showToastMessage("error", "Please select a main item.");
            return;
        }

        if ($("#nodeContainer").children(".node-row").length === 0) {
            showToastMessage("error", "Please add setup values.");
            return;
        }

        if (!validateRows($("#nodeContainer"))) {
            return;
        }

        var nodes = collectRows($("#nodeContainer"));

        $.ajax({
            url: "/CategorySetup/SaveData",
            type: "POST",
            contentType: "application/json",
            headers: {
                "RequestVerificationToken": getAntiForgeryToken()
            },
            data: JSON.stringify({
                categoryId: parseInt(categoryId),
                rootIndex: editRootIndex,
                nodes: nodes
            }),
            success: function (response) {
                if (response.success) {
                    $("#setupConfigurationModal").modal("hide");

                    window.location.href =
                        "/CategorySetup?selectedCategoryId="
                        + categoryId
                        + "&successMessage="
                        + encodeURIComponent(response.message);
                }
                else {
                    showToastMessage("error", response.message);
                }
            },
            error: function (xhr) {
                if (xhr.status === 400) {
                    showToastMessage("error", "Security token missing or expired. Please reload and try again.");
                    return;
                }

                if (xhr.status === 403) {
                    showToastMessage("error", "Access denied. You do not have permission.");
                    return;
                }

                showToastMessage("error", "Failed to save setup configuration.");
            }
        });
    });

    $("#btnFooterClose").off("click").on("click", function () {
        if (isFormChanged === false) {
            $("#setupConfigurationModal").modal("hide");
            return;
        }

        $("#confirmCloseModal").modal("show");
    });

    $("#btnContinueEditing").off("click").on("click", function () {
        $("#confirmCloseModal").modal("hide");
    });

    $("#btnConfirmClose").off("click").on("click", function () {
        $("#confirmCloseModal").modal("hide");
        $("#setupConfigurationModal").modal("hide");
    });
});