using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class ProductStructurePageViewModel
    {
        public int SelectedProductId { get; set; }
        public string ProductName { get; set; }

        public List<ProductDropDownItem> ProductList { get; set; }
        public List<ProductStructureRowViewModel> Rows { get; set; }

        public ProductStructurePageViewModel()
        {
            ProductList = new List<ProductDropDownItem>();
            Rows = new List<ProductStructureRowViewModel>();
        }
    }

    public class ProductDropDownItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class ProductStructureRowViewModel
    {
        public int SN { get; set; }
        public int NodeId { get; set; }
        public int ProductId { get; set; }
        public string NodeName { get; set; }

        public List<ProductStructureChildViewModel> Children { get; set; }

        public ProductStructureRowViewModel()
        {
            Children = new List<ProductStructureChildViewModel>();
        }
    }

    public class ProductStructureChildViewModel
    {
        public int NodeId { get; set; }
        public int ProductId { get; set; }
        public string ItemName { get; set; }
        public int Level { get; set; }
    }
}