using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class ProductConfiguration
    {
        public int NodeId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        // Needed for ProductSetupConfiguration in-memory tree
        public int? ParentNodeId { get; set; }

        public string NodeName { get; set; }

        // Keep temporarily because some service still uses it
        public string NodeType { get; set; }

        public string InputType { get; set; }

        public string ConfigurationJson { get; set; }

        // Needed temporarily because some tree/display logic may use it
        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public string CreatedBy { get; set; }
        public string DeletedBy { get; set; }

        public List<ProductConfiguration> Children { get; set; }

        public ProductConfiguration()
        {
            Children = new List<ProductConfiguration>();
        }
    }
}