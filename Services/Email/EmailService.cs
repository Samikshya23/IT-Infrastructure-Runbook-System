using EmployeeAccessSystem.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmployeeAccessSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _emailSettings = new EmailSettings();

            _emailSettings.SmtpHost = configuration["EmailSettings:SmtpHost"] ?? "";
            _emailSettings.SmtpPort = Convert.ToInt32(configuration["EmailSettings:SmtpPort"] ?? "587");
            _emailSettings.EnableSsl = Convert.ToBoolean(configuration["EmailSettings:EnableSsl"] ?? "true");
            _emailSettings.SenderName = configuration["EmailSettings:SenderName"] ?? "";
            _emailSettings.SenderEmail = configuration["EmailSettings:SenderEmail"] ?? "";
            _emailSettings.SenderPassword = configuration["EmailSettings:SenderPassword"] ?? "";

            _emailSettings.SmtpHost = _emailSettings.SmtpHost.Trim();
            _emailSettings.SenderName = _emailSettings.SenderName.Trim();
            _emailSettings.SenderEmail = _emailSettings.SenderEmail.Trim();
            _emailSettings.SenderPassword = _emailSettings.SenderPassword.Trim();

            _logger = logger;
        }

        public async Task SendUserPasswordEmailAsync(string toEmail, string fullName, string password)
        {
            try
            {
                toEmail = (toEmail ?? "").Trim();
                fullName = (fullName ?? "").Trim();
                password = password ?? "";

                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    throw new Exception("Receiver email address is empty.");
                }

                if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
                {
                    throw new Exception("Sender email is missing in appsettings.json.");
                }

                if (string.IsNullOrWhiteSpace(_emailSettings.SenderPassword))
                {
                    throw new Exception("Sender app password is missing in appsettings.json.");
                }

                string subject = "Your IT Infrastructure Runbook System Account";

                string body = @"
<html>
<body style='font-family: Arial, sans-serif; font-size: 14px; color: #333;'>

<p>Dear " + fullName + @",</p>

<p>Your account has been created for <b>IT Infrastructure Runbook System</b>.</p>

<p>Please use the following credentials to login:</p>

<table style='border-collapse: collapse; margin-top: 10px;'>
    <tr>
        <td style='padding: 6px 10px; border: 1px solid #ddd;'><b>Email</b></td>
        <td style='padding: 6px 10px; border: 1px solid #ddd;'>" + toEmail + @"</td>
    </tr>
    <tr>
        <td style='padding: 6px 10px; border: 1px solid #ddd;'><b>Password</b></td>
        <td style='padding: 6px 10px; border: 1px solid #ddd;'>" + password + @"</td>
    </tr>
</table>

<p style='margin-top: 15px;'>Please keep your password safe.</p>

<p>Regards,<br/>
IT Infrastructure Runbook System</p>

</body>
</html>";

                using MailMessage mailMessage = new MailMessage();

                mailMessage.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                using SmtpClient smtpClient = new SmtpClient(_emailSettings.SmtpHost);

                smtpClient.Port = _emailSettings.SmtpPort;
                smtpClient.EnableSsl = _emailSettings.EnableSsl;
                smtpClient.Credentials = new NetworkCredential(
                    _emailSettings.SenderEmail,
                    _emailSettings.SenderPassword
                );

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Password email sent successfully. Email: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending password email. Email: {Email}", toEmail);

                throw;
            }
        }
    }
}