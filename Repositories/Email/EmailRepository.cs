using System;
using System.IO;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using Microsoft.Extensions.Configuration;

namespace EmployeeAccessSystem.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        private readonly IConfiguration _configuration;

        public EmailRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<EmailSettings> GetEmailSettingsAsync()
        {
            var emailSettings = new EmailSettings
            {
                SmtpHost = (_configuration["EmailSettings:SmtpHost"] ?? "").Trim(),
                SmtpPort = Convert.ToInt32(_configuration["EmailSettings:SmtpPort"] ?? "587"),
                EnableSsl = Convert.ToBoolean(_configuration["EmailSettings:EnableSsl"] ?? "true"),
                SenderName = (_configuration["EmailSettings:SenderName"] ?? "").Trim(),
                SenderEmail = (_configuration["EmailSettings:SenderEmail"] ?? "").Trim(),
                SenderPassword = (_configuration["EmailSettings:SenderPassword"] ?? "").Trim()
            };

            return Task.FromResult(emailSettings);
        }

        private async Task<string> LoadTemplateFileAsync(string fileName)
        {
            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", fileName);
                if (!File.Exists(path))
                {
                    // Fallback to checking the Web project subdirectory context if run from solution root
                    string fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "EmployeeAccessSystem", "EmailTemplates", fileName);
                    if (File.Exists(fallbackPath))
                    {
                        path = fallbackPath;
                    }
                }

                if (File.Exists(path))
                {
                    return await File.ReadAllTextAsync(path);
                }

                throw new FileNotFoundException($"Email template file '{fileName}' not found at '{path}'.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load email template '{fileName}': {ex.Message}", ex);
            }
        }

        public async Task<string> GetUserPasswordEmailTemplateAsync()
        {
            return await LoadTemplateFileAsync("UserPasswordEmail.html");
        }

        public async Task<string> GetForgotPasswordEmailTemplateAsync()
        {
            return await LoadTemplateFileAsync("ForgotPasswordEmail.html");
        }

        public async Task<string> GetReportAttachmentEmailTemplateAsync()
        {
            return await LoadTemplateFileAsync("ReportAttachmentEmail.html");
        }
    }
}
