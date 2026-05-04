using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class ProductSetupConfigurationService : IProductSetupConfigurationService
    {
        private readonly IProductSetupConfigurationRepository _setupRepository;
        private readonly IProductConfigurationRepository _configurationRepository;

        public ProductSetupConfigurationService(
            IProductSetupConfigurationRepository setupRepository,
            IProductConfigurationRepository configurationRepository)
        {
            _setupRepository = setupRepository;
            _configurationRepository = configurationRepository;
        }

        public async Task<List<ProductSetupConfiguration>> GetTreeByProductIdAsync(int productId)
        {
            var setupData = await _setupRepository.GetByProductIdAsync(productId);

            List<ProductSetupConfiguration> nodes = new List<ProductSetupConfiguration>();

            foreach (var item in setupData)
            {
                if (item.Children == null)
                {
                    item.Children = new List<ProductSetupConfiguration>();
                }
                else
                {
                    item.Children.Clear();
                }

                if (item.IsFieldValue)
                {
                    item.ConfigurationNodeName = "Field Value";
                }

                nodes.Add(item);
            }

            List<ProductSetupConfiguration> rootNodes = new List<ProductSetupConfiguration>();

            foreach (var node in nodes)
            {
                if (node.ParentNodeId == null)
                {
                    rootNodes.Add(node);
                }
                else
                {
                    foreach (var parent in nodes)
                    {
                        if (parent.NodeId == node.ParentNodeId.Value)
                        {
                            parent.Children.Add(node);
                            break;
                        }
                    }
                }
            }

            return rootNodes;
        }

        public async Task<List<ProductConfiguration>> GetRootLevelsAsync(int productId)
        {
            return await GetChildLevelsAsync(productId, null);
        }

        public async Task<List<ProductConfiguration>> GetChildLevelsAsync(int productId, int? parentConfigurationNodeId)
        {
            var configData = await _configurationRepository.GetByProductIdAsync(productId);

            List<ProductConfiguration> children = new List<ProductConfiguration>();

            foreach (var item in configData)
            {
                if (parentConfigurationNodeId == null)
                {
                    if (item.ParentNodeId == null)
                    {
                        children.Add(item);
                    }
                }
                else
                {
                    if (item.ParentNodeId != null)
                    {
                        if (item.ParentNodeId.Value == parentConfigurationNodeId.Value)
                        {
                            children.Add(item);
                        }
                    }
                }
            }

            return children;
        }

        private async Task<ProductConfiguration> GetConfigurationNodeByIdAsync(int productId, int configurationNodeId)
        {
            var configData = await _configurationRepository.GetByProductIdAsync(productId);

            foreach (var item in configData)
            {
                if (item.NodeId == configurationNodeId)
                {
                    return item;
                }
            }

            return null;
        }

        public async Task<ProductSetupConfiguration> PrepareAddAsync(int productId, int? parentNodeId)
        {
            ProductSetupConfiguration model = new ProductSetupConfiguration();

            model.ProductId = productId;
            model.ParentNodeId = parentNodeId;

            if (parentNodeId == null)
            {
                List<ProductConfiguration> roots = await GetRootLevelsAsync(productId);

                if (roots.Count == 0)
                {
                    return null;
                }

                model.AvailableConfigurationNodes = roots;
                model.IsFieldValue = false;
                model.FieldType = null;

                return model;
            }

            ProductSetupConfiguration parentNode = await _setupRepository.GetNodeByIdAsync(parentNodeId.Value);

            if (parentNode == null)
            {
                return null;
            }

            if (parentNode.IsFieldValue)
            {
                return null;
            }

            List<ProductConfiguration> childLevels = await GetChildLevelsAsync(productId, parentNode.ConfigurationNodeId);

            if (childLevels.Count == 0)
            {
                model.ConfigurationNodeId = null;
                model.ConfigurationNodeName = "Field Value";
                model.InputType = "Text";
                model.IsFieldValue = true;
                model.FieldType = "Text";
            }
            else
            {
                model.AvailableConfigurationNodes = childLevels;
                model.IsFieldValue = false;
                model.FieldType = null;
            }

            return model;
        }

        public async Task<ProductSetupConfiguration> PrepareEditAsync(int nodeId)
        {
            ProductSetupConfiguration node = await _setupRepository.GetNodeByIdAsync(nodeId);

            if (node == null)
            {
                return null;
            }

            if (node.IsFieldValue)
            {
                node.ConfigurationNodeName = "Field Value";

                if (string.IsNullOrWhiteSpace(node.FieldType))
                {
                    node.FieldType = "Text";
                }
            }

            return node;
        }

        public async Task<(bool Success, string Message)> AddAsync(ProductSetupConfiguration model, string createdBy)
        {
            if (model == null)
            {
                return (false, "Invalid request.");
            }

            if (model.ProductId <= 0)
            {
                return (false, "Product is required.");
            }

            if (string.IsNullOrWhiteSpace(model.NodeValue))
            {
                return (false, "Value is required.");
            }

            model.NodeValue = model.NodeValue.Trim();

            if (model.IsFieldValue)
            {
                model.ConfigurationNodeId = null;
                model.ConfigurationNodeName = "Field Value";
                model.InputType = "Text";

                if (string.IsNullOrWhiteSpace(model.FieldType))
                {
                    model.FieldType = "Text";
                }
            }
            else
            {
                if (model.ConfigurationNodeId == null)
                {
                    return (false, "Please select configuration label.");
                }

                ProductConfiguration configNode = await GetConfigurationNodeByIdAsync(
                    model.ProductId,
                    model.ConfigurationNodeId.Value
                );

                if (configNode == null)
                {
                    return (false, "Invalid configuration label.");
                }

                model.InputType = configNode.InputType;
                model.ConfigurationNodeName = configNode.NodeName;
                model.FieldType = null;
            }

            int duplicateCount = await _setupRepository.CheckDuplicateNodeAsync(
                model.ProductId,
                model.ConfigurationNodeId,
                model.ParentNodeId,
                model.NodeValue
            );

            if (duplicateCount > 0)
            {
                return (false, "Duplicate value found: " + model.NodeValue);
            }

            model.IsActive = true;
            model.CreatedBy = createdBy;

            int result = await _setupRepository.AddAsync(model);

            if (result > 0)
            {
                return (true, "Product setup configuration added successfully.");
            }

            return (false, "Failed to add product setup configuration.");
        }

        public async Task<(bool Success, string Message)> UpdateNodeAsync(ProductSetupConfiguration model, string modifiedBy)
        {
            if (model == null)
            {
                return (false, "Invalid request.");
            }

            if (model.NodeId <= 0)
            {
                return (false, "Invalid node.");
            }

            ProductSetupConfiguration oldNode = await _setupRepository.GetNodeByIdAsync(model.NodeId);

            if (oldNode == null)
            {
                return (false, "Setup node not found.");
            }

            if (string.IsNullOrWhiteSpace(model.NodeValue))
            {
                return (false, "Value is required.");
            }

            model.NodeValue = model.NodeValue.Trim();

            model.ProductId = oldNode.ProductId;
            model.ParentNodeId = oldNode.ParentNodeId;
            model.ConfigurationNodeId = oldNode.ConfigurationNodeId;
            model.IsFieldValue = oldNode.IsFieldValue;

            if (oldNode.IsFieldValue)
            {
                if (string.IsNullOrWhiteSpace(model.FieldType))
                {
                    model.FieldType = "Text";
                }
            }
            else
            {
                model.FieldType = null;
            }

            int duplicateCount = await _setupRepository.CheckDuplicateNodeForUpdateAsync(
                model.NodeId,
                model.ProductId,
                model.ConfigurationNodeId,
                model.ParentNodeId,
                model.NodeValue
            );

            if (duplicateCount > 0)
            {
                return (false, "Duplicate value found: " + model.NodeValue);
            }

            model.ModifiedBy = modifiedBy;

            int result = await _setupRepository.UpdateNodeAsync(model);

            if (result > 0)
            {
                return (true, "Product setup configuration updated successfully.");
            }

            return (false, "Failed to update product setup configuration.");
        }

        public async Task<(bool Success, string Message)> SaveDataAsync(ProductSetupConfigurationSaveRequest request, string createdBy)
        {
            if (request == null)
            {
                return (false, "Invalid request.");
            }

            if (request.ProductId <= 0)
            {
                return (false, "Please select product.");
            }

            if (request.Nodes == null || request.Nodes.Count == 0)
            {
                return (false, "Please add setup data.");
            }

            foreach (var node in request.Nodes)
            {
                var result = await SaveNodeRecursive(
                    request.ProductId,
                    null,
                    node,
                    createdBy
                );

                if (!result.Success)
                {
                    return result;
                }
            }

            return (true, "Product setup configuration saved successfully.");
        }

        private async Task<(bool Success, string Message)> SaveNodeRecursive(
            int productId,
            int? parentNodeId,
            ProductSetupConfigurationNodeRequest node,
            string createdBy)
        {
            if (node == null)
            {
                return (false, "Invalid setup node.");
            }

            if (string.IsNullOrWhiteSpace(node.NodeValue))
            {
                return (false, "Setup value cannot be empty.");
            }

            ProductSetupConfiguration model = new ProductSetupConfiguration();

            model.ProductId = productId;
            model.ParentNodeId = parentNodeId;
            model.NodeValue = node.NodeValue.Trim();
            model.IsFieldValue = node.IsFieldValue;
            model.IsActive = true;
            model.CreatedBy = createdBy;

            if (node.IsFieldValue)
            {
                model.ConfigurationNodeId = null;
                model.ConfigurationNodeName = "Field Value";
                model.InputType = "Text";

                if (string.IsNullOrWhiteSpace(node.FieldType))
                {
                    model.FieldType = "Text";
                }
                else
                {
                    model.FieldType = node.FieldType;
                }
            }
            else
            {
                if (node.ConfigurationNodeId == null)
                {
                    return (false, "Configuration label is missing for " + node.NodeValue);
                }

                ProductConfiguration configNode = await GetConfigurationNodeByIdAsync(
                    productId,
                    node.ConfigurationNodeId.Value
                );

                if (configNode == null)
                {
                    return (false, "Invalid configuration label for " + node.NodeValue);
                }

                model.ConfigurationNodeId = node.ConfigurationNodeId;
                model.ConfigurationNodeName = configNode.NodeName;
                model.InputType = configNode.InputType;
                model.FieldType = null;
            }

            int duplicateCount = await _setupRepository.CheckDuplicateNodeAsync(
                model.ProductId,
                model.ConfigurationNodeId,
                model.ParentNodeId,
                model.NodeValue
            );

            if (duplicateCount > 0)
            {
                return (false, "Duplicate value found: " + model.NodeValue);
            }

            int newNodeId = await _setupRepository.AddAsync(model);

            if (newNodeId <= 0)
            {
                return (false, "Failed to save setup value: " + model.NodeValue);
            }

            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    var result = await SaveNodeRecursive(
                        productId,
                        newNodeId,
                        child,
                        createdBy
                    );

                    if (!result.Success)
                    {
                        return result;
                    }
                }
            }

            return (true, "OK");
        }

        public async Task<(bool Success, string Message)> DeleteNodeAsync(int nodeId, string deletedBy)
        {
            if (nodeId <= 0)
            {
                return (false, "Invalid node.");
            }

            int result = await _setupRepository.DeleteNodeAsync(nodeId, deletedBy);

            if (result > 0)
            {
                return (true, "Product setup configuration deleted successfully.");
            }

            return (false, "Failed to delete product setup configuration.");
        }
    }
}