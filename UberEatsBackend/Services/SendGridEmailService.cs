// Services/SendGridEmailService.cs
using SendGrid;
using SendGrid.Helpers.Mail;
using UberEatsBackend.Utils;

namespace UberEatsBackend.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly AppSettings _appSettings;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(ISendGridClient sendGridClient, AppSettings appSettings, ILogger<SendGridEmailService> logger)
        {
            _sendGridClient = sendGridClient;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
        {
            try
            {
                var resetLink = $"{resetUrl}?token={resetToken}&email={Uri.EscapeDataString(email)}";
                var subject = "Restablecer tu contraseña - Elixium Foods";

                var htmlContent = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: linear-gradient(135deg, #FF416C, #FF4B2B); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
                            .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                            .button {{ display: inline-block; background: #FF416C; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
                            .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🔐 Restablecer Contraseña</h1>
                                <p>Hemos recibido una solicitud para restablecer tu contraseña</p>
                            </div>
                            <div class='content'>
                                <p>Hola,</p>
                                <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en <strong>Elixium Foods</strong>.</p>
                                <p>Si fuiste tú quien solicitó este cambio, haz clic en el botón de abajo para crear una nueva contraseña:</p>
                                <div style='text-align: center;'>
                                    <a href='{resetLink}' class='button'>Restablecer Contraseña</a>
                                </div>
                                <p><strong>Este enlace expirará en 1 hora por seguridad.</strong></p>
                                <p>Si no solicitaste este cambio, puedes ignorar este correo de forma segura.</p>
                                <p>Saludos,<br>El equipo de Elixium Foods</p>
                            </div>
                            <div class='footer'>
                                <p>Si tienes problemas con el botón, copia y pega este enlace en tu navegador:</p>
                                <p><a href='{resetLink}'>{resetLink}</a></p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var textContent = $@"
                    Restablecer tu contraseña - Elixium Foods

                    Hola,

                    Recibimos una solicitud para restablecer la contraseña de tu cuenta en Elixium Foods.

                    Si fuiste tú quien solicitó este cambio, visita el siguiente enlace para crear una nueva contraseña:
                    {resetLink}

                    Este enlace expirará en 1 hora por seguridad.

                    Si no solicitaste este cambio, puedes ignorar este correo de forma segura.

                    Saludos,
                    El equipo de Elixium Foods";

                return await SendEmailAsync(email, subject, htmlContent, textContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de reset de contraseña a {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
        {
            try
            {
                var subject = $"¡Bienvenido a Elixium Foods, {firstName}!";

                var htmlContent = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: linear-gradient(135deg, #FF416C, #FF4B2B); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
                            .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                            .button {{ display: inline-block; background: #FF416C; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🎉 ¡Bienvenido a Elixium Foods!</h1>
                            </div>
                            <div class='content'>
                                <p>¡Hola {firstName}!</p>
                                <p>¡Bienvenido a <strong>Elixium Foods</strong>! Estamos emocionados de tenerte como parte de nuestra comunidad.</p>
                                <p>Ya puedes empezar a explorar nuestros deliciosos restaurantes y realizar tus primeros pedidos.</p>
                                <div style='text-align: center;'>
                                    <a href='{_appSettings.FrontendUrl ?? "http://localhost:5173"}' class='button'>Explorar Restaurantes</a>
                                </div>
                                <p>¡Que disfrutes tu experiencia!</p>
                                <p>Saludos,<br>El equipo de Elixium Foods</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var textContent = $@"
                    ¡Bienvenido a Elixium Foods, {firstName}!

                    ¡Hola {firstName}!

                    ¡Bienvenido a Elixium Foods! Estamos emocionados de tenerte como parte de nuestra comunidad.

                    Ya puedes empezar a explorar nuestros deliciosos restaurantes y realizar tus primeros pedidos.

                    ¡Que disfrutes tu experiencia!

                    Saludos,
                    El equipo de Elixium Foods";

                return await SendEmailAsync(email, subject, htmlContent, textContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de bienvenida a {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string textContent = null)
        {
            try
            {
                var fromEmail = _appSettings.SendGrid?.FromEmail ?? "noreply@elixiumfoods.com";
                var fromName = _appSettings.SendGrid?.FromName ?? "Elixium Foods";

                var from = new EmailAddress(fromEmail, fromName);
                var toAddress = new EmailAddress(to);

                var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, textContent, htmlContent);

                var response = await _sendGridClient.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email enviado exitosamente a {To} con asunto: {Subject}", to, subject);
                    return true;
                }
                else
                {
                    _logger.LogError("Error enviando email. StatusCode: {StatusCode}, Body: {Body}",
                        response.StatusCode, await response.Body.ReadAsStringAsync());
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {To} con asunto {Subject}", to, subject);
                return false;
            }
        }
    }
}
