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
                        menu.Children = new List<MenuModel>();
                        menuList.Add(menu);
                    }
                }

                foreach (MenuModel menu in menuList)
                {
                    if (menu.ParentMenuId != null)
                    {
                        foreach (MenuModel possibleParent in menuList)
                        {
                            if (menu.ParentMenuId == possibleParent.MenuId)
                            {
                                possibleParent.Children.Add(menu);
                                break;
                            }
                        }
                    }
                }

                foreach (MenuModel menu in menuList)
                {
                    if (menu.ParentMenuId == null)
                    {
                        // Exclude the main Dashboard menu to avoid duplicates (since it is already hardcoded at the top of the sidebar)
                        if (menu.MenuId != 1 && !(menu.ControllerName == "Home" && menu.ActionName == "Index"))
                        {
                            parentMenus.Add(menu);
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