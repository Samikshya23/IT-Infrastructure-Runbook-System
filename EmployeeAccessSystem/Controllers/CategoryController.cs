using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EmployeeAccessSystem.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        // Display list
        public async Task<IActionResult> Index(string successMessage, string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                TempData["Success"] = successMessage;
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                TempData["Error"] = errorMessage;
            }

            var data = await _service.GetAllAsync();

            return View(data);
        }

        // Activate or deactivate record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var message = await _service.ToggleAsync(id);

            if (message == "Status updated successfully.")
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Load create modal
        public IActionResult Create()
        {
            var model = new Category
            {
                IsActive = true
            };

            return PartialView(model);
        }

        // Save new record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Create", category);
            }

            var message = await _service.AddAsync(category);

            if (message == "Added successfully.")
            {
                return Content("success|" + message);
            }

            ViewBag.Error = message;

            return PartialView("Create", category);
        }

        // Load edit modal
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _service.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return PartialView("Edit", category);
        }

        // Update record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Edit", category);
            }

            var message = await _service.UpdateAsync(category);

            if (message == "Updated successfully.")
            {
                return Content("success|" + message);
            }

            ViewBag.Error = message;

            return PartialView("Edit", category);
        }

        // Load delete confirmation modal
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _service.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return PartialView("Delete", category);
        }

        // Delete record
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int categoryId)
        {
            var message = await _service.DeleteAsync(categoryId);

            if (message == "Deleted successfully.")
            {
                return Content("success|" + message);
            }

            return Content("error|" + message);
        }
    }
}