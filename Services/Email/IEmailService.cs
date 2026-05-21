namespace EmployeeAccessSystem.Services
{
    public interface IEmailService
    {
        Task SendUserPasswordEmailAsync(string toEmail, string fullName, string password);
    }
}