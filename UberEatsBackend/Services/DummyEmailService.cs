// Services/DummyEmailService.cs
using UberEatsBackend.Utils;

namespace UberEatsBackend.Services
{
    public class DummyEmailService : IEmailService
    {
        private readonly ILogger<DummyEmailService> _logger;
        private readonly AppSettings _appSettings;

        public DummyEmailService(ILogger<DummyEmailService> logger, AppSettings appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
        {
            await Task.Delay(100); // Simular latencia

            var resetLink = $"{resetUrl}?token={resetToken}";

            _logger.LogInformation("=== EMAIL DE RESET SIMULADO ===");
            _logger.LogInformation("Para: {Email}", email);
            _logger.LogInformation("Asunto: Restablecer tu contraseña - Elixium Foods");
            _logger.LogInformation("Link de reset: {ResetLink}", resetLink);
            _logger.LogInformation("Token: {Token}", resetToken);
            _logger.LogInformation("=== FIN EMAIL SIMULADO ===");

            // En desarrollo, también imprimir en consola para facilitar el debugging
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    📧 EMAIL SIMULADO                      ║");
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Para: {email.PadRight(49)} ║");
            Console.WriteLine($"║ Asunto: Restablecer contraseña{new string(' ', 29)} ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine($"║ Link de reset:                                            ║");
            Console.WriteLine($"║ {resetLink.PadRight(57)} ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine($"║ Token: {resetToken.PadRight(49)} ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");

            return true;
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
        {
            await Task.Delay(100); // Simular latencia

            _logger.LogInformation("=== EMAIL DE BIENVENIDA SIMULADO ===");
            _logger.LogInformation("Para: {Email}", email);
            _logger.LogInformation("Nombre: {FirstName}", firstName);
            _logger.LogInformation("Asunto: ¡Bienvenido a Elixium Foods, {FirstName}!", firstName);
            _logger.LogInformation("=== FIN EMAIL SIMULADO ===");

            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    📧 EMAIL SIMULADO                      ║");
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Para: {email.PadRight(49)} ║");
            Console.WriteLine($"║ Nombre: {firstName.PadRight(47)} ║");
            Console.WriteLine($"║ Asunto: ¡Bienvenido a Elixium Foods!{new string(' ', 19)} ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║ 🎉 ¡Usuario registrado exitosamente!                      ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");

            return true;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string textContent = null)
        {
            await Task.Delay(100); // Simular latencia

            _logger.LogInformation("=== EMAIL GENÉRICO SIMULADO ===");
            _logger.LogInformation("Para: {To}", to);
            _logger.LogInformation("Asunto: {Subject}", subject);
            _logger.LogInformation("Contenido HTML: {HtmlLength} caracteres", htmlContent?.Length ?? 0);
            _logger.LogInformation("Contenido Texto: {TextLength} caracteres", textContent?.Length ?? 0);
            _logger.LogInformation("=== FIN EMAIL SIMULADO ===");

            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    📧 EMAIL SIMULADO                      ║");
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ Para: {to.PadRight(49)} ║");
            Console.WriteLine($"║ Asunto: {subject.PadRight(45)} ║");
            Console.WriteLine($"║ HTML: {(htmlContent?.Length ?? 0).ToString().PadRight(49)} ║");
            Console.WriteLine($"║ Texto: {(textContent?.Length ?? 0).ToString().PadRight(48)} ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");

            return true;
        }
    }
}
