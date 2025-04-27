using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UberEatsBackend.Models;
using UberEatsBackend.Utils;

namespace UberEatsBackend.Services
{
  public class TokenService
  {
    private readonly AppSettings _appSettings;

    public TokenService(IOptions<AppSettings> appSettings)
    {
      _appSettings = appSettings.Value;
    }

    public string GenerateToken(User user)
    {
      // Crear los claims del token
      var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role)
            };

      // Crear las credenciales de firma
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JwtSecret));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      // Crear el token
      var token = new JwtSecurityToken(
          issuer: _appSettings.JwtIssuer,
          audience: _appSettings.JwtAudience,
          claims: claims,
          expires: DateTime.Now.AddDays(7), // Token válido por 7 días
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
