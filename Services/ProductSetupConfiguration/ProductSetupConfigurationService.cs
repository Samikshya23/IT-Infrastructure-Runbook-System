using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class ProductSetupConfigurationService : IProductSetupConfigurationService
    {
        private readonly IProductSetupConfigurationRepository _setupRepository;
        private readonly IProductConfigurationRepository _configurationRepository;
        private readonly IProductEntryRepository _productEntryRepository;

        public ProductSetupConfigurationService(IProductSetupConfigurationRepository setupRepository, IProductConfigurationRepository configurationRepository, IProductEntryRepository productEntryRepository)
        {
            _setupRepository = setupRepository;
            _configurationRepository = configurationRepository;
            _productEntryRepository = productEntryRepository;
        }
        #region Load Methods
        // Load products that already have product configuration.
        public async Task<IEnumerable<ProductSetupConfiguration>> GetConfiguredProductsAsync()
        {
            return await _setupRepository.GetConfiguredProductsAsync();
        }
        //Load setup tree by product.
        public async Task<List<ProductSetupConfiguration>> GetTreeByProductIdAsync(int productId)
        {
            List<ProductSetupConfiguration> result = new List<ProductSetupConfiguration>();

            if (productId <= 0)
            {
                return result;
            }
            ProductSetupConfiguration savedData = await _setupRepository.GetJsonByProductIdAsync(productId);

            if (savedData == null || string.IsNullOrWhiteSpace(savedData.SetupJson))
            {
                return result;
            }

            return ConvertSetupJsonToTree(savedData.SetupJson, productId);
        }
        // Load grouped setup tree by product.
        public async Task<List<ProductSetupConfiguration>> GetGroupedTreeByProductIdAsync(int productId)
        {
            List<ProductSetupConfiguration> tree = await GetTreeByProductIdAsync(productId);
            List<ProductSetupConfiguration> grouped = new List<ProductSetupConfiguration>();

            foreach (ProductSetupConfiguration root in tree)
            {
                ProductSetupConfiguration existing = null;

                foreach (ProductSetupConfiguration item in grouped)
                {
                    if (item.NodeValue == root.NodeValue)
                    {
                        existing = item;
                        break;
                    }
                }
                if (existing == null)
                {
                    existing = new ProductSetupConfiguration();
                    existing.NodeId = root.NodeId;
                    existing.ProductId = root.ProductId;
                    existing.ConfigurationNodeId = root.ConfigurationNodeId;
                    existing.ConfigurationNodeName = root.ConfigurationNodeName;
                    existing.NodeValue = root.NodeValue;
                    existing.FieldType = root.FieldType;
                    existing.IsActive = root.IsActive;
                    existing.Children = new List<ProductSetupConfiguration>();
                    grouped.Add(existing);
                }
                AddLeafChildren(root, existing.Children);
            }
            return grouped;
        }
        // Load root configuration levels from product configuration json.
        public async Task<List<ProductConfiguration>> GetRootLevelsAsync(int productId)
        {
            List<ProductConfiguration> allNodes = await GetConfigurationTreeFromJsonAsync(productId);
            List<ProductConfiguration> roots = new List<ProductConfiguration>();

            foreach (ProductConfiguration node in allNodes)
            {
                if (node.ParentNodeId == null)
                {
                    roots.Add(node);
                }
            }
            return roots;
        }
        // Load child configuration levels from product configuration json.
        public async Task<List<ProductConfiguration>> GetChildLevelsAsync(int productId, int? parentConfigurationNodeId)
        {
            List<ProductConfiguration> allNodes = await GetConfigurationTreeFromJsonAsync(productId);
            List<ProductConfiguration> children = new List<ProductConfiguration>();

            foreach (ProductConfiguration node in allNodes)
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
        public async Task<(bool Success, string Message, ProductSetupConfigurationNodeRequest Data)> GetRootForEditAsync(int productId, int rootIndex)
        {
            if (productId <= 0)
            {
                return (false, "Invalid main item.", null);
            }

            if (rootIndex < 0)
            {
                return (false, "Invalid setup item.", null);
            }
            List<ProductSetupConfigurationNodeRequest> nodes = await GetSetupNodesFromJsonAsync(productId);

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
        public async Task<(bool Success, string Message)> SaveDataAsync(ProductSetupConfigurationSaveRequest request, string createdBy)
        {
            if (request == null)
            {
                return (false, "Invalid request.");
            }

            if (request.ProductId <= 0)
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

            List<ProductSetupConfigurationNodeRequest> existingNodes = await GetSetupNodesFromJsonAsync(request.ProductId);

            if (existingNodes == null)
            {
                existingNodes = new List<ProductSetupConfigurationNodeRequest>();
            }

            foreach (ProductSetupConfigurationNodeRequest newNode in request.Nodes)
            {
                foreach (ProductSetupConfigurationNodeRequest existingNode in existingNodes)
                {
                    if (!string.IsNullOrWhiteSpace(existingNode.Value) && !string.IsNullOrWhiteSpace(newNode.Value) && existingNode.Value.Trim().ToLower() == newNode.Value.Trim().ToLower())
                    {
                        return (false, "This setup item already exists. Please edit existing item.");
                    }
                }
            }
            foreach (ProductSetupConfigurationNodeRequest node in request.Nodes)
            {
                existingNodes.Add(node);
            }
            string json = SerializeSetupNodes(existingNodes);
            int result = await _setupRepository.SaveOrUpdateJsonAsync(request.ProductId, json, createdBy, createdBy);

            if (result > 0)
            {
                return (true, "Saved successfully.");
            }

            return (false, "Save failed.");
        }

        // Update selected root setup group only
        public async Task<(bool Success, string Message)> SaveRootDataAsync(ProductSetupConfigurationSaveRequest request, string modifiedBy)
        {
            if (request == null)
            {
                return (false, "Invalid request.");
            }
            if (request.ProductId <= 0)
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

            List<ProductSetupConfigurationNodeRequest> existingNodes = await GetSetupNodesFromJsonAsync(request.ProductId);

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

                if (!string.IsNullOrWhiteSpace(existingNodes[i].Value) && !string.IsNullOrWhiteSpace(newValue) && existingNodes[i].Value.Trim().ToLower() == newValue.Trim().ToLower())
                {
                    return (false, "This setup item already exists.");
                }
            }
            existingNodes[request.RootIndex] = request.Nodes[0];
            string json = SerializeSetupNodes(existingNodes);
            int result = await _setupRepository.SaveOrUpdateJsonAsync(request.ProductId, json, modifiedBy, modifiedBy);

            if (result > 0)
            {
                return (true, "Updated successfully.");
            }
            return (false, "Update failed.");
        }
        #endregion
        #region Delete Methods

        // Delete complete setup configuration by product
        public async Task<(bool Success, string Message)> DeleteByProductAsync(int productId, string deletedBy)
        {
            if (productId <= 0)
            {
                return (false, "Invalid setup configuration.");
            }

            int result = await _setupRepository.DeleteJsonByProductAsync(productId, deletedBy);

            if (result > 0)
            {
                return (true, "Deleted successfully.");
            }

            return (false, "Delete failed.");
        }

        // Delete selected root setup group only
        public async Task<(bool Success, string Message)> DeleteRootAsync(int productId, int rootIndex, string deletedBy)
        {
            if (productId <= 0)
            {
                return (false, "Invalid setup configuration.");
            }

            if (rootIndex < 0)
            {
                return (false, "Invalid setup item.");
            }

            List<ProductSetupConfigurationNodeRequest> existingNodes = await GetSetupNodesFromJsonAsync(productId);

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
                int deleteResult = await _setupRepository.DeleteJsonByProductAsync(productId, deletedBy);

                if (deleteResult >= 0)
                {
                    return (true, "Deleted successfully.");
                }

                return (false, "Delete failed.");
            }
            string json = SerializeSetupNodes(existingNodes);
            int result = await _setupRepository.SaveOrUpdateJsonAsync(productId, json, deletedBy, deletedBy);

            if (result >= 0)
            {
                return (true, "Deleted successfully.");
            }

            return (false, "Delete failed.");
        }
        #endregion
        #region Json Methods
        // Load setup nodes from saved setup json
        private async Task<List<ProductSetupConfigurationNodeRequest>> GetSetupNodesFromJsonAsync(int productId)
        {
            List<ProductSetupConfigurationNodeRequest> result = new List<ProductSetupConfigurationNodeRequest>();

            if (productId <= 0)
            {
                return result;
            }
            ProductSetupConfiguration savedData = await _setupRepository.GetJsonByProductIdAsync(productId);

            if (savedData == null || string.IsNullOrWhiteSpace(savedData.SetupJson))
            {
                return result;
            }
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            result = JsonSerializer.Deserialize<List<ProductSetupConfigurationNodeRequest>>(savedData.SetupJson, options);
            if (result == null)
            {
                result = new List<ProductSetupConfigurationNodeRequest>();
            }
            EnsureNodeIds(result);
            return result;
        }
        // Convert setup nodes into json
        private string SerializeSetupNodes(List<ProductSetupConfigurationNodeRequest> nodes)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = false;
            return JsonSerializer.Serialize(nodes, options);
        }

        #endregion

        #region Validation Methods

        // Validate setup nodes recursively
        private string ValidateSetupNodes(List<ProductSetupConfigurationNodeRequest> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return "Please add setup data.";
            }

            foreach (ProductSetupConfigurationNodeRequest node in nodes)
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
        private void EnsureNodeIds(List<ProductSetupConfigurationNodeRequest> nodes)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (ProductSetupConfigurationNodeRequest node in nodes)
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
        private void AddLeafChildren(ProductSetupConfiguration node, List<ProductSetupConfiguration> children)
        {
            if (node == null)
            {
                return;
            }

            if (node.Children == null || node.Children.Count == 0)
            {
                return;
            }

            foreach (ProductSetupConfiguration child in node.Children)
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

        // Load product configuration tree from json
        private async Task<List<ProductConfiguration>> GetConfigurationTreeFromJsonAsync(int productId)
        {
            List<ProductConfiguration> result = new List<ProductConfiguration>();

            if (productId <= 0)
            {
                return result;
            }

            ProductConfiguration configuration = await _configurationRepository.GetJsonByProductIdAsync(productId);

            if (configuration == null || string.IsNullOrWhiteSpace(configuration.ConfigurationJson))
            {
                return result;
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            ProductConfigurationJsonModel model = JsonSerializer.Deserialize<ProductConfigurationJsonModel>(configuration.ConfigurationJson, options);

            if (model == null || model.Structure == null)
            {
                return result;
            }

            int nodeCounter = 1;

            foreach (ProductConfigurationJsonNode node in model.Structure)
            {
                ConvertConfigurationNode(node, null, result, ref nodeCounter);
            }

            return result;
        }

        #endregion

        #region Conversion Methods

        // Convert product configuration json node into tree node
        private void ConvertConfigurationNode(ProductConfigurationJsonNode requestNode, int? parentNodeId, List<ProductConfiguration> result, ref int nodeCounter)
        {
            if (requestNode == null)
            {
                return;
            }

            ProductConfiguration node = new ProductConfiguration();
            node.NodeId = nodeCounter;
            node.ParentNodeId = parentNodeId;
            node.Heading = requestNode.Heading;
            node.NodeName = requestNode.Label;
            node.InputType = requestNode.ValueType;
            node.Children = new List<ProductConfiguration>();

            int currentNodeId = nodeCounter;
            nodeCounter++;

            result.Add(node);

            if (requestNode.Children != null && requestNode.Children.Count > 0)
            {
                foreach (ProductConfigurationJsonNode child in requestNode.Children)
                {
                    ConvertConfigurationNode(child, currentNodeId, result, ref nodeCounter);
                }
            }
        }

        // Convert setup json into setup tree
        private List<ProductSetupConfiguration> ConvertSetupJsonToTree(string json, int productId)
        {
            List<ProductSetupConfiguration> result = new List<ProductSetupConfiguration>();

            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            List<ProductSetupConfigurationNodeRequest> nodes = JsonSerializer.Deserialize<List<ProductSetupConfigurationNodeRequest>>(json, options);

            if (nodes == null)
            {
                return result;
            }

            EnsureNodeIds(nodes);

            int nodeCounter = 1;

            foreach (ProductSetupConfigurationNodeRequest node in nodes)
            {
                ProductSetupConfiguration converted = ConvertSetupNode(node, productId, ref nodeCounter);
                result.Add(converted);
            }

            return result;
        }

        // Convert setup request node into setup display node
        private ProductSetupConfiguration ConvertSetupNode(ProductSetupConfigurationNodeRequest requestNode, int productId, ref int nodeCounter)
        {
            ProductSetupConfiguration node = new ProductSetupConfiguration();
            node.NodeId = nodeCounter;
            node.ProductId = productId;
            node.ConfigurationNodeId = requestNode.ConfigurationNodeId;
            node.ConfigurationNodeName = requestNode.Label;
            node.NodeValue = requestNode.Value;
            node.FieldType = requestNode.FieldType;
            node.IsFieldValue = false;
            node.IsActive = true;
            node.Children = new List<ProductSetupConfiguration>();

            nodeCounter++;

            if (requestNode.Children != null && requestNode.Children.Count > 0)
            {
                foreach (ProductSetupConfigurationNodeRequest child in requestNode.Children)
                {
                    ProductSetupConfiguration childNode = ConvertSetupNode(child, productId, ref nodeCounter);
                    node.Children.Add(childNode);
                }
            }

            return node;
        }

        #endregion

        #region Entry Helper Methods

        // Add valid setup node ids from leaf nodes
        private void AddValidSetupNodeIds(ProductSetupConfigurationNodeRequest node, List<string> validIds)
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

            foreach (ProductSetupConfigurationNodeRequest child in node.Children)
            {
                AddValidSetupNodeIds(child, validIds);
            }
        }

        // Build comma separated valid setup node ids
        private string BuildValidSetupNodeIds(List<ProductSetupConfigurationNodeRequest> nodes)
        {
            List<string> validIds = new List<string>();

            if (nodes == null)
            {
                return "";
            }

            foreach (ProductSetupConfigurationNodeRequest node in nodes)
            {
                AddValidSetupNodeIds(node, validIds);
            }

            return string.Join(",", validIds);
        }

        #endregion
    }
}