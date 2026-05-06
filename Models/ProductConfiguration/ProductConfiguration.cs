using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class ProductConfiguration
    {
        public int NodeId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? ParentNodeId { get; set; }

        public string Heading { get; set; }
        public string NodeName { get; set; }
        public string InputType { get; set; }

        public string ConfigurationJson { get; set; }
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