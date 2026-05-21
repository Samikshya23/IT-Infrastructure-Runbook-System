using System.ComponentModel.DataAnnotations;

namespace EmployeeAccessSystem.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}