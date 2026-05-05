using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class ProductConfigurationService : IProductConfigurationService
    {
        private readonly IProductConfigurationRepository _repository;

        public ProductConfigurationService(IProductConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductConfigurationIndexItem>> GetIndexAsync()
        {
            List<ProductConfigurationIndexItem> result = new List<ProductConfigurationIndexItem>();

            IEnumerable<ProductConfiguration> data = await _repository.GetAllAsync();

            foreach (ProductConfiguration item in data)
            {
                if (item == null)
                {
                    continue;
                }

                ProductConfigurationIndexItem indexItem = new ProductConfigurationIndexItem();

                indexItem.ProductId = item.ProductId;
                indexItem.ProductName = item.ProductName;

                if (!string.IsNullOrWhiteSpace(item.ConfigurationJson))
                {
                    indexItem.Nodes = ConvertJsonToTree(item.ConfigurationJson);
                }
                else
                {
                    indexItem.Nodes = new List<ProductConfiguration>();
                }

                result.Add(indexItem);
            }

            return result;
        }

        public async Task<List<ProductConfiguration>> GetTreeByProductIdAsync(int productId)
        {
            if (productId <= 0)
            {
                return new List<ProductConfiguration>();
            }

            ProductConfiguration data = await _repository.GetJsonByProductIdAsync(productId);

            if (data == null)
            {
                return new List<ProductConfiguration>();
            }

            if (string.IsNullOrWhiteSpace(data.ConfigurationJson))
            {
                return new List<ProductConfiguration>();
            }

            return ConvertJsonToTree(data.ConfigurationJson);
        }

        public async Task<(bool Success, string Message)> SaveStructureAsync(
            ProductConfigurationSaveRequest request,
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
                return (false, "Please add at least one structure.");
            }

            string validationMessage = ValidateNodes(request.Nodes, true);

            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return (false, validationMessage);
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = false;

            string json = JsonSerializer.Serialize(request.Nodes, options);

            int result = await _repository.SaveOrUpdateJsonAsync(
                request.ProductId,
                json,
                createdBy
            );

            if (result > 0)
            {
                return (true, "Product configuration saved successfully.");
            }

            return (false, "Product configuration save failed.");
        }

        public async Task<(bool Success, string Message)> DeleteByProductAsync(
            int productId,
            string deletedBy)
        {
            if (productId <= 0)
            {
                return (false, "Invalid product configuration.");
            }

            int result = await _repository.DeleteJsonByProductAsync(productId, deletedBy);

            if (result > 0)
            {
                return (true, "Product configuration deleted successfully.");
            }

            return (false, "Product configuration delete failed.");
        }

        private string ValidateNodes(
            List<ProductConfigurationNodeRequest> nodes,
            bool isRootLevel)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return "Please add at least one structure.";
            }

            foreach (ProductConfigurationNodeRequest node in nodes)
            {
                if (node == null)
                {
                    return "Invalid structure found.";
                }

                if (string.IsNullOrWhiteSpace(node.NodeName))
                {
                    return "Node name is required.";
                }

                string nodeName = node.NodeName.Trim();

                if (nodeName.Length > 50)
                {
                    return "Node name cannot be more than 50 characters.";
                }

                if (isRootLevel)
                {
                    if (node.InputType != "Text" && node.InputType != "Date")
                    {
                        return "Parent node input type must be Text or Date.";
                    }
                }
                else
                {
                    if (node.InputType != "Single" && node.InputType != "Multiple")
                    {
                        return "Child node type must be Single or Multiple.";
                    }
                }

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

        private List<ProductConfiguration> ConvertJsonToTree(string json)
        {
            List<ProductConfiguration> result = new List<ProductConfiguration>();

            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            List<ProductConfigurationNodeRequest> nodes =
                JsonSerializer.Deserialize<List<ProductConfigurationNodeRequest>>(json, options);

            if (nodes == null)
            {
                return result;
            }

            foreach (ProductConfigurationNodeRequest node in nodes)
            {
                ProductConfiguration convertedNode =
                    ConvertRequestNodeToProductConfiguration(node);

                result.Add(convertedNode);
            }

            return result;
        }

        private ProductConfiguration ConvertRequestNodeToProductConfiguration(
            ProductConfigurationNodeRequest requestNode)
        {
            ProductConfiguration node = new ProductConfiguration();

            node.NodeName = requestNode.NodeName;
            node.InputType = requestNode.InputType;
            node.IsActive = true;
            node.Children = new List<ProductConfiguration>();

            if (requestNode.Children != null && requestNode.Children.Count > 0)
            {
                foreach (ProductConfigurationNodeRequest child in requestNode.Children)
                {
                    ProductConfiguration childNode =
                        ConvertRequestNodeToProductConfiguration(child);

                    node.Children.Add(childNode);
                }
            }

            return node;
        }
    }
}