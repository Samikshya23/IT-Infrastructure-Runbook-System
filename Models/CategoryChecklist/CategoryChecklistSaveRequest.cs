using System;
using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class CategoryChecklistSaveRequest
    {
        public Guid? EntryGroupId { get; set; }

        public int CategoryId { get; set; }

        public DateTime EntryDate { get; set; }

        public List<CategoryChecklistFormItem> Items { get; set; }

        public CategoryChecklistSaveRequest()
        {
            Items = new List<CategoryChecklistFormItem>();
        }
    }
}