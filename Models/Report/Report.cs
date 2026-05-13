using System;

namespace EmployeeAccessSystem.Models
{
    public class Report
    {
        public int EntryId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public string SetupNodeId { get; set; }
        public string ParentPath { get; set; }
        public string DisplayName { get; set; }

        public string ValueType { get; set; }
        public string ResultValue { get; set; }

        public DateTime EntryDate { get; set; }
        public string CreatedBy { get; set; }
    }
}