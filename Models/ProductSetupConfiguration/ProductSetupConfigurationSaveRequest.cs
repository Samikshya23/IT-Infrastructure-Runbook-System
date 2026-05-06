using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class ProductSetupConfigurationSaveRequest
    {
        public int ProductId { get; set; }

        public List<ProductSetupConfigurationNodeRequest> Nodes { get; set; }

        public ProductSetupConfigurationSaveRequest()
        {
            Nodes = new List<ProductSetupConfigurationNodeRequest>();
        }
    }

    public class ProductSetupConfigurationNodeRequest
    {
        public string Heading { get; set; }

        public string Label { get; set; }

        public string ValueType { get; set; }

        public string Value { get; set; }

        public string FieldType { get; set; }

        public int? ConfigurationNodeId { get; set; }

        public List<ProductSetupConfigurationNodeRequest> Children { get; set; }

        public ProductSetupConfigurationNodeRequest()
        {
            Children = new List<ProductSetupConfigurationNodeRequest>();
        }
    }
}