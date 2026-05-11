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

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string accountIdValue =HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int accountId = 0;
            if (!string.IsNullOrEmpty(accountIdValue))
            {
                accountId = Convert.ToInt32(accountIdValue);
            }

            var menus =await _menuService.GetSidebarMenusAsync(accountId);

            return View("~/Components/Menu/Default.cshtml", menus);
        }
    }
}