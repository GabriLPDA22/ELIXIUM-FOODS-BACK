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

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }
    }
}
