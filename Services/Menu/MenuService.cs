using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<MenuModel>> GetSidebarMenusAsync(int accountId)
        {
            IEnumerable<MenuModel> menus =
                await _menuRepository.GetMenusByAccountIdAsync(accountId);

            List<MenuModel> menuList =
                menus.ToList();

            List<MenuModel> parentMenus =
                new List<MenuModel>();

            foreach (MenuModel menu in menuList)
            {
                if (menu.ParentMenuId == null)
                {
                    parentMenus.Add(menu);
                }
            }

            foreach (MenuModel parent in parentMenus)
            {
                List<MenuModel> childMenus =
                    new List<MenuModel>();

                foreach (MenuModel child in menuList)
                {
                    if (child.ParentMenuId == parent.MenuId)
                    {
                        childMenus.Add(child);
                    }
                }

                parent.Children = childMenus;
            }

            return parentMenus;
        }
    }
}