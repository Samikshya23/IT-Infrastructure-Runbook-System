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

        #region Public Methods

        // Load configuration list for index page
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

                // Show only configured products
                if (string.IsNullOrWhiteSpace(item.ConfigurationJson))
                {
                    continue;
                }

                ProductConfigurationIndexItem indexItem = new ProductConfigurationIndexItem();

                indexItem.ProductId = item.ProductId;
                indexItem.ProductName = item.ProductName;

                // Convert saved JSON into hierarchy tree
                indexItem.Nodes = ConvertJsonToTree(item.ConfigurationJson);

                result.Add(indexItem);
            }

            return result;
        }

        // Load saved hierarchy structure by product
        public async Task<List<ProductConfiguration>> GetTreeByProductIdAsync(int productId)
        {
            if (productId <= 0)
            {
                return new List<ProductConfiguration>();
            }

            ProductConfiguration data = await _repository.GetJsonByProductIdAsync(productId);

            if (data == null ||
                string.IsNullOrWhiteSpace(data.ConfigurationJson))
            {
                return new List<ProductConfiguration>();
            }

            return ConvertJsonToTree(data.ConfigurationJson);
        }

        // Save or update configuration structure
        public async Task<(bool Success, string Message)> SaveStructureAsync(ProductConfigurationSaveRequest request, string createdBy)
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
                return (false, "Please add configuration structure.");
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

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(jsonModel, options);

            int result = await _repository.SaveOrUpdateJsonAsync(request.ProductId, json, createdBy);

            if (result > 0)
            {
                return (true, "Record saved successfully.");
            }

            return (false, "Record save failed.");
        }

        // Delete configuration by product
        public async Task<(bool Success, string Message)> DeleteByProductAsync(int productId, string deletedBy)
        {
            if (productId <= 0)
            {
                return (false, "Invalid record.");
            }

            int result = await _repository.DeleteJsonByProductAsync(productId, deletedBy);

            if (result > 0)
            {
                return (true, "Record deleted successfully.");
            }

            return (false, "Record delete failed.");
        }

        #endregion

        #region Validation Methods

        // Validate hierarchy structure before save
        private string ValidateNodes(List<ProductConfigurationNodeRequest> nodes, bool isRootLevel)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return "Please add configuration structure.";
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
        private List<ProductConfiguration> ConvertJsonToTree(string json)
        {
            List<ProductConfiguration> result = new List<ProductConfiguration>();

            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

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

        // Build JSON structure recursively
        private List<ProductConfigurationJsonNode> BuildJsonStructure(List<ProductConfigurationNodeRequest> nodes)
        {
            List<ProductConfigurationJsonNode> result = new List<ProductConfigurationJsonNode>();

            if (nodes == null)
            {
                return result;
            }

            foreach (ProductConfigurationNodeRequest node in nodes)
            {
                ProductConfigurationJsonNode item = new ProductConfigurationJsonNode();

                item.Heading = node.Heading;
                item.Label = node.NodeName;
                item.ValueType = node.InputType;

                item.Children = BuildJsonStructure(node.Children);

                result.Add(item);
            }

            return result;
        }

        // Convert JSON node into tree node
        private ProductConfiguration ConvertJsonNodeToTree(ProductConfigurationJsonNode jsonNode)
        {
            ProductConfiguration node = new ProductConfiguration();

            node.Heading = jsonNode.Heading;
            node.NodeName = jsonNode.Label;
            node.InputType = jsonNode.ValueType;
            node.IsActive = true;

            node.Children = new List<ProductConfiguration>();

            if (jsonNode.Children != null &&
                jsonNode.Children.Count > 0)
            {
                foreach (ProductConfigurationJsonNode child in jsonNode.Children)
                {
                    ProductConfiguration childNode = ConvertJsonNodeToTree(child);

                    node.Children.Add(childNode);
                }
            }

            return node;
        }

        #endregion
    }
}