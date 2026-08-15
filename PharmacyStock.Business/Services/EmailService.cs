using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using PharmacyStock.Business.Interfaces;

namespace PharmacyStock.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string bodyHtml,
            byte[] attachmentBytes,
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException(
                    "Alıcı e-posta adresi boş olamaz.",
                    nameof(toEmail));

            var smtpServer =
                _configuration["SmtpSettings:Host"]
                ?? "smtp.gmail.com";

            var portText =
                _configuration["SmtpSettings:Port"]
                ?? "587";

            if (!int.TryParse(portText, out var port))
                port = 587;

            var senderEmail =
                _configuration["SmtpSettings:SenderEmail"];

            var senderName =
                _configuration["SmtpSettings:SenderName"]
                ?? "Pharmacy Stock System";

            var username =
                _configuration["SmtpSettings:UserName"];

            var password =
                _configuration["SmtpSettings:Password"];

            if (string.IsNullOrWhiteSpace(senderEmail))
                throw new InvalidOperationException(
                    "SMTP gönderici e-posta adresi tanımlanmamış.");

            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException(
                    "SMTP kullanıcı adı tanımlanmamış.");

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException(
                    "SMTP şifresi tanımlanmamış.");

            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12;

            using var message = new MailMessage
            {
                From = new MailAddress(
                    senderEmail,
                    senderName),

                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            };

            message.To.Add(
                new MailAddress(toEmail.Trim()));

            if (attachmentBytes is { Length: > 0 })
            {
                var stream =
                    new MemoryStream(attachmentBytes);

                var attachment =
                    new Attachment(
                        stream,
                        fileName,
                        "application/pdf");

                message.Attachments.Add(attachment);
            }

            using var client =
                new SmtpClient(smtpServer, port)
                {
                    Credentials =
                        new NetworkCredential(
                            username,
                            password),

                    EnableSsl = true,
                    UseDefaultCredentials = false
                };

            await client.SendMailAsync(message);
        }
    }
}