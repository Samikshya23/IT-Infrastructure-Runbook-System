using System;

namespace EmployeeAccessSystem.Models
{
    public class Report
    {
        public int EntryId { get; set; }

        public Guid? EntryGroupId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string SetupNodeId { get; set; }

        public string ParentPath { get; set; }

        public string DisplayName { get; set; }

        public string ValueType { get; set; }

        public int ValueTypeId { get; set; }

        public string ResultValue { get; set; }

        public DateTime EntryDate { get; set; }

        public string CreatedBy { get; set; }

        public Report()
        {
            CategoryName = string.Empty;
            SetupNodeId = string.Empty;
            ParentPath = string.Empty;
            DisplayName = string.Empty;
            ValueType = string.Empty;
            ResultValue = string.Empty;
            CreatedBy = string.Empty;
        }
    }
}