using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class FormConfigurationIndexItem
    {
        public int CategoryId { get; set; }

        public string Name { get; set; }

        public List<FormConfiguration> Nodes { get; set; }

        public FormConfigurationIndexItem()
        {
            Name = string.Empty;
            Nodes = new List<FormConfiguration>();
        }
    }
}