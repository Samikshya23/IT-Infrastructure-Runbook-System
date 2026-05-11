using System;
using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class ProductEntrySaveRequest
    {
        public Guid? EntryGroupId { get; set; }

        public int ProductId { get; set; }

        public DateTime EntryDate { get; set; }

        public List<ProductEntryFormItem> Items { get; set; }

        public ProductEntrySaveRequest()
        {
            Items = new List<ProductEntryFormItem>();
        }
    }
}