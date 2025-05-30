// Services/SESEmailService.cs
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using UberEatsBackend.Utils;

namespace UberEatsBackend.Services
{
  public class SESEmailService : IEmailService
  {
    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly AppSettings _appSettings;
    private readonly ILogger<SESEmailService> _logger;
    private readonly IWebHostEnvironment _environment;

    public SESEmailService(IAmazonSimpleEmailService sesClient, AppSettings appSettings,
        ILogger<SESEmailService> logger, IWebHostEnvironment environment)
    {
      _sesClient = sesClient;
      _appSettings = appSettings;
      _logger = logger;
      _environment = environment;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
    {
      try
      {
        // ✅ INCLUIR EMAIL EN LA URL DEL RESET
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
        // En desarrollo, verifica que el email esté verificado en SES
        if (_environment.IsDevelopment())
        {
          var isVerified = await IsEmailVerifiedAsync(email);
          if (!isVerified)
          {
            _logger.LogWarning("Email {Email} no está verificado en SES. " +
                "Para desarrollo, verifica el email en la consola de AWS SES.", email);

            // En desarrollo, simular el envío
            _logger.LogInformation("Simulando envío de email de bienvenida para {Email}", email);
            LogWelcomeEmailContent(email, firstName);
            return true;
          }
        }

        var subject = $"¡Bienvenido a Elixium Foods, {firstName}!";
        var htmlContent = GenerateWelcomeHtml(firstName);
        var textContent = GenerateWelcomeText(firstName);

        return await SendEmailAsync(email, subject, htmlContent, textContent);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error enviando email de bienvenida a {Email}", email);

        if (_environment.IsDevelopment())
        {
          LogWelcomeEmailContent(email, firstName);
        }

        return false;
      }
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string textContent = null)
    {
      try
      {
        // Configurar email de origen
        var fromEmail = _appSettings.AWS?.SES?.FromEmail ?? "noreply@elixiumfoods.com";
        var fromName = _appSettings.AWS?.SES?.FromName ?? "Elixium Foods";

        // En desarrollo, verificar que el email de origen esté verificado
        if (_environment.IsDevelopment())
        {
          var isFromVerified = await IsEmailVerifiedAsync(fromEmail);
          if (!isFromVerified)
          {
            _logger.LogError("Email de origen {FromEmail} no está verificado en SES. " +
                "Verifica este email en la consola de AWS SES antes de enviar emails.", fromEmail);
            return false;
          }
        }

        var sendRequest = new SendEmailRequest
        {
          Source = $"{fromName} <{fromEmail}>",
          Destination = new Destination
          {
            ToAddresses = new List<string> { to }
          },
          Message = new Message
          {
            Subject = new Content(subject),
            Body = new Body()
          }
        };

        if (!string.IsNullOrEmpty(htmlContent))
        {
          sendRequest.Message.Body.Html = new Content
          {
            Charset = "UTF-8",
            Data = htmlContent
          };
        }

        if (!string.IsNullOrEmpty(textContent))
        {
          sendRequest.Message.Body.Text = new Content
          {
            Charset = "UTF-8",
            Data = textContent
          };
        }

        _logger.LogInformation("Enviando email a {To} con asunto: {Subject}", to, subject);

        var response = await _sesClient.SendEmailAsync(sendRequest);

        _logger.LogInformation("Email enviado exitosamente. MessageId: {MessageId}", response.MessageId);

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error enviando email a {To} con asunto {Subject}", to, subject);
        return false;
      }
    }

    // Método para verificar si un email está verificado en SES
    private async Task<bool> IsEmailVerifiedAsync(string email)
    {
      try
      {
        var request = new GetIdentityVerificationAttributesRequest
        {
          Identities = new List<string> { email }
        };

        var response = await _sesClient.GetIdentityVerificationAttributesAsync(request);

        if (response.VerificationAttributes.TryGetValue(email, out var attributes))
        {
          return attributes.VerificationStatus == VerificationStatus.Success;
        }

        return false;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error verificando status del email {Email}", email);
        return false;
      }
    }

    // Métodos para generar contenido de email
    private string GeneratePasswordResetHtml(string resetLink)
    {
      return $@"
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
    }

    private string GeneratePasswordResetText(string resetLink)
    {
      return $@"
                Restablecer tu contraseña - Elixium Foods

                Hola,

                Recibimos una solicitud para restablecer la contraseña de tu cuenta en Elixium Foods.

                Si fuiste tú quien solicitó este cambio, visita el siguiente enlace para crear una nueva contraseña:
                {resetLink}

                Este enlace expirará en 1 hora por seguridad.

                Si no solicitaste este cambio, puedes ignorar este correo de forma segura.

                Saludos,
                El equipo de Elixium Foods";
    }

    private string GenerateWelcomeHtml(string firstName)
    {
      return $@"
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
    }

    private string GenerateWelcomeText(string firstName)
    {
      return $@"
                ¡Bienvenido a Elixium Foods, {firstName}!

                ¡Hola {firstName}!

                ¡Bienvenido a Elixium Foods! Estamos emocionados de tenerte como parte de nuestra comunidad.

                Ya puedes empezar a explorar nuestros deliciosos restaurantes y realizar tus primeros pedidos.

                ¡Que disfrutes tu experiencia!

                Saludos,
                El equipo de Elixium Foods";
    }

    // Métodos para logging en desarrollo
    private void LogEmailContent(string email, string resetToken, string resetUrl)
    {
      var resetLink = $"{resetUrl}?token={resetToken}";

      _logger.LogInformation("=== EMAIL DE RESET SIMULADO ===");
      _logger.LogInformation("Para: {Email}", email);
      _logger.LogInformation("Asunto: Restablecer tu contraseña - Elixium Foods");
      _logger.LogInformation("Link de reset: {ResetLink}", resetLink);
      _logger.LogInformation("Token: {Token}", resetToken);
      _logger.LogInformation("=== FIN EMAIL SIMULADO ===");
    }

    private void LogWelcomeEmailContent(string email, string firstName)
    {
      _logger.LogInformation("=== EMAIL DE BIENVENIDA SIMULADO ===");
      _logger.LogInformation("Para: {Email}", email);
      _logger.LogInformation("Nombre: {FirstName}", firstName);
      _logger.LogInformation("Asunto: ¡Bienvenido a Elixium Foods, {FirstName}!", firstName);
      _logger.LogInformation("=== FIN EMAIL SIMULADO ===");
    }
  }
}
