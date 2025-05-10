using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Auth;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
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
