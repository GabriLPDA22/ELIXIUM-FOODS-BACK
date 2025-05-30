// Services/IEmailService.cs
using UberEatsBackend.Models;

namespace UberEatsBackend.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);
        Task<bool> SendWelcomeEmailAsync(string email, string firstName);
        Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string textContent = null);
    }
}

