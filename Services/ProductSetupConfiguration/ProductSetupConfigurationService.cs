using System.Collections.Generic;
using System.Linq;
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
            var data = await _setupRepository.GetByProductIdAsync(productId);
            var nodes = data.ToList();

            foreach (var node in nodes)
            {
                node.Children = new List<ProductSetupConfiguration>();
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
                    ProductSetupConfiguration parentNode = null;

                    foreach (var possibleParent in nodes)
                    {
                        if (possibleParent.NodeId == node.ParentNodeId.Value)
                        {
                            parentNode = possibleParent;
                            break;
                        }
                    }

                    if (parentNode != null)
                    {
                        parentNode.Children.Add(node);
                    }
                }
            }

            return rootNodes;
        }

        public async Task<ProductSetupConfiguration> PrepareAddAsync(int productId, int? parentNodeId)
        {
            ProductConfiguration nextConfigurationNode = null;

            var configurationNodes = await _configurationRepository.GetByProductIdAsync(productId);
            var configurationList = configurationNodes.ToList();

            if (parentNodeId == null)
            {
                foreach (var item in configurationList)
                {
                    if (item.ParentNodeId == null)
                    {
                        nextConfigurationNode = item;
                        break;
                    }
                }
            }
            else
            {
                var parentSetupNode = await _setupRepository.GetNodeByIdAsync(parentNodeId.Value);

                if (parentSetupNode != null)
                {
                    foreach (var item in configurationList)
                    {
                        if (item.ParentNodeId == parentSetupNode.ConfigurationNodeId)
                        {
                            nextConfigurationNode = item;
                            break;
                        }
                    }
                }
            }

            if (nextConfigurationNode == null)
            {
                return null;
            }

            ProductSetupConfiguration model = new ProductSetupConfiguration();

            model.ProductId = productId;
            model.ParentNodeId = parentNodeId;
            model.ConfigurationNodeId = nextConfigurationNode.NodeId;
            model.ConfigurationNodeName = nextConfigurationNode.NodeName;
            model.InputType = nextConfigurationNode.InputType;

            return model;
        }

        public async Task<ProductSetupConfiguration> PrepareEditAsync(int nodeId)
        {
            var node = await _setupRepository.GetNodeByIdAsync(nodeId);
            return node;
        }

        public async Task<(bool Success, string Message)> AddAsync(ProductSetupConfiguration model, string createdBy)
        {
            if (model.ProductId <= 0)
            {
                return (false, "Please select product.");
            }

            if (model.ConfigurationNodeId <= 0)
            {
                return (false, "Configuration level is missing.");
            }

            if (string.IsNullOrWhiteSpace(model.NodeValue))
            {
                return (false, "Value is required.");
            }

            int duplicateCount = await _setupRepository.CheckDuplicateNodeAsync(
                model.ProductId,
                model.ConfigurationNodeId,
                model.ParentNodeId,
                model.NodeValue.Trim()
            );

            if (duplicateCount > 0)
            {
                return (false, "This value already exists.");
            }

            model.NodeValue = model.NodeValue.Trim();
            model.IsActive = true;
            model.CreatedBy = createdBy;

            int result = await _setupRepository.AddAsync(model);

            if (result > 0)
            {
                return (true, "Product setup configuration saved successfully.");
            }

            return (false, "Failed to save product setup configuration.");
        }

        public async Task<(bool Success, string Message)> UpdateNodeAsync(ProductSetupConfiguration model, string modifiedBy)
        {
            if (model.NodeId <= 0)
            {
                return (false, "Invalid node.");
            }

            if (string.IsNullOrWhiteSpace(model.NodeValue))
            {
                return (false, "Value is required.");
            }

            model.NodeValue = model.NodeValue.Trim();
            model.ModifiedBy = modifiedBy;

            int result = await _setupRepository.UpdateNodeAsync(model);

            if (result > 0)
            {
                return (true, "Product setup configuration updated successfully.");
            }

            return (false, "Failed to update product setup configuration.");
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