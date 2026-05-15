using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
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

        // Load index page with optional selected record
        public async Task<IActionResult> Index(int? productId)
        {
            await LoadProductDropdown(productId);

            List<string> headings = new List<string>();

            if (productId.HasValue && productId.Value > 0)
            {
                ViewBag.SelectedProductId = productId.Value;

                string configurationJson = await _service.GetConfigurationAsync(productId.Value);
                headings = GetHeadingsFromJson(configurationJson);

                if (headings.Count == 0)
                {
                    TempData["Error"] = "No configuration found.";
                }

                ViewBag.Headings = headings;

                IEnumerable<ProductEntryModel> data = await _service.GetByProductAsync(productId.Value);
                return View(data);
            }

            ViewBag.SelectedProductId = null;
            ViewBag.Headings = headings;

            IEnumerable<ProductEntryModel> allData = await _service.GetAllAsync();
            return View(allData);
        }

        // Load setup JSON for selected record
        [HttpGet]
        public async Task<IActionResult> GetSetup(int productId)
        {
            string setupJson = await _service.GetSetupAsync(productId);
            return Json(setupJson);
        }

        // Load saved entry details by group
        [HttpGet]
        public async Task<IActionResult> GetDetails(Guid entryGroupId)
        {
            IEnumerable<ProductEntryModel> data = await _service.GetDetailsAsync(entryGroupId);
            return Json(data);
        }

        // Load today's entries for selected record
        [HttpGet]
        public async Task<IActionResult> GetTodayByProduct(int productId)
        {
            if (productId <= 0)
            {
                return Json(new List<ProductEntryModel>());
            }

            IEnumerable<ProductEntryModel> data = await _service.GetByProductAsync(productId);
            return Json(data);
        }

        // Save dynamic entry values
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ProductEntrySaveRequest request)
        {
            string createdBy = GetLoginUser();
            string message = await _service.SaveAsync(request, createdBy);

            if (message == "Entry saved successfully.")
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            int selectedId = request != null ? request.ProductId : 0;
            return RedirectToAction("Index", new { productId = selectedId });
        }

        // Soft delete one entry group
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid entryGroupId, int productId)
        {
            string deletedBy = GetLoginUser();
            string message = await _service.DeleteAsync(entryGroupId, deletedBy);

            if (message == "Entry deleted successfully.")
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction("Index", new { productId = productId });
        }

        // Extract dynamic heading labels from configuration JSON
        private List<string> GetHeadingsFromJson(string json)
        {
            List<string> headings = new List<string>();

            if (string.IsNullOrWhiteSpace(json))
            {
                return headings;
            }

            try
            {
                JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("structure", out JsonElement structure))
                {
                    AddHeadingLabels(structure, headings);
                }
                else if (root.TryGetProperty("Structure", out JsonElement capitalStructure))
                {
                    AddHeadingLabels(capitalStructure, headings);
                }
                else
                {
                    AddHeadingLabels(root, headings);
                }
            }
            catch
            {
                headings.Clear();
            }

            return headings;
        }

        // Recursively collect heading labels
        private void AddHeadingLabels(JsonElement element, List<string> headings)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in element.EnumerateArray())
                {
                    AddHeadingLabels(item, headings);
                }

                return;
            }

            if (element.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            string labelText = "";

            if (element.TryGetProperty("label", out JsonElement label))
            {
                labelText = label.GetString();
            }
            else if (element.TryGetProperty("Label", out JsonElement capitalLabel))
            {
                labelText = capitalLabel.GetString();
            }

            if (!string.IsNullOrWhiteSpace(labelText))
            {
                headings.Add(labelText.Trim());
            }

            if (element.TryGetProperty("children", out JsonElement children))
            {
                AddHeadingLabels(children, headings);
            }
            else if (element.TryGetProperty("Children", out JsonElement capitalChildren))
            {
                AddHeadingLabels(capitalChildren, headings);
            }
        }

        // Get logged-in user email/name
        private string GetLoginUser()
        {
            string userName = "";

            if (User != null)
            {
                Claim emailClaim = User.FindFirst(ClaimTypes.Email);

                if (emailClaim != null)
                {
                    userName = emailClaim.Value;
                }

                if (string.IsNullOrWhiteSpace(userName) && User.Identity != null)
                {
                    userName = User.Identity.Name;
                }
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = "Unknown User";
            }

            return userName;
        }

        // Load configured records for dropdown
        private async Task LoadProductDropdown(int? selectedProductId)
        {
            IEnumerable<ProductConfigurationIndexItem> items = await _service.GetConfiguredProductsAsync();
            List<SelectListItem> dropdownList = new List<SelectListItem>();

            dropdownList.Add(new SelectListItem
            {
                Text = "Select",
                Value = ""
            });

            foreach (ProductConfigurationIndexItem item in items)
            {
                SelectListItem option = new SelectListItem
                {
                    Text = item.ProductName,
                    Value = item.ProductId.ToString(),
                    Selected = selectedProductId.HasValue && selectedProductId.Value == item.ProductId
                };

                dropdownList.Add(option);
            }

            ViewBag.Products = dropdownList;
        }
    }
}