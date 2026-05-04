using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class ProductSetupConfiguration
    {
        public int NodeId { get; set; }
        public int ProductId { get; set; }

        public int? ConfigurationNodeId { get; set; }
        public int? ParentNodeId { get; set; }

        public string NodeValue { get; set; }

        public string InputType { get; set; }
        public string FieldType { get; set; }

        public bool IsFieldValue { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public string ConfigurationNodeName { get; set; }

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }

        public List<ProductConfiguration> AvailableConfigurationNodes { get; set; }
        public List<ProductSetupConfiguration> Children { get; set; }

        public ProductSetupConfiguration()
        {
            AvailableConfigurationNodes = new List<ProductConfiguration>();
            Children = new List<ProductSetupConfiguration>();
        }
    }
}