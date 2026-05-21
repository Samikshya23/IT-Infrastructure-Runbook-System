using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class CategorySetup
    {
        public int NodeId { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; }

        public string SetupJson { get; set; }

        public bool IsActive { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        public string DeletedBy { get; set; }

        public int? ConfigurationNodeId { get; set; }

        public string NodeValue { get; set; }

        public string FieldType { get; set; }

        public bool IsFieldValue { get; set; }

        public string ConfigurationNodeName { get; set; }

        public List<FormConfiguration> AvailableConfigurationNodes { get; set; }

        public List<CategorySetup> Children { get; set; }

        public CategorySetup()
        {
            Name = string.Empty;
            SetupJson = string.Empty;
            CreatedBy = string.Empty;
            ModifiedBy = string.Empty;
            DeletedBy = string.Empty;
            NodeValue = string.Empty;
            FieldType = string.Empty;
            ConfigurationNodeName = string.Empty;

            AvailableConfigurationNodes = new List<FormConfiguration>();
            Children = new List<CategorySetup>();
        }
    }
}