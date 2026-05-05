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

        public ProductSetupConfigurationService(
            IProductSetupConfigurationRepository setupRepository,
            IProductConfigurationRepository configurationRepository)
        {
            _setupRepository = setupRepository;
            _configurationRepository = configurationRepository;
        }

        public async Task<List<ProductSetupConfiguration>> GetTreeByProductIdAsync(int productId)
        {
            List<ProductSetupConfiguration> result = new List<ProductSetupConfiguration>();

            if (productId <= 0)
            {
                return result;
            }

            ProductSetupConfiguration savedData =
                await _setupRepository.GetJsonByProductIdAsync(productId);

            if (savedData == null ||
                string.IsNullOrWhiteSpace(savedData.SetupJson))
            {
                return result;
            }

            return ConvertSetupJsonToTree(savedData.SetupJson, productId);
        }

        public async Task<List<ProductConfiguration>> GetRootLevelsAsync(int productId)
        {
            List<ProductConfiguration> allNodes =
                await GetConfigurationTreeFromJsonAsync(productId);

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

        public async Task<List<ProductConfiguration>> GetChildLevelsAsync(
            int productId,
            int? parentConfigurationNodeId)
        {
            List<ProductConfiguration> allNodes =
                await GetConfigurationTreeFromJsonAsync(productId);

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
                    if (node.ParentNodeId != null &&
                        node.ParentNodeId.Value == parentConfigurationNodeId.Value)
                    {
                        children.Add(node);
                    }
                }
            }

            return children;
        }

        public async Task<(bool Success, string Message)> SaveDataAsync(
            ProductSetupConfigurationSaveRequest request,
            string createdBy)
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

            string validationMessage = ValidateSetupNodes(request.Nodes);

            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return (false, validationMessage);
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = false;

            string json = JsonSerializer.Serialize(request.Nodes, options);

            int result = await _setupRepository.SaveOrUpdateJsonAsync(
                request.ProductId,
                json,
                createdBy
            );

            if (result > 0)
            {
                return (true, "Product setup configuration saved successfully.");
            }

            return (false, "Product setup configuration save failed.");
        }

        public async Task<(bool Success, string Message)> DeleteByProductAsync(
            int productId,
            string deletedBy)
        {
            if (productId <= 0)
            {
                return (false, "Invalid product setup configuration.");
            }

            int result =
                await _setupRepository.DeleteJsonByProductAsync(productId, deletedBy);

            if (result > 0)
            {
                return (true, "Product setup configuration deleted successfully.");
            }

            return (false, "Product setup configuration delete failed.");
        }

        private string ValidateSetupNodes(
            List<ProductSetupConfigurationNodeRequest> nodes)
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

                if (string.IsNullOrWhiteSpace(node.NodeValue))
                {
                    return "Setup value is required.";
                }

                if (node.NodeValue.Trim().Length > 100)
                {
                    return "Setup value cannot be more than 100 characters.";
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

        private async Task<List<ProductConfiguration>> GetConfigurationTreeFromJsonAsync(
            int productId)
        {
            List<ProductConfiguration> result = new List<ProductConfiguration>();

            if (productId <= 0)
            {
                return result;
            }

            ProductConfiguration configuration =
                await _configurationRepository.GetJsonByProductIdAsync(productId);

            if (configuration == null ||
                string.IsNullOrWhiteSpace(configuration.ConfigurationJson))
            {
                return result;
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            List<ProductConfigurationNodeRequest> nodes =
                JsonSerializer.Deserialize<List<ProductConfigurationNodeRequest>>(
                    configuration.ConfigurationJson,
                    options
                );

            if (nodes == null)
            {
                return result;
            }

            int nodeCounter = 1;

            foreach (ProductConfigurationNodeRequest node in nodes)
            {
                ConvertConfigurationNode(
                    node,
                    null,
                    result,
                    ref nodeCounter
                );
            }

            return result;
        }

        private void ConvertConfigurationNode(
            ProductConfigurationNodeRequest requestNode,
            int? parentNodeId,
            List<ProductConfiguration> result,
            ref int nodeCounter)
        {
            if (requestNode == null)
            {
                return;
            }

            ProductConfiguration node = new ProductConfiguration();

            node.NodeId = nodeCounter;
            node.ParentNodeId = parentNodeId;
            node.NodeName = requestNode.NodeName;
            node.InputType = requestNode.InputType;
            node.Children = new List<ProductConfiguration>();

            int currentNodeId = nodeCounter;
            nodeCounter++;

            result.Add(node);

            if (requestNode.Children != null &&
                requestNode.Children.Count > 0)
            {
                foreach (ProductConfigurationNodeRequest child
                    in requestNode.Children)
                {
                    ConvertConfigurationNode(
                        child,
                        currentNodeId,
                        result,
                        ref nodeCounter
                    );
                }
            }
        }

        private List<ProductSetupConfiguration> ConvertSetupJsonToTree(
            string json,
            int productId)
        {
            List<ProductSetupConfiguration> result =
                new List<ProductSetupConfiguration>();

            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            List<ProductSetupConfigurationNodeRequest> nodes =
                JsonSerializer.Deserialize<List<ProductSetupConfigurationNodeRequest>>(
                    json,
                    options
                );

            if (nodes == null)
            {
                return result;
            }

            int nodeCounter = 1;

            foreach (ProductSetupConfigurationNodeRequest node in nodes)
            {
                ProductSetupConfiguration converted =
                    ConvertSetupNode(node, productId, ref nodeCounter);

                result.Add(converted);
            }

            return result;
        }

        private ProductSetupConfiguration ConvertSetupNode(
            ProductSetupConfigurationNodeRequest requestNode,
            int productId,
            ref int nodeCounter)
        {
            ProductSetupConfiguration node =
                new ProductSetupConfiguration();

            node.NodeId = nodeCounter;
            node.ProductId = productId;

            node.NodeValue = requestNode.NodeValue;
            node.FieldType = requestNode.FieldType;
            node.IsFieldValue = requestNode.IsFieldValue;
            node.ConfigurationNodeId = requestNode.ConfigurationNodeId;

            node.IsActive = true;
            node.Children = new List<ProductSetupConfiguration>();

            nodeCounter++;

            if (requestNode.Children != null &&
                requestNode.Children.Count > 0)
            {
                foreach (ProductSetupConfigurationNodeRequest child
                    in requestNode.Children)
                {
                    ProductSetupConfiguration childNode =
                        ConvertSetupNode(child, productId, ref nodeCounter);

                    node.Children.Add(childNode);
                }
            }

            return node;
        }
    }
}