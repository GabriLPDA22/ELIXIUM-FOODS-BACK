using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UberEatsBackend.Models;
using UberEatsBackend.Utils;
using UberEatsBackend.Repositories;

namespace UberEatsBackend.Services
{
    public class TokenService
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<TokenService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TokenService(AppSettings appSettings, ILogger<TokenService> logger, IServiceProvider serviceProvider)
        {
            _appSettings = appSettings;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public (string accessToken, string refreshToken) GenerateTokens(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // ✅ GUARDAR REFRESH TOKEN DE FORMA SEGURA (en background)
            _ = Task.Run(async () => await SaveRefreshTokenSafeAsync(user.Id, refreshToken));

            return (accessToken, refreshToken);
        }

        public string GenerateAccessToken(User user)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_appSettings.JwtSecret);

                var claims = new List<Claim>
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Email", user.Email),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName),
                    new Claim("Role", user.Role),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                // Agregar BusinessId si existe
                if (user.Business?.Id != null)
                {
                    claims.Add(new Claim("BusinessId", user.Business.Id.ToString()));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(60), // Token válido por 1 hora
                    Issuer = _appSettings.JwtIssuer,
                    Audience = _appSettings.JwtAudience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Access token generado para usuario: {UserId}", user.Id);
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando access token para usuario: {UserId}", user.Id);
                throw;
            }
        }

        public string GenerateRefreshToken()
        {
            try
            {
                var randomNumber = new byte[64];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                }

                var refreshToken = Convert.ToBase64String(randomNumber)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");

                _logger.LogInformation("Refresh token generado");
                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando refresh token");
                throw;
            }
        }

        // ✅ MÉTODO QUE ESPERA TU AUTHSERVICE (SIN parámetros adicionales)
        public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // Crear scope fresh para evitar DbContext disposed
                using var scope = _serviceProvider.CreateScope();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var user = await userRepository.GetUserByRefreshTokenAsync(refreshToken);
                if (user == null)
                {
                    throw new SecurityTokenException("Refresh token inválido");
                }

                var newAccessToken = GenerateAccessToken(user);
                var newRefreshToken = GenerateRefreshToken();

                // Guardar nuevo refresh token
                await SaveRefreshTokenSafeAsync(user.Id, newRefreshToken);

                return (newAccessToken, newRefreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refrescando token");
                throw;
            }
        }

        // ✅ MÉTODO QUE ESPERA TU AUTHSERVICE
        public async Task RevokeTokenAsync(string refreshToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var user = await userRepository.GetUserByRefreshTokenAsync(refreshToken);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiry = null;
                    await userRepository.UpdateAsync(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revocando token");
                throw;
            }
        }

        // ✅ MÉTODO QUE ESPERA TU AUTHSERVICE
        public async Task<bool> ValidateTokenAsync(string token)
        {
            return await Task.FromResult(ValidateToken(token));
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_appSettings.JwtSecret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _appSettings.JwtIssuer,
                    ValidAudience = _appSettings.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token inválido");
                return false;
            }
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_appSettings.JwtSecret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // No validar expiración aquí
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _appSettings.JwtIssuer,
                    ValidAudience = _appSettings.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error obteniendo principal del token");
                return null;
            }
        }

        public DateTime? GetTokenExpiration(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                return jsonToken.ValidTo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error obteniendo expiración del token");
                return null;
            }
        }

        // ✅ MÉTODO PRIVADO SEGURO PARA GUARDAR REFRESH TOKEN
        private async Task SaveRefreshTokenSafeAsync(int userId, string refreshToken)
        {
            try
            {
                // Crear scope fresh para evitar problemas de DbContext disposed
                using var scope = _serviceProvider.CreateScope();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var user = await userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                    await userRepository.UpdateAsync(user);

                    _logger.LogInformation("Refresh token guardado exitosamente para usuario: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Usuario no encontrado para guardar refresh token: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando refresh token para usuario: {UserId}", userId);
                // No lanzar excepción para no romper el flujo principal
            }
        }
    }
}
