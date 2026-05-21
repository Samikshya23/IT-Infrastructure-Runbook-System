namespace EmployeeAccessSystem.Models
{
    public class AccessControlRoleActionModel
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public int? ParentMenuId { get; set; }

        public int? MainMenuId { get; set; }

        public string MainMenuName { get; set; }

        public int? ParentGroupId { get; set; }

        public string ParentGroupName { get; set; }

        public int? PageMenuId { get; set; }

        public string PageMenuName { get; set; }

        public bool IsVisibleInSidebar { get; set; }

        public bool IsChecked { get; set; }

        public AccessControlRoleActionModel()
        {
            MenuName = string.Empty;
            ControllerName = string.Empty;
            ActionName = string.Empty;
            MainMenuName = string.Empty;
            ParentGroupName = string.Empty;
            PageMenuName = string.Empty;
        }
    }
}