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

            var products = await _productRepo.GetActiveAsync();

            ViewBag.ProductList = new SelectList(
                products,
                "ProductId",
                "ProductName",
                selectedProductId
            );

            if (selectedProductId.HasValue && selectedProductId.Value > 0)
            {
                ViewBag.SelectedProductId = selectedProductId.Value;
            }
            else
            {
                ViewBag.SelectedProductId = 0;
            }

            return View(new List<ProductConfigurationIndexItem>());
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

            if (productId.HasValue && productId.Value > 0)
            {
                ViewBag.SelectedProductId = productId.Value;
            }
            else
            {
                ViewBag.SelectedProductId = 0;
            }

            string existingJson = "[]";

            if (productId.HasValue && productId.Value > 0)
            {
                var existingNodes = await _service.GetTreeByProductIdAsync(productId.Value);

                existingJson = JsonSerializer.Serialize(
                    existingNodes,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }
                );
            }

            ViewBag.ExistingJson = existingJson;

            return PartialView("_Add");
        }
        [HttpGet]
        public async Task<IActionResult> GetNodeNameOptions(int productId)
        {
            List<string> data = await _service.GetNodeNameOptionsAsync(productId);

            return Json(data);
        }
        [HttpPost]
        public async Task<IActionResult> SaveStructure([FromBody] ProductConfigurationSaveRequest request)
        {
            try
            {
                string userName = "System";

                if (User != null &&
                    User.Identity != null &&
                    User.Identity.IsAuthenticated)
                {
                    userName = User.Identity.Name;
                }

                var result = await _service.SaveStructureAsync(request, userName);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    productId = request.ProductId
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

        public async Task<IActionResult> EditNode(int nodeId)
        {
            ProductConfiguration node = await _service.GetNodeByIdAsync(nodeId);

            if (node == null)
            {
                node = new ProductConfiguration();
            }

            return PartialView("_EditNode", node);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNode(ProductConfiguration model)
        {
            var result = await _service.UpdateNodeAsync(model);

            if (result.Success)
            {
                return Content("success|" + result.Message + "|" + model.ProductId);
            }

            ViewBag.Error = result.Message;

            return PartialView("_EditNode", model);
        }

        public async Task<IActionResult> DeleteNode(int nodeId)
        {
            ProductConfiguration node = await _service.GetNodeByIdAsync(nodeId);

            if (node == null)
            {
                node = new ProductConfiguration();
            }

            return PartialView("_DeleteNode", node);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNodeConfirmed(int nodeId, int productId)
        {
            string userName = "System";

            if (User != null &&
                User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                userName = User.Identity.Name;
            }

            var result = await _service.DeleteNodeAsync(nodeId, userName);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Index", new { selectedProductId = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int productId)
        {
            string userName = "System";

            if (User != null &&
                User.Identity != null &&
                User.Identity.IsAuthenticated)
            {
                userName = User.Identity.Name;
            }

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
    }
}