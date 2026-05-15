
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EmployeeAccessSystem.Controllers
{
    [Authorize]
    public class ProductSetupController : Controller
    {
        private readonly IProductSetupService _service;

        public ProductSetupController(IProductSetupService service)
        {
            _service = service;
        }
        // Display product setup list
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
            var model = new ProductSetup
            {
                IsActive = true
            };

            return PartialView(model);
        }

        // Save new record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductSetup productSetup)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Create", productSetup);
            }

            var message = await _service.AddAsync(productSetup);

            if (message == "Record saved successfully.")
            {
                return Content("success|" + message);
            }

            ViewBag.Error = message;

            return PartialView("Create", productSetup);
        }

        // Load edit modal
        public async Task<IActionResult> Edit(int id)
        {
            var productSetup = await _service.GetByIdAsync(id);

            if (productSetup == null)
            {
                return NotFound();
            }

            return PartialView("Edit", productSetup);
        }

        // Update record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductSetup productSetup)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Edit", productSetup);
            }

            var message = await _service.UpdateAsync(productSetup);

            if (message == "Record updated successfully.")
            {
                return Content("success|" + message);
            }

            ViewBag.Error = message;

            return PartialView("Edit", productSetup);
        }

        // Load delete confirmation modal
        public async Task<IActionResult> Delete(int id)
        {
            var productSetup = await _service.GetByIdAsync(id);

            if (productSetup == null)
            {
                return NotFound();
            }

            return PartialView("Delete", productSetup);
        }

        // Delete record
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int productId)
        {
            var message = await _service.DeleteAsync(productId);

            if (message == "Record deleted successfully.")
            {
                return Content("success|" + message);
            }

            return Content("error|" + message);
        }
    }
}

