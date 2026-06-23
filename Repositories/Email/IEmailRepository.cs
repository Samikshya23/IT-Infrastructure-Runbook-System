using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IEmailRepository
    {
        Task<EmailSettings> GetEmailSettingsAsync();
        Task<string> GetUserPasswordEmailTemplateAsync();
        Task<string> GetForgotPasswordEmailTemplateAsync();
        Task<string> GetReportAttachmentEmailTemplateAsync();
    }
}
