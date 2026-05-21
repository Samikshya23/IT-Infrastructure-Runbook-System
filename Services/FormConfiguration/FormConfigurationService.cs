using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class FormConfigurationService : IFormConfigurationService
    {
        private readonly IFormConfigurationRepository _repository;

        public FormConfigurationService(IFormConfigurationRepository repository)
        {
            _repository = repository;
        }

        #region Public Methods

        // Load configuration list for index page
        public async Task<List<FormConfigurationIndexItem>> GetIndexAsync()
        {
            List<FormConfigurationIndexItem> result = new List<FormConfigurationIndexItem>();

            IEnumerable<FormConfiguration> data = await _repository.GetAllAsync();

            foreach (FormConfiguration item in data)
            {
                if (item == null)
                {
                    continue;
                }

                // Show only configured records
                if (string.IsNullOrWhiteSpace(item.ConfigurationJson))
                {
                    continue;
                }

                FormConfigurationIndexItem indexItem = new FormConfigurationIndexItem();

                indexItem.CategoryId = item.CategoryId;
                indexItem.Name = item.Name;

                // Convert saved JSON into hierarchy tree
                indexItem.Nodes = ConvertJsonToTree(item.ConfigurationJson);

                result.Add(indexItem);
            }

            return result;
        }

        // Load saved hierarchy structure by category
        public async Task<List<FormConfiguration>> GetTreeByCategoryIdAsync(int categoryId)
        {
            if (categoryId <= 0)
            {
                return new List<FormConfiguration>();
            }

            FormConfiguration data = await _repository.GetJsonByCategoryIdAsync(categoryId);

            if (data == null ||
                string.IsNullOrWhiteSpace(data.ConfigurationJson))
            {
                return new List<FormConfiguration>();
            }

            return ConvertJsonToTree(data.ConfigurationJson);
        }

        // Save or update configuration structure
        public async Task<(bool Success, string Message)> SaveStructureAsync(FormConfigurationSaveRequest request, string createdBy)
        {
            if (request == null)
            {
                return (false, "Invalid request.");
            }

            if (request.CategoryId <= 0)
            {
                return (false, "Please select category.");
            }

            if (request.Nodes == null || request.Nodes.Count == 0)
            {
                return (false, "Please add configuration structure.");
            }

            string validationMessage = ValidateNodes(request.Nodes, true);

            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return (false, validationMessage);
            }

            FormConfigurationJsonModel jsonModel = new FormConfigurationJsonModel();

            string name = "";

            IEnumerable<FormConfiguration> allRecords = await _repository.GetAllAsync();

            foreach (FormConfiguration item in allRecords)
            {
                if (item.CategoryId == request.CategoryId)
                {
                    name = item.Name;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Category";
            }

            jsonModel.Category = name;
            jsonModel.Structure = BuildJsonStructure(request.Nodes);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(jsonModel, options);

            int result = await _repository.SaveOrUpdateJsonAsync(request.CategoryId, json, createdBy);

            if (result > 0)
            {
                return (true, "Saved successfully.");
            }

            return (false, "Save failed.");
        }

        // Delete configuration by category
        public async Task<(bool Success, string Message)> DeleteByCategoryAsync(int categoryId, string deletedBy)
        {
            if (categoryId <= 0)
            {
                return (false, "Invalid record.");
            }

            int result = await _repository.DeleteJsonByCategoryAsync(categoryId, deletedBy);

            if (result > 0)
            {
                return (true, "Deleted successfully.");
            }

            return (false, "Delete failed.");
        }

        #endregion

        #region Validation Methods

        // Validate hierarchy structure before save
        private string ValidateNodes(List<FormConfigurationNodeRequest> nodes, bool isRootLevel)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return "Please add configuration structure.";
            }

            foreach (FormConfigurationNodeRequest node in nodes)
            {
                if (node == null)
                {
                    return "Invalid structure found.";
                }

                if (string.IsNullOrWhiteSpace(node.Heading))
                {
                    return "Heading is required.";
                }

                if (string.IsNullOrWhiteSpace(node.NodeName))
                {
                    return "Name is required.";
                }

                if (node.NodeName.Trim().Length > 50)
                {
                    return "Name cannot exceed 50 characters.";
                }

                if (string.IsNullOrWhiteSpace(node.InputType))
                {
                    return "Please select input type.";
                }

                // Validate parent level
                if (isRootLevel)
                {
                    if (node.Children == null || node.Children.Count == 0)
                    {
                        return "Please add at least one child level.";
                    }

                    if (node.InputType != "Text" &&
                        node.InputType != "Date")
                    {
                        return "Parent input type must be Text or Date.";
                    }
                }
                else
                {
                    // Validate child level
                    if (node.InputType != "Single" &&
                        node.InputType != "Multiple")
                    {
                        return "Child input type must be Single or Multiple.";
                    }
                }

                // Validate child nodes recursively
                if (node.Children != null && node.Children.Count > 0)
                {
                    string childMessage = ValidateNodes(node.Children, false);

                    if (!string.IsNullOrWhiteSpace(childMessage))
                    {
                        return childMessage;
                    }
                }
            }

            return "";
        }

        #endregion

        #region JSON Methods

        // Convert saved JSON into hierarchy tree
        private List<FormConfiguration> ConvertJsonToTree(string json)
        {
            List<FormConfiguration> result = new List<FormConfiguration>();

            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            FormConfigurationJsonModel model =
                JsonSerializer.Deserialize<FormConfigurationJsonModel>(json, options);

            if (model == null || model.Structure == null)
            {
                return result;
            }

            foreach (FormConfigurationJsonNode node in model.Structure)
            {
                FormConfiguration convertedNode = ConvertJsonNodeToTree(node);

                result.Add(convertedNode);
            }

            return result;
        }

        // Build JSON structure recursively
        private List<FormConfigurationJsonNode> BuildJsonStructure(List<FormConfigurationNodeRequest> nodes)
        {
            List<FormConfigurationJsonNode> result = new List<FormConfigurationJsonNode>();

            if (nodes == null)
            {
                return result;
            }

            foreach (FormConfigurationNodeRequest node in nodes)
            {
                FormConfigurationJsonNode item = new FormConfigurationJsonNode();

                item.Heading = node.Heading;
                item.Label = node.NodeName;
                item.ValueType = node.InputType;

                item.Children = BuildJsonStructure(node.Children);

                result.Add(item);
            }

            return result;
        }

        // Convert JSON node into tree node
        private FormConfiguration ConvertJsonNodeToTree(FormConfigurationJsonNode jsonNode)
        {
            FormConfiguration node = new FormConfiguration();

            node.Heading = jsonNode.Heading;
            node.NodeName = jsonNode.Label;
            node.InputType = jsonNode.ValueType;
            node.IsActive = true;

            node.Children = new List<FormConfiguration>();

            if (jsonNode.Children != null &&
                jsonNode.Children.Count > 0)
            {
                foreach (FormConfigurationJsonNode child in jsonNode.Children)
                {
                    FormConfiguration childNode = ConvertJsonNodeToTree(child);

                    node.Children.Add(childNode);
                }
            }

            return node;
        }

        #endregion
    }
}