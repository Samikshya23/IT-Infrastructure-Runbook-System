using System.Collections.Generic;
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
            IEnumerable<ProductConfiguration> data = await _repository.GetAllAsync();

            List<ProductConfiguration> flatList = new List<ProductConfiguration>();

            foreach (ProductConfiguration item in data)
            {
                flatList.Add(item);
            }

            List<ProductConfigurationIndexItem> result = new List<ProductConfigurationIndexItem>();

            foreach (ProductConfiguration item in flatList)
            {
                bool productExists = false;

                foreach (ProductConfigurationIndexItem existing in result)
                {
                    if (existing.ProductId == item.ProductId)
                    {
                        productExists = true;
                    }
                }

                if (!productExists)
                {
                    ProductConfigurationIndexItem indexItem = new ProductConfigurationIndexItem();

                    indexItem.ProductId = item.ProductId;
                    indexItem.ProductName = item.ProductName;
                    indexItem.Nodes = new List<ProductConfiguration>();

                    List<ProductConfiguration> productNodes = new List<ProductConfiguration>();

                    foreach (ProductConfiguration node in flatList)
                    {
                        if (node.ProductId == item.ProductId)
                        {
                            productNodes.Add(node);
                        }
                    }

                    indexItem.Nodes = BuildTree(productNodes);

                    result.Add(indexItem);
                }
            }

            return result;
        }

        public async Task<List<ProductConfiguration>> GetTreeByProductIdAsync(int productId)
        {
            IEnumerable<ProductConfiguration> data = await _repository.GetByProductIdAsync(productId);

            List<ProductConfiguration> flatList = new List<ProductConfiguration>();

            foreach (ProductConfiguration item in data)
            {
                flatList.Add(item);
            }

            return BuildTree(flatList);
        }

        public async Task<ProductConfiguration> GetNodeByIdAsync(int nodeId)
        {
            if (nodeId <= 0)
            {
                return null;
            }

            return await _repository.GetNodeByIdAsync(nodeId);
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
                return (false, "Please add at least one node.");
            }

            await _repository.DeleteByProductAsync(request.ProductId, createdBy);

            int sortOrder = 1;

            foreach (ProductConfigurationNodeRequest node in request.Nodes)
            {
                await SaveNodeRecursive(
                    request.ProductId,
                    null,
                    node,
                    sortOrder,
                    createdBy
                );

                sortOrder++;
            }

            return (true, "Product configuration saved successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateNodeAsync(ProductConfiguration model)
        {
            if (model == null)
            {
                return (false, "Invalid node.");
            }

            if (model.NodeId <= 0)
            {
                return (false, "Invalid node.");
            }

            if (string.IsNullOrWhiteSpace(model.NodeName))
            {
                return (false, "Node name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.NodeType))
            {
                model.NodeType = "Block";
            }

            if (string.IsNullOrWhiteSpace(model.InputType))
            {
                model.InputType = "None";
            }

            int result = await _repository.UpdateNodeAsync(model);

            if (result > 0)
            {
                return (true, "Node updated successfully.");
            }

            return (false, "Node update failed.");
        }

        public async Task<(bool Success, string Message)> DeleteByProductAsync(
            int productId,
            string deletedBy)
        {
            if (productId <= 0)
            {
                return (false, "Invalid product configuration.");
            }

            int result = await _repository.DeleteByProductAsync(productId, deletedBy);

            if (result > 0)
            {
                return (true, "Product configuration deleted successfully.");
            }

            return (false, "Product configuration delete failed.");
        }

        public async Task<(bool Success, string Message)> DeleteNodeAsync(
            int nodeId,
            string deletedBy)
        {
            if (nodeId <= 0)
            {
                return (false, "Invalid node.");
            }

            int result = await _repository.DeleteNodeAsync(nodeId, deletedBy);

            if (result > 0)
            {
                return (true, "Node deleted successfully.");
            }

            return (false, "Node delete failed.");
        }

        private async Task SaveNodeRecursive(
            int productId,
            int? parentNodeId,
            ProductConfigurationNodeRequest requestNode,
            int sortOrder,
            string createdBy)
        {
            if (requestNode == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(requestNode.NodeName))
            {
                return;
            }

            string nodeType = requestNode.NodeType;

            if (string.IsNullOrWhiteSpace(nodeType))
            {
                nodeType = "Block";
            }

            string inputType = requestNode.InputType;

            if (string.IsNullOrWhiteSpace(inputType))
            {
                inputType = "None";
            }

            ProductConfiguration model = new ProductConfiguration();

            model.ProductId = productId;
            model.ParentNodeId = parentNodeId;
            model.NodeName = requestNode.NodeName.Trim();
            model.NodeType = nodeType;
            model.InputType = inputType;
            model.SortOrder = sortOrder;
            model.IsActive = true;
            model.CreatedBy = createdBy;

            int newNodeId = await _repository.AddAsync(model);

            if (requestNode.Children != null && requestNode.Children.Count > 0)
            {
                int childSort = 1;

                foreach (ProductConfigurationNodeRequest child in requestNode.Children)
                {
                    await SaveNodeRecursive(
                        productId,
                        newNodeId,
                        child,
                        childSort,
                        createdBy
                    );

                    childSort++;
                }
            }
        }

        private List<ProductConfiguration> BuildTree(List<ProductConfiguration> flatList)
        {
            List<ProductConfiguration> roots = new List<ProductConfiguration>();

            foreach (ProductConfiguration item in flatList)
            {
                item.Children = new List<ProductConfiguration>();
            }

            foreach (ProductConfiguration item in flatList)
            {
                if (item.ParentNodeId == null)
                {
                    roots.Add(item);
                }
                else
                {
                    ProductConfiguration parent = null;

                    foreach (ProductConfiguration possibleParent in flatList)
                    {
                        if (possibleParent.NodeId == item.ParentNodeId.Value)
                        {
                            parent = possibleParent;
                        }
                    }

                    if (parent != null)
                    {
                        parent.Children.Add(item);
                    }
                }
            }

            SortNodes(roots);

            return roots;
        }

        private void SortNodes(List<ProductConfiguration> nodes)
        {
            nodes.Sort(delegate (ProductConfiguration first, ProductConfiguration second)
            {
                return first.SortOrder.CompareTo(second.SortOrder);
            });

            foreach (ProductConfiguration node in nodes)
            {
                if (node.Children != null && node.Children.Count > 0)
                {
                    SortNodes(node.Children);
                }
            }
        }
    }
}