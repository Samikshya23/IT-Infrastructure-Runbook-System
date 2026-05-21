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
    public class FormConfigurationController : Controller
    {
        private readonly IFormConfigurationService _service;
        private readonly ICategoryRepositories _categoryRepo;

        public FormConfigurationController(IFormConfigurationService service, ICategoryRepositories categoryRepo)
        {
            _service = service;
            _categoryRepo = categoryRepo;
        }

        // Load configuration list
        public async Task<IActionResult> Index(int? selectedCategoryId, string successMessage, string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                TempData["Success"] = successMessage;
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                TempData["Error"] = errorMessage;
            }

            List<FormConfigurationIndexItem> model = await _service.GetIndexAsync();

            ViewBag.SelectedCategoryId = selectedCategoryId ?? 0;

            return View(model);
        }

        // Load add/edit modal
        public async Task<IActionResult> Add(int? categoryId)
        {
            var categories = await _categoryRepo.GetActiveAsync();

            ViewBag.CategoryList = new SelectList(categories, "CategoryId", "Name", categoryId);
            ViewBag.SelectedCategoryId = categoryId ?? 0;

            string existingJson = "[]";

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                List<FormConfiguration> existingNodes = await _service.GetTreeByCategoryIdAsync(categoryId.Value);

                if (existingNodes != null && existingNodes.Count > 0)
                {
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    existingJson = JsonSerializer.Serialize(existingNodes, options);
                }
            }

            ViewBag.ExistingJson = existingJson;

            return PartialView("_Add");
        }

        // Save configuration structure
        [HttpPost]
        public async Task<IActionResult> SaveStructure([FromBody] FormConfigurationSaveRequest request)
        {
            string userName = GetCurrentUserName();

            var result = await _service.SaveStructureAsync(request, userName);

            int categoryId = 0;

            if (request != null)
            {
                categoryId = request.CategoryId;
            }

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                categoryId = categoryId
            });
        }

        // Delete configuration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int categoryId)
        {
            string userName = GetCurrentUserName();

            var result = await _service.DeleteByCategoryAsync(categoryId, userName);

            return RedirectToAction("Index", new
            {
                successMessage = result.Success ? result.Message : "",
                errorMessage = result.Success ? "" : result.Message
            });
        }

        // Get current logged-in user
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