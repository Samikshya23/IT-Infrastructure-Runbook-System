using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAccessSystem.Controllers
{
    public class ProductSetupConfigurationController : Controller
    {
        private readonly IProductSetupConfigurationService _setupConfigurationService;
        private readonly IProductSetupService _productSetupService;

        public ProductSetupConfigurationController(
            IProductSetupConfigurationService setupConfigurationService,
            IProductSetupService productSetupService)
        {
            _setupConfigurationService = setupConfigurationService;
            _productSetupService = productSetupService;
        }

        public async Task<IActionResult> Index(int? productId)
        {
            var products = await _productSetupService.GetAllAsync();
            ViewBag.Products = products;
            ViewBag.SelectedProductId = productId;

            if (productId != null && productId.Value > 0)
            {
                var tree = await _setupConfigurationService.GetTreeByProductIdAsync(productId.Value);
                return View(tree);
            }

            return View(new List<ProductSetupConfiguration>());
        }

        public async Task<IActionResult> Add(int productId, int? parentNodeId)
        {
            var model = await _setupConfigurationService.PrepareAddAsync(productId, parentNodeId);

            if (model == null)
            {
                TempData["Error"] = "No next configuration level found. Please check Product Configuration.";
                return RedirectToAction("Index", new { productId = productId });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(ProductSetupConfiguration model)
        {
            var result = await _setupConfigurationService.AddAsync(model, GetCurrentUser());

            TempData[result.Success ? "Success" : "Error"] = result.Message;

            return RedirectToAction("Index", new { productId = model.ProductId });
        }

        public async Task<IActionResult> Edit(int nodeId)
        {
            var model = await _setupConfigurationService.PrepareEditAsync(nodeId);

            if (model == null)
            {
                TempData["Error"] = "Configuration not found.";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductSetupConfiguration model)
        {
            var result = await _setupConfigurationService.UpdateNodeAsync(model, GetCurrentUser());

            TempData[result.Success ? "Success" : "Error"] = result.Message;

            return RedirectToAction("Index", new { productId = model.ProductId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int nodeId, int productId)
        {
            var result = await _setupConfigurationService.DeleteNodeAsync(nodeId, GetCurrentUser());

            TempData[result.Success ? "Success" : "Error"] = result.Message;

            return RedirectToAction("Index", new { productId = productId });
        }

        private string GetCurrentUser()
        {
            var name = User.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (User.Identity != null && !string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                return User.Identity.Name;
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            return "User";
        }
    }
}