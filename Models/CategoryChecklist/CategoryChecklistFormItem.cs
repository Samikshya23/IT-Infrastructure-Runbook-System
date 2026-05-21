namespace EmployeeAccessSystem.Models
{
    public class CategoryChecklistFormItem
    {
        public string SetupNodeId { get; set; }

        public string ParentPath { get; set; }

        public string DisplayName { get; set; }

        public string ValueType { get; set; }

        public string ResultValue { get; set; }

        public int ValueTypeId { get; set; }

        public CategoryChecklistFormItem()
        {
            SetupNodeId = string.Empty;
            ParentPath = string.Empty;
            DisplayName = string.Empty;
            ValueType = string.Empty;
            ResultValue = string.Empty;
        }
    }
}