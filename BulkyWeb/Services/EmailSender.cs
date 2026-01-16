using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BulkyWeb.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _sendGridKey;

        public EmailSender(IConfiguration configuration)
        {
            _sendGridKey = configuration.GetSection("SendGrid:SecretKey").Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(_sendGridKey);
            var from = new EmailAddress("khanhbuiduc44@gmail.com", "Bulky Book");
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);
            return client.SendEmailAsync(msg);
        }
    }
}
