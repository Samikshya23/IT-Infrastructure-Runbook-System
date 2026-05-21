using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAccessSystem.Controllers
{
    public class CategorySetupController : Controller
    {
        private readonly ICategorySetupService _setupService;
        private readonly ICategoryService _categoryService;
        private readonly IDropdownService _dropdownService;

        public CategorySetupController(
            ICategorySetupService setupService,
            ICategoryService categoryService,
            IDropdownService dropdownService)
        {
            _setupService = setupService;
            _categoryService = categoryService;
            _dropdownService = dropdownService;
        }

        public async Task<IActionResult> Index(int? selectedCategoryId, string successMessage)
        {
            ViewBag.SelectedCategoryId = selectedCategoryId ?? 0;
            ViewBag.SuccessMessage = successMessage;
            ViewBag.Categories = await _setupService.GetConfiguredCategoriesAsync();

            return View(new List<CategorySetup>());
        }

        public async Task<IActionResult> Add(int? categoryId, int? rootIndex)
        {
            ViewBag.Categories = await _setupService.GetConfiguredCategoriesAsync();
            ViewBag.ValueTypes = await _dropdownService.GetItemsByGroupNameAsync("ValueType");
            ViewBag.SelectedCategoryId = categoryId ?? 0;
            ViewBag.RootIndex = rootIndex ?? -1;

            return PartialView("_Add");
        }

        public async Task<IActionResult> ShowTable(int categoryId)
        {
            List<CategorySetup> tree =
                await _setupService.GetTreeByCategoryIdAsync(categoryId);

            return PartialView("_ShowTable", tree);
        }

        public async Task<IActionResult> GetRootLevels(int categoryId)
        {
            List<FormConfiguration> roots =
                await _setupService.GetRootLevelsAsync(categoryId);

            List<object> data = new List<object>();

            foreach (FormConfiguration item in roots)
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

        public async Task<IActionResult> GetChildLevels(int categoryId, int? parentConfigurationNodeId)
        {
            List<FormConfiguration> children =
                await _setupService.GetChildLevelsAsync(
                    categoryId,
                    parentConfigurationNodeId
                );

            List<object> data = new List<object>();

            foreach (FormConfiguration item in children)
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

        public async Task<IActionResult> GetRootForEdit(int categoryId, int rootIndex)
        {
            var result =
                await _setupService.GetRootForEditAsync(
                    categoryId,
                    rootIndex
                );

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(
            [FromBody] CategorySetupSaveRequest request)
        {
            if (request == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request."
                });
            }

            if (request.RootIndex >= 0)
            {
                var editResult =
                    await _setupService.SaveRootDataAsync(
                        request,
                        GetCurrentUser()
                    );

                return Json(new
                {
                    success = editResult.Success,
                    message = editResult.Message,
                    categoryId = request.CategoryId
                });
            }

            var result =
                await _setupService.SaveDataAsync(
                    request,
                    GetCurrentUser()
                );

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                categoryId = request.CategoryId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int categoryId, int rootIndex)
        {
            var result =
                await _setupService.DeleteRootAsync(
                    categoryId,
                    rootIndex,
                    GetCurrentUser()
                );

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(
                "Index",
                new
                {
                    selectedCategoryId = categoryId
                }
            );
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

            return "Unknown User";
        }
    }
}