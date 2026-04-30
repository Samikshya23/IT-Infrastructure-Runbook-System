using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class ProductSetupConfigurationService : IProductSetupConfigurationService
    {
        private readonly IProductSetupConfigurationRepository _repository;

        public ProductSetupConfigurationService(IProductSetupConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductSetupConfiguration>> GetTreeByProductIdAsync(int productId)
        {
            var data = await _repository.GetByProductIdAsync(productId);
            var nodes = data.ToList();

            foreach (var node in nodes)
            {
                node.Children = new List<ProductSetupConfiguration>();
            }

            var rootNodes = new List<ProductSetupConfiguration>();

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

        public async Task<ProductSetupConfiguration> GetNodeByIdAsync(int nodeId)
        {
            return await _repository.GetNodeByIdAsync(nodeId);
        }

        public async Task<List<string>> GetNodeNameOptionsAsync(int productId)
        {
            var data = await _repository.GetNodeNameOptionsAsync(productId);
            return data.ToList();
        }

        public async Task<(bool Success, string Message)> AddAsync(ProductSetupConfiguration model, string createdBy)
        {
            if (model.ProductId <= 0)
            {
                return (false, "Please select product.");
            }

            if (string.IsNullOrWhiteSpace(model.NodeName))
            {
                return (false, "Node name is required.");
            }

            int duplicateCount = await _repository.CheckDuplicateNodeAsync(
                model.ProductId,
                model.ParentNodeId,
                model.NodeName.Trim()
            );

            if (duplicateCount > 0)
            {
                return (false, "This node already exists under the selected parent.");
            }

            model.NodeName = model.NodeName.Trim();
            model.IsActive = true;
            model.CreatedBy = createdBy;

            int result = await _repository.AddAsync(model);

            if (result > 0)
            {
                return (true, "Product setup configuration added successfully.");
            }

            return (false, "Failed to add product setup configuration.");
        }

        public async Task<(bool Success, string Message)> UpdateNodeAsync(ProductSetupConfiguration model, string modifiedBy)
        {
            if (model.NodeId <= 0)
            {
                return (false, "Invalid node.");
            }

            if (string.IsNullOrWhiteSpace(model.NodeName))
            {
                return (false, "Node name is required.");
            }

            model.NodeName = model.NodeName.Trim();
            model.ModifiedBy = modifiedBy;

            int result = await _repository.UpdateNodeAsync(model);

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

            int result = await _repository.DeleteNodeAsync(nodeId, deletedBy);

            if (result > 0)
            {
                return (true, "Product setup configuration deleted successfully.");
            }

            return (false, "Failed to delete product setup configuration.");
        }
    }
}