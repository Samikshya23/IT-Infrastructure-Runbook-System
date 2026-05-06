using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeAccessSystem.Controllers
{
    public class ProductConfigurationController : Controller
    {
        private readonly IProductConfigurationService _service;
        private readonly IProductSetupRepositories _productRepo;

        public ProductConfigurationController(
            IProductConfigurationService service,
            IProductSetupRepositories productRepo)
        {
            _service = service;
            _productRepo = productRepo;
        }

        public async Task<IActionResult> Index(
            int? selectedProductId,
            string successMessage,
            string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                TempData["Success"] = successMessage;
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                TempData["Error"] = errorMessage;
            }

            var products = await _productRepo.GetActiveAsync();

            ViewBag.ProductList = new SelectList(
                products,
                "ProductId",
                "ProductName",
                selectedProductId
            );

            ViewBag.SelectedProductId = selectedProductId ?? 0;

            List<ProductConfigurationIndexItem> model =
                new List<ProductConfigurationIndexItem>();

            if (selectedProductId.HasValue && selectedProductId.Value > 0)
            {
                List<ProductConfiguration> nodes =
                    await _service.GetTreeByProductIdAsync(selectedProductId.Value);

                if (nodes != null && nodes.Count > 0)
                {
                    ProductConfigurationIndexItem item =
                        new ProductConfigurationIndexItem();

                    item.ProductId = selectedProductId.Value;
                    item.ProductName = "";

                    foreach (var product in products)
                    {
                        if (product.ProductId == selectedProductId.Value)
                        {
                            item.ProductName = product.ProductName;
                            break;
                        }
                    }

                    item.Nodes = nodes;
                    model.Add(item);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Add(int? productId)
        {
            var products = await _productRepo.GetActiveAsync();

            ViewBag.ProductList = new SelectList(
                products,
                "ProductId",
                "ProductName",
                productId
            );

            ViewBag.SelectedProductId = productId ?? 0;

            string existingJson = "[]";

            if (productId.HasValue && productId.Value > 0)
            {
                List<ProductConfiguration> existingNodes =
                    await _service.GetTreeByProductIdAsync(productId.Value);

                if (existingNodes != null && existingNodes.Count > 0)
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                    existingJson = JsonSerializer.Serialize(existingNodes, options);
                }
            }

            ViewBag.ExistingJson = existingJson;

            return PartialView("_Add");
        }

        [HttpPost]
        public async Task<IActionResult> SaveStructure(
            [FromBody] ProductConfigurationSaveRequest request)
        {
            try
            {
                string userName = GetCurrentUserName();

                var result = await _service.SaveStructureAsync(request, userName);

                int productId = 0;

                if (request != null)
                {
                    productId = request.ProductId;
                }

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    productId = productId
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int productId)
        {
            string userName = GetCurrentUserName();

            var result = await _service.DeleteByProductAsync(productId, userName);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Index");
        }

        private string GetCurrentUserName()
        {
            string userName = "System";

            if (User != null &&
                User.Identity != null &&
                User.Identity.IsAuthenticated &&
                !string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                userName = User.Identity.Name;
            }

            return userName;
        }
    }
}