using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class CategorySetupService : ICategorySetupService
    {
        private readonly ICategorySetupRepository _setupRepository;
        private readonly IFormConfigurationRepository _configurationRepository;

        public CategorySetupService(ICategorySetupRepository setupRepository, IFormConfigurationRepository configurationRepository)
        {
            _setupRepository = setupRepository;
            _configurationRepository = configurationRepository;
        }

        #region Load Methods

        // Load categories that already have form configuration.
        public async Task<IEnumerable<CategorySetup>> GetConfiguredCategoriesAsync()
        {
            return await _setupRepository.GetConfiguredCategoriesAsync();
        }

        // Load setup tree by category.
        public async Task<List<CategorySetup>> GetTreeByCategoryIdAsync(int categoryId)
        {
            List<CategorySetup> result = new List<CategorySetup>();

            if (categoryId <= 0)
            {
                return result;
            }

            CategorySetup savedData = await _setupRepository.GetJsonByCategoryIdAsync(categoryId);

            if (savedData == null || string.IsNullOrWhiteSpace(savedData.SetupJson))
            {
                return result;
            }

            return ConvertSetupJsonToTree(savedData.SetupJson, categoryId);
        }

        // Load grouped setup tree by category.
        public async Task<List<CategorySetup>> GetGroupedTreeByCategoryIdAsync(int categoryId)
        {
            List<CategorySetup> tree = await GetTreeByCategoryIdAsync(categoryId);
            List<CategorySetup> grouped = new List<CategorySetup>();

            foreach (CategorySetup root in tree)
            {
                CategorySetup existing = null;

                foreach (CategorySetup item in grouped)
                {
                    if (item.NodeValue == root.NodeValue)
                    {
                        existing = item;
                        break;
                    }
                }

                if (existing == null)
                {
                    existing = new CategorySetup();
                    existing.NodeId = root.NodeId;
                    existing.CategoryId = root.CategoryId;
                    existing.ConfigurationNodeId = root.ConfigurationNodeId;
                    existing.ConfigurationNodeName = root.ConfigurationNodeName;
                    existing.NodeValue = root.NodeValue;
                    existing.FieldType = root.FieldType;
                    existing.IsActive = root.IsActive;
                    existing.Children = new List<CategorySetup>();

                    grouped.Add(existing);
                }

                AddLeafChildren(root, existing.Children);
            }

            return grouped;
        }

        // Load root configuration levels from form configuration json.
        public async Task<List<FormConfiguration>> GetRootLevelsAsync(int categoryId)
        {
            List<FormConfiguration> allNodes = await GetConfigurationTreeFromJsonAsync(categoryId);
            List<FormConfiguration> roots = new List<FormConfiguration>();

            foreach (FormConfiguration node in allNodes)
            {
                if (node.ParentNodeId == null)
                {
                    roots.Add(node);
                }
            }

            return roots;
        }

        // Load child configuration levels from form configuration json.
        public async Task<List<FormConfiguration>> GetChildLevelsAsync(int categoryId, int? parentConfigurationNodeId)
        {
            List<FormConfiguration> allNodes = await GetConfigurationTreeFromJsonAsync(categoryId);
            List<FormConfiguration> children = new List<FormConfiguration>();

            foreach (FormConfiguration node in allNodes)
            {
                if (parentConfigurationNodeId == null)
                {
                    if (node.ParentNodeId == null)
                    {
                        children.Add(node);
                    }
                }
                else
                {
                    if (node.ParentNodeId != null && node.ParentNodeId.Value == parentConfigurationNodeId.Value)
                    {
                        children.Add(node);
                    }
                }
            }

            return children;
        }

        // Load selected root setup group for edit.
        public async Task<(bool Success, string Message, CategorySetupNodeRequest Data)> GetRootForEditAsync(int categoryId, int rootIndex)
        {
            if (categoryId <= 0)
            {
                return (false, "Invalid main item.", null);
            }

            if (rootIndex < 0)
            {
                return (false, "Invalid setup item.", null);
            }

            List<CategorySetupNodeRequest> nodes = await GetSetupNodesFromJsonAsync(categoryId);

            if (nodes == null || nodes.Count == 0)
            {
                return (false, "No setup data found.", null);
            }

            if (rootIndex >= nodes.Count)
            {
                return (false, "Setup item not found.", null);
            }

            return (true, "Loaded successfully.", nodes[rootIndex]);
        }

        #endregion

        #region Save Methods

        // Save new setup data into existing setup json.
        public async Task<(bool Success, string Message)> SaveDataAsync(CategorySetupSaveRequest request, string createdBy)
        {
            if (request == null)
            {
                return (false, "Invalid request.");
            }

            if (request.CategoryId <= 0)
            {
                return (false, "Please select a main item.");
            }

            if (request.Nodes == null || request.Nodes.Count == 0)
            {
                return (false, "Please add setup data.");
            }

            EnsureNodeIds(request.Nodes);

            string validationMessage = ValidateSetupNodes(request.Nodes);

            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return (false, validationMessage);
            }

            List<CategorySetupNodeRequest> existingNodes = await GetSetupNodesFromJsonAsync(request.CategoryId);

            if (existingNodes == null)
            {
                existingNodes = new List<CategorySetupNodeRequest>();
            }

            foreach (CategorySetupNodeRequest newNode in request.Nodes)
            {
                foreach (CategorySetupNodeRequest existingNode in existingNodes)
                {
                    if (!string.IsNullOrWhiteSpace(existingNode.Value) &&
                        !string.IsNullOrWhiteSpace(newNode.Value) &&
                        existingNode.Value.Trim().ToLower() == newNode.Value.Trim().ToLower())
                    {
                        return (false, "This setup item already exists. Please edit existing item.");
                    }
                }
            }

            foreach (CategorySetupNodeRequest node in request.Nodes)
            {
                existingNodes.Add(node);
            }

            string json = SerializeSetupNodes(existingNodes);

            int result = await _setupRepository.SaveOrUpdateJsonAsync(request.CategoryId, json, createdBy, createdBy);

            if (result > 0)
            {
                return (true, "Saved successfully.");
            }

            return (false, "Save failed.");
        }

        // Update selected root setup group only
        public async Task<(bool Success, string Message)> SaveRootDataAsync(CategorySetupSaveRequest request, string modifiedBy)
        {
            if (request == null)
            {
                return (false, "Invalid request.");
            }

            if (request.CategoryId <= 0)
            {
                return (false, "Please select a main item.");
            }

            if (request.RootIndex < 0)
            {
                return (false, "Invalid setup item.");
            }

            if (request.Nodes == null || request.Nodes.Count == 0)
            {
                return (false, "Please add setup data.");
            }

            EnsureNodeIds(request.Nodes);

            string validationMessage = ValidateSetupNodes(request.Nodes);

            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return (false, validationMessage);
            }

            List<CategorySetupNodeRequest> existingNodes = await GetSetupNodesFromJsonAsync(request.CategoryId);

            if (existingNodes == null || existingNodes.Count == 0)
            {
                return (false, "No setup data found.");
            }

            if (request.RootIndex >= existingNodes.Count)
            {
                return (false, "Setup item not found.");
            }

            string newValue = request.Nodes[0].Value;

            for (int i = 0; i < existingNodes.Count; i++)
            {
                if (i == request.RootIndex)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(existingNodes[i].Value) &&
                    !string.IsNullOrWhiteSpace(newValue) &&
                    existingNodes[i].Value.Trim().ToLower() == newValue.Trim().ToLower())
                {
                    return (false, "This setup item already exists.");
                }
            }

            existingNodes[request.RootIndex] = request.Nodes[0];

            string json = SerializeSetupNodes(existingNodes);

            int result = await _setupRepository.SaveOrUpdateJsonAsync(request.CategoryId, json, modifiedBy, modifiedBy);

            if (result > 0)
            {
                return (true, "Updated successfully.");
            }

            return (false, "Update failed.");
        }

        #endregion

        #region Delete Methods

        // Delete complete setup configuration by category
        public async Task<(bool Success, string Message)> DeleteByCategoryAsync(int categoryId, string deletedBy)
        {
            if (categoryId <= 0)
            {
                return (false, "Invalid setup configuration.");
            }

            int result = await _setupRepository.DeleteJsonByCategoryAsync(categoryId, deletedBy);

            if (result > 0)
            {
                return (true, "Deleted successfully.");
            }

            return (false, "Delete failed.");
        }

        // Delete selected root setup group only
        public async Task<(bool Success, string Message)> DeleteRootAsync(int categoryId, int rootIndex, string deletedBy)
        {
            if (categoryId <= 0)
            {
                return (false, "Invalid setup configuration.");
            }

            if (rootIndex < 0)
            {
                return (false, "Invalid setup item.");
            }

            List<CategorySetupNodeRequest> existingNodes = await GetSetupNodesFromJsonAsync(categoryId);

            if (existingNodes == null || existingNodes.Count == 0)
            {
                return (false, "No setup data found.");
            }

            if (rootIndex >= existingNodes.Count)
            {
                return (false, "Setup item not found.");
            }

            existingNodes.RemoveAt(rootIndex);

            if (existingNodes.Count == 0)
            {
                int deleteResult = await _setupRepository.DeleteJsonByCategoryAsync(categoryId, deletedBy);

                if (deleteResult >= 0)
                {
                    return (true, "Deleted successfully.");
                }

                return (false, "Delete failed.");
            }

            string json = SerializeSetupNodes(existingNodes);

            int result = await _setupRepository.SaveOrUpdateJsonAsync(categoryId, json, deletedBy, deletedBy);

            if (result >= 0)
            {
                return (true, "Deleted successfully.");
            }

            return (false, "Delete failed.");
        }

        #endregion

        #region Json Methods

        // Load setup nodes from saved setup json
        private async Task<List<CategorySetupNodeRequest>> GetSetupNodesFromJsonAsync(int categoryId)
        {
            List<CategorySetupNodeRequest> result = new List<CategorySetupNodeRequest>();

            if (categoryId <= 0)
            {
                return result;
            }

            CategorySetup savedData = await _setupRepository.GetJsonByCategoryIdAsync(categoryId);

            if (savedData == null || string.IsNullOrWhiteSpace(savedData.SetupJson))
            {
                return result;
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            result = JsonSerializer.Deserialize<List<CategorySetupNodeRequest>>(savedData.SetupJson, options);

            if (result == null)
            {
                result = new List<CategorySetupNodeRequest>();
            }

            EnsureNodeIds(result);

            return result;
        }

        // Convert setup nodes into json
        private string SerializeSetupNodes(List<CategorySetupNodeRequest> nodes)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = false;

            return JsonSerializer.Serialize(nodes, options);
        }

        #endregion

        #region Validation Methods

        // Validate setup nodes recursively
        private string ValidateSetupNodes(List<CategorySetupNodeRequest> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return "Please add setup data.";
            }

            foreach (CategorySetupNodeRequest node in nodes)
            {
                if (node == null)
                {
                    return "Invalid setup data.";
                }

                if (string.IsNullOrWhiteSpace(node.Id))
                {
                    return "Invalid setup identity.";
                }

                if (string.IsNullOrWhiteSpace(node.Value))
                {
                    return "Setup value is required.";
                }

                if (node.Value.Trim().Length > 100)
                {
                    return "Setup value cannot be more than 100 characters.";
                }

                if (string.IsNullOrWhiteSpace(node.Label))
                {
                    return "Invalid setup label.";
                }

                if (string.IsNullOrWhiteSpace(node.ValueType))
                {
                    return "Invalid setup value type.";
                }

                if (node.ConfigurationNodeId == null || node.ConfigurationNodeId.Value <= 0)
                {
                    return "Invalid configuration node.";
                }

                if (node.Children != null && node.Children.Count > 0)
                {
                    string childMessage = ValidateSetupNodes(node.Children);

                    if (!string.IsNullOrWhiteSpace(childMessage))
                    {
                        return childMessage;
                    }
                }
            }

            return "";
        }

        #endregion

        #region Tree Helper Methods

        // Ensure every setup node has unique id
        private void EnsureNodeIds(List<CategorySetupNodeRequest> nodes)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (CategorySetupNodeRequest node in nodes)
            {
                if (node == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(node.Id))
                {
                    node.Id = "node_" + System.Guid.NewGuid().ToString("N");
                }

                if (node.Children != null && node.Children.Count > 0)
                {
                    EnsureNodeIds(node.Children);
                }
            }
        }

        // Add only leaf children into grouped node
        private void AddLeafChildren(CategorySetup node, List<CategorySetup> children)
        {
            if (node == null)
            {
                return;
            }

            if (node.Children == null || node.Children.Count == 0)
            {
                return;
            }

            foreach (CategorySetup child in node.Children)
            {
                if (child.Children == null || child.Children.Count == 0)
                {
                    children.Add(child);
                }
                else
                {
                    AddLeafChildren(child, children);
                }
            }
        }

        // Load form configuration tree from json
        private async Task<List<FormConfiguration>> GetConfigurationTreeFromJsonAsync(int categoryId)
        {
            List<FormConfiguration> result = new List<FormConfiguration>();

            if (categoryId <= 0)
            {
                return result;
            }

            FormConfiguration configuration = await _configurationRepository.GetJsonByCategoryIdAsync(categoryId);

            if (configuration == null || string.IsNullOrWhiteSpace(configuration.ConfigurationJson))
            {
                return result;
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            FormConfigurationJsonModel model = JsonSerializer.Deserialize<FormConfigurationJsonModel>(configuration.ConfigurationJson, options);

            if (model == null || model.Structure == null)
            {
                return result;
            }

            int nodeCounter = 1;

            foreach (FormConfigurationJsonNode node in model.Structure)
            {
                ConvertConfigurationNode(node, null, result, ref nodeCounter);
            }

            return result;
        }

        #endregion

        #region Conversion Methods

        // Convert form configuration json node into tree node
        private void ConvertConfigurationNode(FormConfigurationJsonNode requestNode, int? parentNodeId, List<FormConfiguration> result, ref int nodeCounter)
        {
            if (requestNode == null)
            {
                return;
            }

            FormConfiguration node = new FormConfiguration();

            node.NodeId = nodeCounter;
            node.ParentNodeId = parentNodeId;
            node.Heading = requestNode.Heading;
            node.NodeName = requestNode.Label;
            node.InputType = requestNode.ValueType;
            node.Children = new List<FormConfiguration>();

            int currentNodeId = nodeCounter;
            nodeCounter++;

            result.Add(node);

            if (requestNode.Children != null && requestNode.Children.Count > 0)
            {
                foreach (FormConfigurationJsonNode child in requestNode.Children)
                {
                    ConvertConfigurationNode(child, currentNodeId, result, ref nodeCounter);
                }
            }
        }

        // Convert setup json into setup tree
        private List<CategorySetup> ConvertSetupJsonToTree(string json, int categoryId)
        {
            List<CategorySetup> result = new List<CategorySetup>();

            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            List<CategorySetupNodeRequest> nodes = JsonSerializer.Deserialize<List<CategorySetupNodeRequest>>(json, options);

            if (nodes == null)
            {
                return result;
            }

            EnsureNodeIds(nodes);

            int nodeCounter = 1;

            foreach (CategorySetupNodeRequest node in nodes)
            {
                CategorySetup converted = ConvertSetupNode(node, categoryId, ref nodeCounter);

                result.Add(converted);
            }

            return result;
        }

        // Convert setup request node into setup display node
        private CategorySetup ConvertSetupNode(CategorySetupNodeRequest requestNode, int categoryId, ref int nodeCounter)
        {
            CategorySetup node = new CategorySetup();

            node.NodeId = nodeCounter;
            node.CategoryId = categoryId;
            node.ConfigurationNodeId = requestNode.ConfigurationNodeId;
            node.ConfigurationNodeName = requestNode.Label;
            node.NodeValue = requestNode.Value;
            node.FieldType = requestNode.FieldType;
            node.IsFieldValue = false;
            node.IsActive = true;
            node.Children = new List<CategorySetup>();

            nodeCounter++;

            if (requestNode.Children != null && requestNode.Children.Count > 0)
            {
                foreach (CategorySetupNodeRequest child in requestNode.Children)
                {
                    CategorySetup childNode = ConvertSetupNode(child, categoryId, ref nodeCounter);

                    node.Children.Add(childNode);
                }
            }

            return node;
        }

        #endregion

        #region Entry Helper Methods

        // Add valid setup node ids from leaf nodes
        private void AddValidSetupNodeIds(CategorySetupNodeRequest node, List<string> validIds)
        {
            if (node == null)
            {
                return;
            }

            if (node.Children == null || node.Children.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(node.Id))
                {
                    validIds.Add(node.Id);
                }

                return;
            }

            foreach (CategorySetupNodeRequest child in node.Children)
            {
                AddValidSetupNodeIds(child, validIds);
            }
        }

        // Build comma separated valid setup node ids
        private string BuildValidSetupNodeIds(List<CategorySetupNodeRequest> nodes)
        {
            List<string> validIds = new List<string>();

            if (nodes == null)
            {
                return "";
            }

            foreach (CategorySetupNodeRequest node in nodes)
            {
                AddValidSetupNodeIds(node, validIds);
            }

            return string.Join(",", validIds);
        }

        #endregion
    }
}