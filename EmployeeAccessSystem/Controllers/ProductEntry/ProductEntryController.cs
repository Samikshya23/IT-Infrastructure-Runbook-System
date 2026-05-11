using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeAccessSystem.Controllers
{
    public class ProductEntryController : Controller
    {
        private readonly IProductEntryService _service;

        public ProductEntryController(IProductEntryService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int? productId)
        {
            await LoadProductDropdown(productId);

            if (productId.HasValue && productId.Value > 0)
            {
                ViewBag.SelectedProductId = productId.Value;

                IEnumerable<ProductEntryModel> data =
                    await _service.GetByProductAsync(productId.Value);

                return View(data);
            }

            ViewBag.SelectedProductId = null;

            IEnumerable<ProductEntryModel> allData =
                await _service.GetAllAsync();

            return View(allData);
        }

        [HttpGet]
        public async Task<IActionResult> GetSetupJson(int productId)
        {
            string setupJson =
                await _service.GetSetupJsonByProductAsync(productId);

            return Json(setupJson);
        }

        [HttpGet]
        public async Task<IActionResult> GetByGroup(Guid entryGroupId)
        {
            IEnumerable<ProductEntryModel> data =
                await _service.GetByGroupAsync(entryGroupId);

            return Json(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ProductEntrySaveRequest request)
        {
            string createdBy = "System";

            if (User != null &&
                User.Identity != null &&
                !string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                createdBy = User.Identity.Name;
            }

            string message =
                await _service.SaveEntryAsync(request, createdBy);

            if (message == "Product entry saved successfully.")
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction(
                "Index",
                new { productId = request.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid entryGroupId, int productId)
        {
            string deletedBy = "System";

            if (User != null &&
                User.Identity != null &&
                !string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                deletedBy = User.Identity.Name;
            }

            string message =
                await _service.DeleteGroupAsync(entryGroupId, deletedBy);

            TempData["Success"] = message;

            return RedirectToAction(
                "Index",
                new { productId = productId });
        }

        private async Task LoadProductDropdown(int? selectedProductId)
        {
            IEnumerable<ProductConfigurationIndexItem> products =
                await _service.GetConfiguredProductsAsync();

            List<SelectListItem> productList =
                new List<SelectListItem>();

            productList.Add(new SelectListItem
            {
                Text = "Select Product",
                Value = ""
            });

            foreach (ProductConfigurationIndexItem product in products)
            {
                SelectListItem item = new SelectListItem();

                item.Text = product.ProductName;
                item.Value = product.ProductId.ToString();

                if (selectedProductId.HasValue &&
                    selectedProductId.Value == product.ProductId)
                {
                    item.Selected = true;
                }

                productList.Add(item);
            }

            ViewBag.Products = productList;
        }
    }
}