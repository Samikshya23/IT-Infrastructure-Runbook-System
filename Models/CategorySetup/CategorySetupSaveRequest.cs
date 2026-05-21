using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class CategorySetupSaveRequest
    {
        public int CategoryId { get; set; }

        public int RootIndex { get; set; }

        public List<CategorySetupNodeRequest> Nodes { get; set; }

        public CategorySetupSaveRequest()
        {
            RootIndex = -1;
            Nodes = new List<CategorySetupNodeRequest>();
        }
    }

    public class CategorySetupNodeRequest
    {
        public string Id { get; set; }

        public string Heading { get; set; }

        public string Label { get; set; }

        public string ValueType { get; set; }

        public string Value { get; set; }

        public int? FieldTypeId { get; set; }

        public string FieldType { get; set; }

        public int? ConfigurationNodeId { get; set; }

        public List<CategorySetupNodeRequest> Children { get; set; }

        public CategorySetupNodeRequest()
        {
            Id = string.Empty;
            Heading = string.Empty;
            Label = string.Empty;
            ValueType = string.Empty;
            Value = string.Empty;
            FieldType = string.Empty;

            Children = new List<CategorySetupNodeRequest>();
        }
    }
}