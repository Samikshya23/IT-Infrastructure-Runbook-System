// Product Setup Configuration Add/Edit Modal Script

$(document).ready(function () {
    // Global variables
    var valueTypeList = window.productSetupValueTypes || [];
    var selectedProductId = parseInt($("#SelectedProductId").val() || "0");
    var editRootIndex = parseInt($("#RootIndex").val() || "-1");
    var isFormChanged = false;

    // Get anti-forgery token for secure AJAX POST request
    function getAntiForgeryToken() {
        return $('input[name="__RequestVerificationToken"]').val();
    }

    // Generate unique setup node id
    function generateSetupNodeId() {
        return "node_" + Date.now() + "_" + Math.floor(Math.random() * 100000);
    }

    // Encode html value
    function encodeHtml(value) {
        if (value == null) {
            return "";
        }

        return $("<div/>").text(value).html();
    }

    // Show toastr message
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

    // Initial load
    if (selectedProductId > 0) {
        $("#ProductId").val(selectedProductId);

        if (editRootIndex >= 0) {
            loadRootForEdit(selectedProductId, editRootIndex);
        }
        else {
            loadGeneratedForm(selectedProductId);
        }
    }

    // Track form changes
    $(document).off("input change", ".node-value, .field-type");
    $(document).on("input change", ".node-value, .field-type", function () {
        isFormChanged = true;
    });

    // Product dropdown change
    $("#ProductId").off("change").on("change", function () {
        var productId = $(this).val();

        $("#nodeContainer").html("");
        $("#generatedSetupArea").hide();

        if (productId === "") {
            return;
        }

        loadGeneratedForm(productId);
    });

    // Load generated form from configuration
    function loadGeneratedForm(productId) {
        $("#generatedSetupArea").show();
        $("#nodeContainer").html('<div class="text-muted text-center py-3">Loading...</div>');

        $.get("/ProductSetupConfiguration/GetRootLevels", { productId: productId }, function (response) {
            if (!response.success) {
                showToastMessage("error", response.message);
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
        });
    }

    // Load selected root for edit
    function loadRootForEdit(productId, rootIndex) {
        $("#generatedSetupArea").show();
        $("#nodeContainer").html('<div class="text-muted text-center py-3">Loading...</div>');

        $.get("/ProductSetupConfiguration/GetRootForEdit", { productId: productId, rootIndex: rootIndex }, function (response) {
            if (!response.success) {
                showToastMessage("error", response.message);
                return;
            }

            $("#nodeContainer").html("");
            addSavedRow($("#nodeContainer"), 0, response.data, true);
            refreshActionButtons();
            isFormChanged = false;
        });
    }

    // Add row from product configuration
    function addConfiguredRow(container, level, configNode, isRootLevel) {
        var margin = level * 30;
        var setupNodeId = generateSetupNodeId();
        var html = "";

        html += '<div class="node-row mb-2" data-setup-node-id="' + setupNodeId + '" data-level="' + level + '" data-config-id="' + configNode.id + '" data-heading="' + encodeHtml(configNode.heading) + '" data-label-name="' + encodeHtml(configNode.name) + '" data-input-type="' + encodeHtml(configNode.inputType) + '" data-root="' + isRootLevel + '" style="margin-left:' + margin + 'px;">';
        html += '<div class="row align-items-center">';
        html += '<div class="col-md-3"><label class="mb-0 font-weight-bold">' + encodeHtml(configNode.name) + '</label></div>';
        html += '<div class="col-md-5"><input type="text" maxlength="100" class="form-control form-control-sm node-value" placeholder="Enter value"></div>';

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

    // Add row from saved setup json
    function addSavedRow(container, level, savedNode, isRootLevel) {
        var margin = level * 30;
        var setupNodeId = savedNode.id || savedNode.Id;

        if (setupNodeId == null || setupNodeId === "") {
            setupNodeId = generateSetupNodeId();
        }

        var fieldType = savedNode.fieldType || "";
        var fieldTypeId = savedNode.fieldTypeId || "";
        var valueType = savedNode.valueType || "";
        var html = "";

        html += '<div class="node-row mb-2" data-setup-node-id="' + setupNodeId + '" data-level="' + level + '" data-config-id="' + savedNode.configurationNodeId + '" data-heading="' + encodeHtml(savedNode.heading) + '" data-label-name="' + encodeHtml(savedNode.label) + '" data-input-type="' + encodeHtml(valueType) + '" data-root="' + isRootLevel + '" style="margin-left:' + margin + 'px;">';
        html += '<div class="row align-items-center">';
        html += '<div class="col-md-3"><label class="mb-0 font-weight-bold">' + encodeHtml(savedNode.label) + '</label></div>';
        html += '<div class="col-md-5"><input type="text" maxlength="100" class="form-control form-control-sm node-value" value="' + encodeHtml(savedNode.value) + '" placeholder="Enter value"></div>';

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

        if (savedNode.children != null && savedNode.children.length > 0) {
            for (var i = 0; i < savedNode.children.length; i++) {
                addSavedRow(currentRow.children(".child-area"), level + 1, savedNode.children[i], false);
            }
        }
    }

    // Generate field type dropdown
    function getFieldTypeDropdown(selectedValue, selectedId) {
        var html = "";

        html += '<div class="col-md-2">';
        html += '<select class="form-control form-control-sm field-type">';
        html += '<option value="">Select</option>';

        if (valueTypeList != null) {
            for (var i = 0; i < valueTypeList.length; i++) {
                var item = valueTypeList[i];
                var id = item.DropdownItemId || item.dropdownItemId;
                var text = item.ItemName || item.itemName;

                html += getFieldTypeOption(id, text, selectedValue, selectedId);
            }
        }

        html += '</select>';
        html += '</div>';

        return html;
    }

    // Generate field type option
    function getFieldTypeOption(id, text, selectedValue, selectedId) {
        var selected = "";

        if (id == selectedId || text === selectedValue) {
            selected = " selected";
        }

        return '<option value="' + id + '" data-text="' + encodeHtml(text) + '"' + selected + '>' + encodeHtml(text) + '</option>';
    }

    // Generate add/remove buttons
    function getActionButtons() {
        var html = "";

        html += '<div class="col-md-2 text-right">';
        html += '<button type="button" class="btn btn-outline-primary btn-sm btn-add-same mr-1"><i class="fas fa-plus"></i></button>';
        html += '<button type="button" class="btn btn-outline-danger btn-sm btn-remove-row"><i class="fas fa-minus"></i></button>';
        html += '</div>';

        return html;
    }

    // Load default child levels
    function loadDefaultChildren(parentRow) {
        var productId = $("#ProductId").val();
        var configId = parentRow.attr("data-config-id");
        var childArea = parentRow.children(".child-area");
        var level = parseInt(parentRow.attr("data-level"));

        $.get("/ProductSetupConfiguration/GetChildLevels", { productId: productId, parentConfigurationNodeId: configId }, function (response) {
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

    // Add same level row
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

        addConfiguredRow(parentContainer, level, configNode, isRootLevel);

        isFormChanged = true;

        refreshActionButtons();
    });

    // Remove selected row
    // Parent row cannot be removed until all child rows are removed first
    $(document).off("click", ".btn-remove-row");
    $(document).on("click", ".btn-remove-row", function () {

        var currentRow = $(this).closest(".node-row");

        // Check if current row has direct child rows
        var childCount = currentRow.children(".child-area").children(".node-row").length;

        if (childCount > 0) {
            showToastMessage("error", "Please remove child items first.");
            return;
        }

        currentRow.remove();

        isFormChanged = true;

        if ($("#nodeContainer").children(".node-row").length === 0 && editRootIndex < 0) {
            $("#generatedSetupArea").hide();
            $("#nodeContainer").html("");
        }

        refreshActionButtons();
    });

    // Refresh add/remove buttons
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

                row.find("> .row .btn-remove-row").show();
                return;
            }

            if (inputType === "Single") {
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

    // Validate rows before save
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
        });

        return valid;
    }

    // Collect recursive rows
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

    // Delete root when edit row is removed
    function deleteCurrentRoot(productId) {
        $.ajax({
            url: "/ProductSetupConfiguration/DeleteRoot",
            type: "POST",
            headers: {
                "RequestVerificationToken": getAntiForgeryToken()
            },
            data: {
                productId: parseInt(productId),
                rootIndex: editRootIndex
            },
            success: function (response) {
                if (response.success) {
                    $("#setupConfigurationModal").modal("hide");
                    window.location.href = "/ProductSetupConfiguration?selectedProductId=" + productId + "&successMessage=" + encodeURIComponent(response.message);
                }
                else {
                    showToastMessage("error", response.message);
                }
            },
            error: function () {
                showToastMessage("error", "Delete failed.");
            }
        });
    }

    // Save setup configuration
    $("#btnSave").off("click").on("click", function () {
        var productId = $("#ProductId").val();

        if (productId === "") {
            showToastMessage("error", "Please select a main item.");
            return;
        }

        if ($("#nodeContainer").children(".node-row").length === 0) {
            if (editRootIndex >= 0) {
                deleteCurrentRoot(productId);
                return;
            }

            showToastMessage("error", "Please add setup values.");
            return;
        }

        if (!validateRows($("#nodeContainer"))) {
            return;
        }

        var nodes = collectRows($("#nodeContainer"));

        $.ajax({
            url: "/ProductSetupConfiguration/SaveData",
            type: "POST",
            contentType: "application/json",
            headers: {
                "RequestVerificationToken": getAntiForgeryToken()
            },
            data: JSON.stringify({
                productId: parseInt(productId),
                rootIndex: editRootIndex,
                nodes: nodes
            }),
            success: function (response) {
                if (response.success) {
                    $("#setupConfigurationModal").modal("hide");
                    window.location.href = "/ProductSetupConfiguration?selectedProductId=" + productId + "&successMessage=" + encodeURIComponent(response.message);
                }
                else {
                    showToastMessage("error", response.message);
                }
            },
            error: function () {
                showToastMessage("error", "Failed to save setup configuration.");
            }
        });
    });

    // Close modal
    $("#btnFooterClose").off("click").on("click", function () {
        if (isFormChanged === false) {
            $("#setupConfigurationModal").modal("hide");
            return;
        }

        $("#confirmCloseModal").modal("show");
    });

    // Continue editing
    $("#btnContinueEditing").off("click").on("click", function () {
        $("#confirmCloseModal").modal("hide");
    });

    // Confirm close modal
    $("#btnConfirmClose").off("click").on("click", function () {
        $("#confirmCloseModal").modal("hide");
        $("#setupConfigurationModal").modal("hide");
    });
});