using System;

namespace EmployeeAccessSystem.Models
{
    public class ProductEntryModel
    {
        public int EntryId { get; set; }
        public Guid? EntryGroupId { get; set; }
        public int ProductId { get; set; }

        public string SetupNodeId { get; set; }
        public string ParentPath { get; set; }
        public string DisplayName { get; set; }
        public string ValueType { get; set; }
        public string ResultValue { get; set; }

        public DateTime EntryDate { get; set; }
        public bool IsActive { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}