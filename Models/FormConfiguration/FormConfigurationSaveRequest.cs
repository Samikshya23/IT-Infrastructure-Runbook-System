using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class FormConfigurationSaveRequest
    {
        public int CategoryId { get; set; }

        public List<FormConfigurationNodeRequest> Nodes { get; set; }

        public FormConfigurationSaveRequest()
        {
            Nodes = new List<FormConfigurationNodeRequest>();
        }
    }

    public class FormConfigurationNodeRequest
    {
        public string Heading { get; set; }

        public string NodeName { get; set; }

        public string InputType { get; set; }

        public List<FormConfigurationNodeRequest> Children { get; set; }

        public FormConfigurationNodeRequest()
        {
            Heading = string.Empty;
            NodeName = string.Empty;
            InputType = string.Empty;
            Children = new List<FormConfigurationNodeRequest>();
        }
    }
}