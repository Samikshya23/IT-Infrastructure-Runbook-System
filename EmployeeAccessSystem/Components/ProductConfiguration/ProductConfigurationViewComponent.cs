using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAccessSystem.Components.ProductConfiguration
{
    public class ProductConfigurationViewComponent : ViewComponent
    {
        private readonly IProductConfigurationService _service;

        public ProductConfigurationViewComponent(IProductConfigurationService service)
        {
            _service = service;
        }

        public async Task<IViewComponentResult> InvokeAsync(int selectedProductId)
        {
            List<ProductConfigurationIndexItem> result = new List<ProductConfigurationIndexItem>();

            if (selectedProductId > 0)
            {
                List<ProductConfigurationIndexItem> data = await _service.GetIndexAsync();

                foreach (ProductConfigurationIndexItem item in data)
                {
                    if (item.ProductId == selectedProductId)
                    {
                        result.Add(item);
                    }
                }
            }

            ViewBag.SelectedProductId = selectedProductId;

            return View("~/Components/ProductConfiguration/Default.cshtml", result);
        }
    }
}