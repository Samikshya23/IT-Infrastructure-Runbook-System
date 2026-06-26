using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmployeeAccessSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IEmailRepository emailRepository,
            ILogger<EmailService> logger)
        {
            _emailRepository = emailRepository;
            _logger = logger;
        }

        private async Task<EmailSettings> GetSettingsAsync()
        {
            return await _emailRepository.GetEmailSettingsAsync();
        }

        public async Task SendUserPasswordEmailAsync(string toEmail, string fullName, string password)
        {
            try
            {
                var settings = await GetSettingsAsync();
                toEmail = (toEmail ?? "").Trim();
                fullName = (fullName ?? "").Trim();
                password = password ?? "";

                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    throw new Exception("Receiver email address is empty.");
                }

                if (string.IsNullOrWhiteSpace(settings.SenderEmail))
                {
                    throw new Exception("Sender email is missing in configuration.");
                }

                if (string.IsNullOrWhiteSpace(settings.SenderPassword))
                {
                    throw new Exception("Sender app password is missing in configuration.");
                }

                string subject = "Your IT Infrastructure Runbook System Account";

                // Load template from Repository
                string template = await _emailRepository.GetUserPasswordEmailTemplateAsync();
                string body = template
                    .Replace("{FullName}", fullName)
                    .Replace("{Email}", toEmail)
                    .Replace("{Password}", password);

                using MailMessage mailMessage = new MailMessage();

                mailMessage.From = new MailAddress(settings.SenderEmail, settings.SenderName);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                using SmtpClient smtpClient = new SmtpClient(settings.SmtpHost);

                smtpClient.Port = settings.SmtpPort;
                smtpClient.EnableSsl = settings.EnableSsl;
                smtpClient.Timeout = 10000;
                smtpClient.Credentials = new NetworkCredential(
                    settings.SenderEmail,
                    settings.SenderPassword
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

        public async Task SendForgotPasswordEmailAsync(string toEmail, string fullName, string newPassword)
        {
            try
            {
                var settings = await GetSettingsAsync();
                toEmail = (toEmail ?? "").Trim();
                fullName = (fullName ?? "").Trim();
                newPassword = newPassword ?? "";

                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    throw new Exception("Receiver email address is empty.");
                }

                if (string.IsNullOrWhiteSpace(settings.SenderEmail))
                {
                    throw new Exception("Sender email is missing in configuration.");
                }

                if (string.IsNullOrWhiteSpace(settings.SenderPassword))
                {
                    throw new Exception("Sender app password is missing in configuration.");
                }

                string subject = "Your IT Infrastructure Runbook System Account Password Reset";

                // Load template from Repository
                string template = await _emailRepository.GetForgotPasswordEmailTemplateAsync();
                string body = template
                    .Replace("{FullName}", fullName)
                    .Replace("{Email}", toEmail)
                    .Replace("{TemporaryPassword}", newPassword);

                using MailMessage mailMessage = new MailMessage();

                mailMessage.From = new MailAddress(settings.SenderEmail, settings.SenderName);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                using SmtpClient smtpClient = new SmtpClient(settings.SmtpHost);

                smtpClient.Port = settings.SmtpPort;
                smtpClient.EnableSsl = settings.EnableSsl;
                smtpClient.Timeout = 10000;
                smtpClient.Credentials = new NetworkCredential(
                    settings.SenderEmail,
                    settings.SenderPassword
                );

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Forgot password email sent successfully. Email: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending forgot password email. Email: {Email}", toEmail);
                throw;
            }
        }

        public async Task SendReportAttachmentEmailAsync(
            string toEmail, 
            string fullName, 
            string attachmentName, 
            byte[] attachmentBytes, 
            string contentType, 
            string subject = null, 
            string body = null,
            string senderEmail = null,
            string senderPassword = null,
            string smtpHost = null,
            int? smtpPort = null,
            bool? enableSsl = null)
        {
            try
            {
                var settings = await GetSettingsAsync();
                toEmail = (toEmail ?? "").Trim();
                fullName = (fullName ?? "").Trim();

                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    throw new Exception("Receiver email address is empty.");
                }

                // Validate all recipient emails
                var emails = toEmail.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var email in emails)
                {
                    string trimmed = email.Trim();
                    if (!IsValidEmail(trimmed))
                    {
                        throw new Exception($"Invalid receiver email address: '{trimmed}'");
                    }
                }

                string activeSenderEmail = !string.IsNullOrWhiteSpace(senderEmail) ? senderEmail.Trim() : settings.SenderEmail;
                string activeSenderPassword = !string.IsNullOrWhiteSpace(senderPassword) ? senderPassword.Trim() : settings.SenderPassword;
                string activeSmtpHost = !string.IsNullOrWhiteSpace(smtpHost) ? smtpHost.Trim() : settings.SmtpHost;
                int activeSmtpPort = smtpPort ?? settings.SmtpPort;
                bool activeEnableSsl = enableSsl ?? settings.EnableSsl;

                // Auto-detect common email providers if they just type their email address
                if (string.IsNullOrWhiteSpace(smtpHost) && !string.IsNullOrWhiteSpace(senderEmail))
                {
                    string domain = senderEmail.Split('@').Last().ToLower();
                    if (domain == "gmail.com")
                    {
                        activeSmtpHost = "smtp.gmail.com";
                        activeSmtpPort = 587;
                        activeEnableSsl = true;
                    }
                    else if (domain == "yahoo.com" || domain == "ymail.com")
                    {
                        activeSmtpHost = "smtp.mail.yahoo.com";
                        activeSmtpPort = 587;
                        activeEnableSsl = true;
                    }
                    else if (domain == "outlook.com" || domain == "hotmail.com" || domain == "live.com")
                    {
                        activeSmtpHost = "smtp.office365.com";
                        activeSmtpPort = 587;
                        activeEnableSsl = true;
                    }
                }

                if (string.IsNullOrWhiteSpace(activeSenderEmail))
                {
                    throw new Exception("Sender email is missing.");
                }

                if (string.IsNullOrWhiteSpace(activeSenderPassword))
                {
                    throw new Exception("Sender password is missing.");
                }

                if (string.IsNullOrWhiteSpace(subject))
                {
                    subject = "IT Infrastructure Runbook Report";
                }

                if (string.IsNullOrWhiteSpace(body))
                {
                    // Load template from Repository
                    body = await _emailRepository.GetReportAttachmentEmailTemplateAsync();
                }
                else
                {
                    body = body.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
                }

                using MailMessage mailMessage = new MailMessage();

                mailMessage.From = new MailAddress(activeSenderEmail, settings.SenderName);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                using (System.IO.MemoryStream stream = new System.IO.MemoryStream(attachmentBytes))
                {
                    Attachment attachment = new Attachment(stream, attachmentName, contentType);
                    mailMessage.Attachments.Add(attachment);

                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                    using SmtpClient smtpClient = new SmtpClient(activeSmtpHost);

                    smtpClient.Port = activeSmtpPort;
                    smtpClient.EnableSsl = activeEnableSsl;
                    smtpClient.Credentials = new NetworkCredential(
                        activeSenderEmail,
                        activeSenderPassword
                    );

                    // Enforce a strict 10 second timeout for SendMailAsync in .NET Core
                    var sendTask = smtpClient.SendMailAsync(mailMessage);
                    var timeoutTask = Task.Delay(10000);

                    var completedTask = await Task.WhenAny(sendTask, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        smtpClient.Dispose(); // Cancel underlying connection
                        throw new TimeoutException("The SMTP connection timed out. Please check your SMTP Host, Port, and network connection.");
                    }

                    await sendTask; // Propagate exceptions if any
                }

                _logger.LogInformation("Report attachment email sent successfully. Email: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending report attachment email. Email: {Email}", toEmail);
                throw;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}