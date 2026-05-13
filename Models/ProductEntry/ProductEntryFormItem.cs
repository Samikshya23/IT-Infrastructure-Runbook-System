namespace EmployeeAccessSystem.Models
{
    public class ProductEntryFormItem
    {
        public string SetupNodeId { get; set; }

        public string ParentPath { get; set; }

        public string DisplayName { get; set; }

        public string ValueType { get; set; }

        public string ResultValue { get; set; }
        public int ValueTypeId { get; set; }
    }
}