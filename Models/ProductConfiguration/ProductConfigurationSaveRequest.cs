using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class ProductConfigurationSaveRequest
    {
        public int ProductId { get; set; }

        public List<ProductConfigurationNodeRequest> Nodes { get; set; }

        public ProductConfigurationSaveRequest()
        {
            Nodes = new List<ProductConfigurationNodeRequest>();
        }
    }

    public class ProductConfigurationNodeRequest
    {
        public string NodeName { get; set; }

        // Keep temporarily because old service still references it
        public string NodeType { get; set; }

        public string InputType { get; set; }

        public List<ProductConfigurationNodeRequest> Children { get; set; }

        public ProductConfigurationNodeRequest()
        {
            Children = new List<ProductConfigurationNodeRequest>();
        }
    }
}