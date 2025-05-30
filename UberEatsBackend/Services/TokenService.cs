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
        private readonly IUserRepository _userRepository;

        public TokenService(AppSettings appSettings, ILogger<TokenService> logger, IUserRepository userRepository)
        {
            _appSettings = appSettings;
            _logger = logger;
            _userRepository = userRepository;
        }

        public (string accessToken, string refreshToken) GenerateTokens(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Guardar refresh token en el usuario
            SaveRefreshToken(user.Id, refreshToken);

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

        public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
                if (user == null)
                {
                    throw new SecurityTokenException("Refresh token inválido");
                }

                var newAccessToken = GenerateAccessToken(user);
                var newRefreshToken = GenerateRefreshToken();

                // Actualizar el refresh token en la base de datos
                await SaveRefreshTokenAsync(user.Id, newRefreshToken);

                return (newAccessToken, newRefreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refrescando token");
                throw;
            }
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiry = null;
                    await _userRepository.UpdateAsync(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revocando token");
                throw;
            }
        }

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

        private void SaveRefreshToken(int userId, string refreshToken)
        {
            Task.Run(async () => await SaveRefreshTokenAsync(userId, refreshToken));
        }

        private async Task SaveRefreshTokenAsync(int userId, string refreshToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                    await _userRepository.UpdateAsync(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando refresh token para usuario: {UserId}", userId);
            }
        }
    }
}
