using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        // Load sidebar menu according to logged-in user permission
        public async Task<List<MenuModel>> GetSidebarMenusAsync(int accountId)
        {
            try
            {
                if (accountId <= 0)
                {
                    return new List<MenuModel>();
                }

                IEnumerable<MenuModel> result = await _menuRepository.GetMenusByAccountIdAsync(accountId);
                List<MenuModel> menuList = new List<MenuModel>();
                List<MenuModel> parentMenus = new List<MenuModel>();

                if (result != null)
                {
                    foreach (MenuModel menu in result)
                    {
                        menuList.Add(menu);
                    }
                }

                foreach (MenuModel menu in menuList)
                {
                    if (menu.ParentMenuId == null)
                    {
                        parentMenus.Add(menu);
                    }
                }

                foreach (MenuModel parent in parentMenus)
                {
                    parent.Children = new List<MenuModel>();

                    foreach (MenuModel child in menuList)
                    {
                        if (child.ParentMenuId == parent.MenuId)
                        {
                            parent.Children.Add(child);
                        }
                    }
                }

                return parentMenus;
            }
            catch
            {
                return new List<MenuModel>();
            }
        }
    }
}