namespace EmployeeAccessSystem.Services
{
    public interface IEmailService
    {
        Task SendUserPasswordEmailAsync(string toEmail, string fullName, string password);
        Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string newPassword);
        Task SendReportAttachmentEmailAsync(string toEmail, string fullName, string attachmentName, byte[] attachmentBytes, string contentType, string subject = null, string body = null, string senderEmail = null, string senderPassword = null, string smtpHost = null, int? smtpPort = null, bool? enableSsl = null);
    }
}