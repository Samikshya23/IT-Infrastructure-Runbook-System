namespace EmployeeAccessSystem.Models
{
    public class DropdownItemModel
    {
        public int DropdownItemId { get; set; }

        public int DropdownGroupId { get; set; }

        public string ItemName { get; set; }

        public string ItemValue { get; set; }

        public string IconClass { get; set; }

        public string CssClass { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }
}