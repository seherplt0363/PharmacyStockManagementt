using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using pharmacystock.Services.Interfaces;

namespace pharmacystock.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string bodyHtml, byte[] attachmentBytes, string fileName)
        {
            var smtpServer = _configuration["SmtpSettings:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
            var senderEmail = _configuration["SmtpSettings:SenderEmail"];
            var senderName = _configuration["SmtpSettings:SenderName"] ?? "Pharmacy Stock System";
            var username = _configuration["SmtpSettings:UserName"];
            var password = _configuration["SmtpSettings:Password"];

            // TLS 1.2 Güvenlik Protokolü
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(senderEmail, senderName);
                message.To.Add(new MailAddress(toEmail.Trim()));
                message.Subject = subject;
                message.Body = bodyHtml;
                message.IsBodyHtml = true;

                if (attachmentBytes != null && attachmentBytes.Length > 0)
                {
                    using (var ms = new MemoryStream(attachmentBytes))
                    {
                        message.Attachments.Add(new Attachment(ms, fileName, "application/pdf"));

                        using (var client = new SmtpClient(smtpServer, port))
                        {
                            client.Credentials = new NetworkCredential(username, password);
                            client.EnableSsl = true;
                            client.UseDefaultCredentials = false;

                            await client.SendMailAsync(message);
                        }
                    }
                }
            }
        }
    }
}