using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Auth;
using UberEatsBackend.Services;
using System;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly GoogleAuthService _googleAuthService;

        public AuthController(AuthService authService, GoogleAuthService googleAuthService)
        {
            _authService = authService;
            _googleAuthService = googleAuthService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
                return BadRequest(result);

            // Mostrar más información sobre el token generado
            var tokenInfo = $"Token generado: {result.Token.Substring(0, Math.Min(20, result.Token.Length))}...";
            var refreshTokenInfo = $"RefreshToken generado: {result.RefreshToken.Substring(0, Math.Min(20, result.RefreshToken.Length))}...";

            Console.WriteLine($"Registro exitoso para {request.Email}");
            Console.WriteLine(tokenInfo);
            Console.WriteLine(refreshTokenInfo);
            Console.WriteLine($"JWT partes: {result.Token.Split('.').Length}");

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                return Unauthorized(result);

            // Mostrar más información sobre el token generado
            var tokenInfo = $"Token JWT generado: {result.Token.Substring(0, Math.Min(20, result.Token.Length))}...";
            var refreshTokenInfo = $"RefreshToken generado: {result.RefreshToken.Substring(0, Math.Min(20, result.RefreshToken.Length))}...";

            Console.WriteLine($"Login exitoso para {request.Email}");
            Console.WriteLine(tokenInfo);
            Console.WriteLine(refreshTokenInfo);
            Console.WriteLine($"JWT partes: {result.Token.Split('.').Length}");

            return Ok(result);
        }

        // NUEVO: Endpoint para Google Login
        [HttpPost("google-login")]
        public async Task<ActionResult<AuthResponseDto>> GoogleLogin(GoogleLoginRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Token de Google es requerido"
                    });
                }

                Console.WriteLine($"Recibido token de Google: {request.IdToken.Substring(0, Math.Min(50, request.IdToken.Length))}...");

                // Verificar el token con Google
                var googleUser = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
                if (googleUser == null)
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Token de Google inválido"
                    });
                }

                Console.WriteLine($"Token de Google verificado para: {googleUser.Email}");

                // Autenticar o registrar al usuario
                var result = await _authService.AuthenticateGoogleUserAsync(googleUser);

                if (!result.Success)
                    return BadRequest(result);

                Console.WriteLine($"Login con Google exitoso para {googleUser.Email}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Google Login: {ex.Message}");
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Error interno del servidor durante la autenticación con Google"
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new AuthResponseDto {
                    Success = false,
                    Message = "Refresh token es requerido"
                });

            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken(RefreshTokenRequestDto request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { message = "Refresh token es requerido" });

            var success = await _authService.RevokeTokenAsync(request.RefreshToken);

            if (!success)
                return NotFound(new { message = "Token no encontrado" });

            return Ok(new { message = "Token revocado correctamente" });
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken(ValidateTokenRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Token))
                return BadRequest(new { message = "Token es requerido" });

            var isValid = await _authService.ValidateTokenAsync(request.Token);

            return Ok(new { isValid });
        }

        [HttpGet("debug-token")]
        public IActionResult DebugToken([FromQuery] string token)
        {
            // Verificar formato JWT
            bool isJwtFormat = token.Count(c => c == '.') == 2;

            // Devolver información de diagnóstico
            return Ok(new
            {
                token,
                isJwtFormat,
                jwtParts = isJwtFormat ? token.Split('.').Length : 0,
                tokenType = isJwtFormat ? "JWT" : "No es un JWT válido"
            });
        }
    }
}
