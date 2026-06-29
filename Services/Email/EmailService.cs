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

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string host, int port, bool ssl)> _smtpCache = 
            new System.Collections.Concurrent.ConcurrentDictionary<string, (string, int, bool)>(StringComparer.OrdinalIgnoreCase);

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
                
                string activeSmtpHost = "";
                int activeSmtpPort = 587;
                bool activeEnableSsl = true;

                if (!string.IsNullOrWhiteSpace(senderEmail))
                {
                    activeSmtpHost = ResolveSmtpHostFromMx(activeSenderEmail, out activeSmtpPort, out activeEnableSsl);
                }
                else
                {
                    activeSmtpHost = settings.SmtpHost;
                    activeSmtpPort = settings.SmtpPort;
                    activeEnableSsl = settings.EnableSsl;
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

                    // Enforce a strict 5 second timeout for SendMailAsync in .NET Core
                    var sendTask = smtpClient.SendMailAsync(mailMessage);
                    var timeoutTask = Task.Delay(5000);

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

        private string ResolveSmtpHostFromMx(string senderEmail, out int port, out bool enableSsl)
        {
            // Default fallbacks
            port = 587;
            enableSsl = true;

            if (string.IsNullOrWhiteSpace(senderEmail) || !senderEmail.Contains("@"))
            {
                return "";
            }

            string domain = senderEmail.Split('@').Last().Trim().ToLower();

            // Check cache first
            if (_smtpCache.TryGetValue(domain, out var cached))
            {
                port = cached.port;
                enableSsl = cached.ssl;
                return cached.host;
            }

            string host = "";

            // 1. Direct check for popular domains
            if (domain == "gmail.com")
            {
                host = "smtp.gmail.com";
            }
            else if (domain == "yahoo.com" || domain == "ymail.com" || domain == "rocketmail.com")
            {
                host = "smtp.mail.yahoo.com";
            }
            else if (domain == "outlook.com" || domain == "hotmail.com" || domain == "live.com" || domain == "msn.com")
            {
                host = "smtp.office365.com";
            }
            else if (domain == "zoho.com")
            {
                host = "smtp.zoho.com";
            }
            else if (domain == "yandex.com")
            {
                host = "smtp.yandex.com";
            }

            if (!string.IsNullOrEmpty(host))
            {
                _smtpCache[domain] = (host, port, enableSsl);
                return host;
            }

            // 2. Perform MX lookup for custom domains
            try
            {
                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo.FileName = "nslookup";
                    process.StartInfo.Arguments = $"-type=MX {domain}";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        string lowerOutput = output.ToLower();
                        
                        // Check if Google Workspace is hosting this domain
                        if (lowerOutput.Contains("google.com") || lowerOutput.Contains("googlemail.com"))
                        {
                            host = "smtp.gmail.com";
                        }
                        // Check if Microsoft 365 / Exchange Online is hosting this domain
                        else if (lowerOutput.Contains("outlook.com") || lowerOutput.Contains("mail.protection.outlook.com"))
                        {
                            host = "smtp.office365.com";
                        }
                        // Check if Zoho Mail is hosting this domain
                        else if (lowerOutput.Contains("zoho.com"))
                        {
                            host = "smtp.zoho.com";
                        }
                        // Check if Yandex Mail is hosting this domain
                        else if (lowerOutput.Contains("yandex.com"))
                        {
                            host = "smtp.yandex.com";
                        }
                        else
                        {
                            // Extract the primary mail exchanger host
                            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var line in lines)
                            {
                                if (line.Contains("mail exchanger ="))
                                {
                                    var parts = line.Split(new[] { "mail exchanger =" }, StringSplitOptions.None);
                                    if (parts.Length > 1)
                                    {
                                        string resolved = parts[1].Trim().TrimEnd('.');
                                        if (!string.IsNullOrEmpty(resolved))
                                        {
                                            host = resolved;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve MX records for domain {Domain}", domain);
            }

            if (string.IsNullOrEmpty(host))
            {
                host = $"mail.{domain}";
            }

            _smtpCache[domain] = (host, port, enableSsl);
            return host;
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