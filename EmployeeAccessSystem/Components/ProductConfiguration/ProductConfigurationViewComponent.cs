using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAccessSystem.Components.FormConfiguration
{
    public class FormConfigurationViewComponent : ViewComponent
    {
        private readonly IFormConfigurationService _service;

        public FormConfigurationViewComponent(IFormConfigurationService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(int selectedCategoryId)
        {
            List<FormConfigurationIndexItem> result = new List<FormConfigurationIndexItem>();

            if (selectedCategoryId > 0)
            {
                List<FormConfigurationIndexItem> data = await _service.GetIndexAsync();

                foreach (FormConfigurationIndexItem item in data)
                {
                    if (item.CategoryId == selectedCategoryId)
                    {
                        result.Add(item);
                    }
                }
            }

            ViewBag.SelectedCategoryId = selectedCategoryId;

            return View("~/Views/Shared/Components/FormConfiguration/Default.cshtml", result);
        }
    }
}