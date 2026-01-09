using Microsoft.AspNetCore.Identity.UI.Services;

namespace BulkyWeb.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // TODO: Implement actual email sending logic here
            // For now, this is a placeholder implementation
            return Task.CompletedTask;
        }
    }
}
