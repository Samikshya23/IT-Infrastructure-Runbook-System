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

            if (data == null || string.IsNullOrWhiteSpace(data.ConfigurationJson))
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

            if (request.Nodes == null)
            {
                request.Nodes = new List<ProductConfigurationNodeRequest>();
            }

            string validationMessage = ValidateNodes(request.Nodes, true);

            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return (false, validationMessage);
            }

            ProductConfigurationJsonModel jsonModel = new ProductConfigurationJsonModel();

            string productName = "";

            IEnumerable<ProductConfiguration> allProducts = await _repository.GetAllAsync();

            foreach (ProductConfiguration item in allProducts)
            {
                if (item.ProductId == request.ProductId)
                {
                    productName = item.ProductName;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(productName))
            {
                productName = "Product";
            }

            jsonModel.Product = productName;
            jsonModel.Structure = BuildJsonStructure(request.Nodes);

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = false;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            string json = JsonSerializer.Serialize(jsonModel, options);

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
            if (nodes == null)
            {
                return "";
            }

            foreach (ProductConfigurationNodeRequest node in nodes)
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
                    return "Node name is required.";
                }

                if (node.NodeName.Trim().Length > 50)
                {
                    return "Node name cannot be more than 50 characters.";
                }

                if (string.IsNullOrWhiteSpace(node.InputType))
                {
                    return "Input type is required.";
                }

                if (isRootLevel)
                {
                    if (node.InputType != "Text" && node.InputType != "Date")
                    {
                        return "Parent input type must be Text or Date.";
                    }
                }
                else
                {
                    if (node.InputType != "Single" && node.InputType != "Multiple")
                    {
                        return "Child input type must be Single or Multiple.";
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

            ProductConfigurationJsonModel model =
                JsonSerializer.Deserialize<ProductConfigurationJsonModel>(json, options);

            if (model == null || model.Structure == null)
            {
                return result;
            }

            foreach (ProductConfigurationJsonNode node in model.Structure)
            {
                ProductConfiguration convertedNode = ConvertJsonNodeToTree(node);
                result.Add(convertedNode);
            }

            return result;
        }

        private List<ProductConfigurationJsonNode> BuildJsonStructure(
            List<ProductConfigurationNodeRequest> nodes)
        {
            List<ProductConfigurationJsonNode> result =
                new List<ProductConfigurationJsonNode>();

            if (nodes == null)
            {
                return result;
            }

            foreach (ProductConfigurationNodeRequest node in nodes)
            {
                ProductConfigurationJsonNode item =
                    new ProductConfigurationJsonNode();

                item.Heading = node.Heading;
                item.Label = node.NodeName;
                item.ValueType = node.InputType;
                item.Children = BuildJsonStructure(node.Children);

                result.Add(item);
            }

            return result;
        }

        private ProductConfiguration ConvertJsonNodeToTree(
            ProductConfigurationJsonNode jsonNode)
        {
            ProductConfiguration node = new ProductConfiguration();

            node.Heading = jsonNode.Heading;
            node.NodeName = jsonNode.Label;
            node.InputType = jsonNode.ValueType;
            node.IsActive = true;
            node.Children = new List<ProductConfiguration>();

            if (jsonNode.Children != null && jsonNode.Children.Count > 0)
            {
                foreach (ProductConfigurationJsonNode child in jsonNode.Children)
                {
                    ProductConfiguration childNode = ConvertJsonNodeToTree(child);
                    node.Children.Add(childNode);
                }
            }

            return node;
        }
    }
}