using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAccessSystem.Controllers
{
    public class ProductStructureController : Controller
    {
        private readonly IProductConfigurationService _configurationService;
        private readonly IProductSetupRepositories _productRepo;

        public ProductStructureController(
            IProductConfigurationService configurationService,
            IProductSetupRepositories productRepo)
        {
            _configurationService = configurationService;
            _productRepo = productRepo;
        }

        public async Task<IActionResult> Index(int? selectedProductId, string successMessage, string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                TempData["Success"] = successMessage;
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                TempData["Error"] = errorMessage;
            }

            ProductStructurePageViewModel model = new ProductStructurePageViewModel();

            var products = await _productRepo.GetActiveAsync();

            foreach (var product in products)
            {
                ProductDropDownItem item = new ProductDropDownItem();
                item.ProductId = product.ProductId;
                item.ProductName = product.ProductName;

                model.ProductList.Add(item);

                if (selectedProductId.HasValue && selectedProductId.Value == product.ProductId)
                {
                    model.ProductName = product.ProductName;
                }
            }

            if (!selectedProductId.HasValue || selectedProductId.Value <= 0)
            {
                model.SelectedProductId = 0;
                return View(model);
            }

            model.SelectedProductId = selectedProductId.Value;

            List<ProductConfiguration> nodes =
                await _configurationService.GetTreeByProductIdAsync(selectedProductId.Value);

            int sn = 1;

            foreach (ProductConfiguration node in nodes)
            {
                ProductStructureRowViewModel row = new ProductStructureRowViewModel();
                row.SN = sn;
                row.NodeId = node.NodeId;
                row.ProductId = node.ProductId;
                row.NodeName = node.NodeName;

                AddChildren(node, row.Children, 0);

                if (row.Children.Count == 0)
                {
                    ProductStructureChildViewModel child = new ProductStructureChildViewModel();
                    child.NodeId = node.NodeId;
                    child.ProductId = node.ProductId;
                    child.ItemName = "-";
                    child.Level = 0;

                    row.Children.Add(child);
                }

                model.Rows.Add(row);
                sn++;
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            ProductConfiguration node = await _configurationService.GetNodeByIdAsync(id);

            if (node == null)
            {
                node = new ProductConfiguration();
            }

            return PartialView("Delete", node);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int nodeId, int productId)
        {
            string userName = "System";

            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                userName = User.Identity.Name;
            }

            var result = await _configurationService.DeleteNodeAsync(nodeId, userName);

            if (result.Success)
            {
                return Content("success|" + result.Message + "|" + productId);
            }

            return Content("error|" + result.Message + "|" + productId);
        }

        private void AddChildren(
            ProductConfiguration node,
            List<ProductStructureChildViewModel> children,
            int level)
        {
            if (node.Children == null || node.Children.Count == 0)
            {
                return;
            }

            foreach (ProductConfiguration childNode in node.Children)
            {
                ProductStructureChildViewModel child = new ProductStructureChildViewModel();
                child.NodeId = childNode.NodeId;
                child.ProductId = childNode.ProductId;
                child.ItemName = childNode.NodeName;
                child.Level = level;

                children.Add(child);

                if (childNode.Children != null && childNode.Children.Count > 0)
                {
                    AddChildren(childNode, children, level + 1);
                }
            }
        }
    }
}