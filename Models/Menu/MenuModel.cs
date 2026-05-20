using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class MenuModel
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string MenuSlug { get; set; }

        public string IconClass { get; set; }

        public string Url { get; set; }

        public int? ParentMenuId { get; set; }

        public bool IsSubMenu { get; set; }

        public bool IsActive { get; set; }

        public bool IsVisibleInSidebar { get; set; }

        public int SortBy { get; set; }

        public List<MenuModel> Children { get; set; }

        public MenuModel()
        {
            Children = new List<MenuModel>();
        }
    }
}