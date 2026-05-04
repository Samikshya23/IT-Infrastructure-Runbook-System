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

        public async Task<IActionResult> Index(int? selectedProductId, string successMessage)
        {
            ViewBag.SelectedProductId = selectedProductId ?? 0;
            ViewBag.SuccessMessage = successMessage;

            if (selectedProductId != null && selectedProductId.Value > 0)
            {
                List<ProductSetupConfiguration> tree =
                    await _setupConfigurationService.GetTreeByProductIdAsync(selectedProductId.Value);

                return View(tree);
            }

            return View(new List<ProductSetupConfiguration>());
        }

        public async Task<IActionResult> Add(int? productId)
        {
            var products = await _productSetupService.GetAllAsync();

            ViewBag.Products = products;
            ViewBag.SelectedProductId = productId ?? 0;

            return PartialView("_Add");
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

        public async Task<IActionResult> GetChildLevels(int productId, int? parentConfigurationNodeId)
        {
            List<ProductConfiguration> children =
                await _setupConfigurationService.GetChildLevelsAsync(productId, parentConfigurationNodeId);

            List<object> data = new List<object>();

            foreach (ProductConfiguration item in children)
            {
                data.Add(new
                {
                    id = item.NodeId,
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
        public async Task<IActionResult> SaveData([FromBody] ProductSetupConfigurationSaveRequest request)
        {
            if (request == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request."
                });
            }

            var result = await _setupConfigurationService.SaveDataAsync(request, GetCurrentUser());

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        public async Task<IActionResult> AddChild(int productId, int? parentNodeId)
        {
            ProductSetupConfiguration model =
                await _setupConfigurationService.PrepareAddAsync(productId, parentNodeId);

            if (model == null)
            {
                return Content("Cannot add child. Product Configuration is missing.");
            }

            return PartialView("_AddChild", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddChild(ProductSetupConfiguration model)
        {
            var result = await _setupConfigurationService.AddAsync(model, GetCurrentUser());

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Index", new { selectedProductId = model.ProductId });
        }

        public async Task<IActionResult> EditNode(int nodeId)
        {
            ProductSetupConfiguration model =
                await _setupConfigurationService.PrepareEditAsync(nodeId);

            if (model == null)
            {
                return Content("Item not found.");
            }

            return PartialView("_EditNode", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditNode(ProductSetupConfiguration model)
        {
            var result = await _setupConfigurationService.UpdateNodeAsync(model, GetCurrentUser());

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Index", new { selectedProductId = model.ProductId });
        }

        public async Task<IActionResult> DeleteNode(int nodeId)
        {
            ProductSetupConfiguration model =
                await _setupConfigurationService.PrepareEditAsync(nodeId);

            if (model == null)
            {
                return Content("Item not found.");
            }

            return PartialView("_DeleteNode", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int nodeId, int productId)
        {
            var result = await _setupConfigurationService.DeleteNodeAsync(nodeId, GetCurrentUser());

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

        private string GetCurrentUser()
        {
            string name = User.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (User.Identity != null && !string.IsNullOrWhiteSpace(User.Identity.Name))
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