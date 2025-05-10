// Services/TokenService.cs
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.Models;
using UberEatsBackend.Utils;
using Microsoft.EntityFrameworkCore;

namespace UberEatsBackend.Services
{
  public class TokenService
  {
    private readonly AppSettings _appSettings;
    private readonly ApplicationDbContext _context;

    public TokenService(IOptions<AppSettings> appSettings, ApplicationDbContext context)
    {
      _appSettings = appSettings.Value;
      _context = context;
    }

    public (string accessToken, string refreshToken) GenerateTokens(User user)
    {
      // Generar JWT
      var accessToken = GenerateToken(user);

      // Generar refresh token
      var refreshToken = GenerateRefreshToken();

      // Guardar refresh token en el usuario
      user.RefreshToken = refreshToken;
      user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30); // Expira en 30 días

      _context.SaveChanges();

      return (accessToken, refreshToken);
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
          expires: DateTime.UtcNow.AddHours(2), // Token válido por 2 horas
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
      // Generar un token aleatorio seguro
      var randomNumber = new byte[32];
      using var rng = RandomNumberGenerator.Create();
      rng.GetBytes(randomNumber);
      return Convert.ToBase64String(randomNumber);
    }

    public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken)
    {
      // Buscar usuario con este refresh token
      var user = await _context.Users
        .SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);

      if (user == null)
        throw new SecurityTokenException("Token de refresco inválido");

      // Verificar si el token está expirado
      if (user.RefreshTokenExpiry < DateTime.UtcNow)
        throw new SecurityTokenException("Token de refresco expirado");

      // Generar nuevo par de tokens
      return GenerateTokens(user);
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
      // Buscar usuario con este refresh token
      var user = await _context.Users
        .SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);

      if (user == null)
        return;

      // Revocar token
      user.RefreshToken = null;
      user.RefreshTokenExpiry = null;

      await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
      try
      {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.JwtSecret);

        // Validar token
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = true,
          ValidIssuer = _appSettings.JwtIssuer,
          ValidateAudience = true,
          ValidAudience = _appSettings.JwtAudience,
          ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}
