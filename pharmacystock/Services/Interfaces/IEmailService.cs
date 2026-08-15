namespace pharmacystock.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailWithAttachmentAsync(string toEmail, string subject, string bodyHtml, byte[] attachmentBytes, string fileName);
    }
}