using System.ComponentModel.DataAnnotations;

namespace EmployeeAccessSystem.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please select role.")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}