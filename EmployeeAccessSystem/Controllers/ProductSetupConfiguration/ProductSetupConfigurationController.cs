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

        public async Task<IActionResult> Index(
            int? selectedProductId,
            string successMessage)
        {
            ViewBag.SelectedProductId = selectedProductId ?? 0;
            ViewBag.SuccessMessage = successMessage;
            ViewBag.Products = await _productSetupService.GetAllAsync();

            return View(new List<ProductSetupConfiguration>());
        }

        public async Task<IActionResult> Add(int? productId)
        {
            var products = await _productSetupService.GetAllAsync();

            ViewBag.Products = products;
            ViewBag.SelectedProductId = productId ?? 0;

            return PartialView("_Add");
        }

        public async Task<IActionResult> ShowTable(int productId)
        {
            List<ProductSetupConfiguration> tree =
                await _setupConfigurationService.GetTreeByProductIdAsync(productId);

            return PartialView("_ShowTable", tree);
        }

        public async Task<IActionResult> GetRootLevels(int productId)
        {
            List<ProductConfiguration> roots =
                await _setupConfigurationService.GetRootLevelsAsync(productId);

            List<object> data = new List<object>();

            foreach (ProductConfiguration item in roots)
            {
                data.Add(new
                {
                    id = item.NodeId,
                    heading = item.Heading,
                    name = item.NodeName,
                    inputType = item.InputType
                });
            }

            return Json(new
            {
                success = true,
                data = data
            });
        }

        public async Task<IActionResult> GetChildLevels(
            int productId,
            int? parentConfigurationNodeId)
        {
            List<ProductConfiguration> children =
                await _setupConfigurationService.GetChildLevelsAsync(
                    productId,
                    parentConfigurationNodeId);

            List<object> data = new List<object>();

            foreach (ProductConfiguration item in children)
            {
                data.Add(new
                {
                    id = item.NodeId,
                    heading = item.Heading,
                    name = item.NodeName,
                    inputType = item.InputType
                });
            }

            return Json(new
            {
                success = true,
                data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(
            [FromBody] ProductSetupConfigurationSaveRequest request)
        {
            if (request == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request."
                });
            }

            var result = await _setupConfigurationService.SaveDataAsync(
                request,
                GetCurrentUser());

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                productId = request.ProductId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int productId)
        {
            var result = await _setupConfigurationService.DeleteByProductAsync(
                productId,
                GetCurrentUser());

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

        private string GetCurrentUser()
        {
            string name = User.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (User.Identity != null &&
                !string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                return User.Identity.Name;
            }

            string email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            return "User";
        }
    }
}