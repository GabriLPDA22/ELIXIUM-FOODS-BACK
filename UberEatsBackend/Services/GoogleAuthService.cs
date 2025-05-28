// UberEatsBackend/Services/GoogleAuthService.cs
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Auth;
using UberEatsBackend.Utils;

namespace UberEatsBackend.Services
{
  public class GoogleAuthService
  {
    private readonly AppSettings _appSettings;

    public GoogleAuthService(IOptions<AppSettings> appSettings)
    {
      _appSettings = appSettings.Value;
    }

    public async Task<GoogleUserDto?> VerifyGoogleTokenAsync(string idToken)
    {
      try
      {
        Console.WriteLine("Verificando token de Google...");

        // Validar el token con Google usando tu Client ID
        var payload = await GoogleJsonWebSignature.ValidateAsync(
            idToken,
            new GoogleJsonWebSignature.ValidationSettings()
            {
              Audience = new[] { "385291527056-q394iq6ki0uts61ra7ctomfohrqanusf.apps.googleusercontent.com" }
            });

        if (payload == null)
        {
          Console.WriteLine("❌ Payload de Google es null");
          return null;
        }

        Console.WriteLine($"✅ Token de Google verificado exitosamente para: {payload.Email}");

        // Extraer nombres
        var names = ExtractNames(payload.Name ?? "");

        return new GoogleUserDto
        {
          Email = payload.Email,
          FirstName = names.firstName,
          LastName = names.lastName,
          GoogleId = payload.Subject,
          Picture = payload.Picture ?? "",
          EmailVerified = payload.EmailVerified
        };
      }
      catch (InvalidJwtException ex)
      {
        Console.WriteLine($"❌ Token JWT de Google inválido: {ex.Message}");
        return null;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Error verificando token de Google: {ex.Message}");
        return null;
      }
    }

    private (string firstName, string lastName) ExtractNames(string fullName)
    {
      if (string.IsNullOrEmpty(fullName))
        return ("Usuario", "Google");

      var nameParts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

      if (nameParts.Length == 0)
        return ("Usuario", "Google");

      if (nameParts.Length == 1)
        return (nameParts[0], "");

      // Si hay 2 o más partes, tomar la primera como nombre y el resto como apellido
      var firstName = nameParts[0];
      var lastName = string.Join(" ", nameParts[1..]);

      return (firstName, lastName);
    }
  }
}
