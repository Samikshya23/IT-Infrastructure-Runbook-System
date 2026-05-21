using System.ComponentModel.DataAnnotations;

namespace EmployeeAccessSystem.Models
{
    public class ProductSetup
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string ProductName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}