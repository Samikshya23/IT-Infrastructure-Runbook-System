using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class AccessControlMenuModel
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public int? ParentMenuId { get; set; }

        public string ParentMenuName { get; set; }

        public bool IsSubMenu { get; set; }

        public bool IsActive { get; set; }

        public bool IsVisibleInSidebar { get; set; }

        public int SortBy { get; set; }

        public bool IsChecked { get; set; }

        public List<AccessControlMenuModel> Children { get; set; }

        public AccessControlMenuModel()
        {
            Children = new List<AccessControlMenuModel>();
        }
    }
}