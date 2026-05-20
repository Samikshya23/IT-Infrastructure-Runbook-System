using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAccessSystem.Components.Menu
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IMenuService _menuService;

        public MenuViewComponent(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // Load sidebar menu according to logged-in account
        public async Task<IViewComponentResult> InvokeAsync()
        {
            int accountId = 0;

            string accountIdValue = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(accountIdValue))
            {
                int.TryParse(accountIdValue, out accountId);
            }

            var menus = await _menuService.GetSidebarMenusAsync(accountId);

            return View("~/Components/Menu/Default.cshtml", menus);
        }
    }
}