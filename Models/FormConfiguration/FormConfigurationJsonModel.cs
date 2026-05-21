using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class FormConfigurationJsonModel
    {
        public string Category { get; set; }

        public List<FormConfigurationJsonNode> Structure { get; set; }

        public FormConfigurationJsonModel()
        {
            Category = string.Empty;
            Structure = new List<FormConfigurationJsonNode>();
        }
    }

    public class FormConfigurationJsonNode
    {
        public string Heading { get; set; }

        public string Label { get; set; }

        public string ValueType { get; set; }

        public List<FormConfigurationJsonNode> Children { get; set; }

        public FormConfigurationJsonNode()
        {
            Heading = string.Empty;
            Label = string.Empty;
            ValueType = string.Empty;
            Children = new List<FormConfigurationJsonNode>();
        }
    }
}