using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class FormConfiguration
    {
        public int NodeId { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int? ParentNodeId { get; set; }

        public string Heading { get; set; } = string.Empty;

        public string NodeName { get; set; } = string.Empty;

        public string InputType { get; set; } = string.Empty;

        public string ConfigurationJson { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public string DeletedBy { get; set; } = string.Empty;

        public List<FormConfiguration> Children { get; set; }

        public FormConfiguration()
        {
            Children = new List<FormConfiguration>();
        }
    }
}