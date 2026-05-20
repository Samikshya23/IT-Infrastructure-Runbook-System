namespace EmployeeAccessSystem.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        public bool IsActive { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; } = "";

        public bool HasFullAccess { get; set; }
    }
}