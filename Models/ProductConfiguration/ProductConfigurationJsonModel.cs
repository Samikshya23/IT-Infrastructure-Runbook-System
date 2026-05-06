using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class ProductConfigurationJsonModel
    {
        public string Product { get; set; }

        public List<ProductConfigurationJsonNode> Structure { get; set; }

        public ProductConfigurationJsonModel()
        {
            Structure = new List<ProductConfigurationJsonNode>();
        }
    }

    public class ProductConfigurationJsonNode
    {
        public string Heading { get; set; }

        public string Label { get; set; }

        public string ValueType { get; set; }

        public List<ProductConfigurationJsonNode> Children { get; set; }

        public ProductConfigurationJsonNode()
        {
            Children = new List<ProductConfigurationJsonNode>();
        }
    }
}