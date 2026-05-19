namespace EmployeeAccessSystem.Models
{
    public class AccessControlUserModel
    {
        public int AccountId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }
    }
}