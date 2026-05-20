namespace EmployeeAccessSystem.Models
{
    public class AccessControlRoleActionModel
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public int? ParentMenuId { get; set; }

        public string ParentMenuName { get; set; }

        public int? MainMenuId { get; set; }

        public string MainMenuName { get; set; }

        public int? PageMenuId { get; set; }

        public string PageMenuName { get; set; }

        public bool IsVisibleInSidebar { get; set; }

        public bool IsChecked { get; set; }
    }
}