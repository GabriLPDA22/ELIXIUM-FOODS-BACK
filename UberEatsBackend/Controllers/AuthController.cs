using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using UberEatsBackend.DTOs.Auth;
using UberEatsBackend.DTOs.User;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly AuthService _authService;
    private readonly IUserService _userService;
    private readonly TokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly GoogleAuthService _googleAuthService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthService authService,
        IUserService userService,
        TokenService tokenService,
        IEmailService emailService,
        GoogleAuthService googleAuthService,
        ILogger<AuthController> logger)

    {
      _authService = authService;
      _userService = userService;
      _tokenService = tokenService;
      _emailService = emailService;
      _logger = logger;
      _googleAuthService = googleAuthService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
      try
      {
        if (!ModelState.IsValid)
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "Datos de registro inválidos"
          });
        }

        // Verificar si el email ya existe
        if (await _userService.EmailExistsAsync(request.Email))
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "El email ya está registrado"
          });
        }

        // Crear usuario
        var createUserDto = new CreateUserDto
        {
          Email = request.Email,
          Password = request.Password,
          FirstName = request.FirstName,
          LastName = request.LastName,
          PhoneNumber = request.PhoneNumber,
          Role = request.Role
        };

        var createdUser = await _userService.CreateUserAsync(createUserDto);

        // Obtener el usuario entity para generar tokens
        var userEntity = await _userService.GetUserEntityByIdAsync(createdUser.Id);
        if (userEntity == null)
        {
          return StatusCode(500, new AuthResponseDto
          {
            Success = false,
            Message = "Error interno del servidor"
          });
        }

        // Generar tokens
        var accessToken = _tokenService.GenerateAccessToken(userEntity);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Guardar refresh token
        await _userService.SetRefreshTokenAsync(userEntity.Id, refreshToken, DateTime.UtcNow.AddDays(7));

        _logger.LogInformation("Usuario registrado exitosamente: {Email}", request.Email);

        return Ok(new AuthResponseDto
        {
          Success = true,
          Message = "Registro exitoso",
          Token = accessToken,
          RefreshToken = refreshToken,
          UserId = userEntity.Id,
          Email = userEntity.Email,
          FirstName = userEntity.FirstName,
          LastName = userEntity.LastName,
          Role = userEntity.Role,
          BusinessId = userEntity.Business?.Id
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en registro para email: {Email}", request.Email);
        return StatusCode(500, new AuthResponseDto
        {
          Success = false,
          Message = "Error interno del servidor"
        });
      }
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


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
      try
      {
        if (!ModelState.IsValid)
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "Datos de login inválidos"
          });
        }

        var user = await _userService.AuthenticateAsync(request.Email, request.Password);
        if (user == null)
        {
          return Unauthorized(new AuthResponseDto
          {
            Success = false,
            Message = "Email o contraseña incorrectos"
          });
        }

        // Generar tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Guardar refresh token
        await _userService.SetRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));

        _logger.LogInformation("Usuario logueado exitosamente: {Email}", request.Email);

        return Ok(new AuthResponseDto
        {
          Success = true,
          Message = "Login exitoso",
          Token = accessToken,
          RefreshToken = refreshToken,
          UserId = user.Id,
          Email = user.Email,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Role = user.Role,
          BusinessId = user.Business?.Id
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en login para email: {Email}", request.Email);
        return StatusCode(500, new AuthResponseDto
        {
          Success = false,
          Message = "Error interno del servidor"
        });
      }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
      try
      {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "Refresh token es requerido"
          });
        }

        var user = await _userService.GetUserByRefreshTokenAsync(request.RefreshToken);
        if (user == null)
        {
          return Unauthorized(new AuthResponseDto
          {
            Success = false,
            Message = "Refresh token inválido o expirado"
          });
        }

        // Generar nuevos tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Actualizar refresh token
        await _userService.SetRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(7));

        _logger.LogInformation("Tokens refrescados para usuario: {UserId}", user.Id);

        return Ok(new AuthResponseDto
        {
          Success = true,
          Message = "Tokens refrescados exitosamente",
          Token = newAccessToken,
          RefreshToken = newRefreshToken,
          UserId = user.Id,
          Email = user.Email,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Role = user.Role,
          BusinessId = user.Business?.Id
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error refrescando token");
        return StatusCode(500, new AuthResponseDto
        {
          Success = false,
          Message = "Error interno del servidor"
        });
      }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
      try
      {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
          await _userService.ClearRefreshTokenAsync(userId);
          _logger.LogInformation("Usuario deslogueado: {UserId}", userId);
        }

        return Ok(new AuthResponseDto
        {
          Success = true,
          Message = "Logout exitoso"
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en logout");
        return StatusCode(500, new AuthResponseDto
        {
          Success = false,
          Message = "Error interno del servidor"
        });
      }
    }

    [HttpPost("validate-token")]
    public IActionResult ValidateToken([FromBody] ValidateTokenRequestDto request)
    {
      try
      {
        if (string.IsNullOrEmpty(request.Token))
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "Token es requerido"
          });
        }

        var isValid = _tokenService.ValidateToken(request.Token);

        return Ok(new AuthResponseDto
        {
          Success = isValid,
          Message = isValid ? "Token válido" : "Token inválido"
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error validando token");
        return Ok(new AuthResponseDto
        {
          Success = false,
          Message = "Token inválido"
        });
      }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
      try
      {
        if (string.IsNullOrEmpty(request.Email))
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "Email es requerido"
          });
        }

        var user = await _userService.GetUserEntityByEmailAsync(request.Email);
        if (user == null)
        {
          // Por seguridad, devolvemos éxito aunque el usuario no exista
          return Ok(new AuthResponseDto
          {
            Success = true,
            Message = "Si el email existe, se enviaron las instrucciones de restablecimiento"
          });
        }

        // Generar token de reset
        var resetToken = GeneratePasswordResetToken();
        var resetTokenExpiry = DateTime.UtcNow.AddHours(1); // Válido por 1 hora

        // Actualizar usuario con token de reset
        await _userService.SetPasswordResetTokenAsync(user.Id, resetToken, resetTokenExpiry);

        // ✅ ENVIAR EL EMAIL CON URL COMPLETA (token + email)
        try
        {
          // ✅ INCLUIR EMAIL EN LA URL
          var resetUrl = $"http://localhost:5173/reset-password?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";
          var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, "http://localhost:5173/reset-password");

          if (emailSent)
          {
            _logger.LogInformation("✅ Email de reset enviado exitosamente a: {Email}", user.Email);
          }
          else
          {
            _logger.LogWarning("⚠️ No se pudo enviar email de reset a: {Email}", user.Email);
          }
        }
        catch (Exception emailEx)
        {
          _logger.LogError(emailEx, "🔴 Error enviando email de reset a: {Email}", user.Email);
          // No fallar el proceso por el email
        }

        // ✅ LOG PARA DEBUGGING CON EMAIL EN LA URL
        _logger.LogInformation("Password reset token generated for user {Email}: {Token}", user.Email, resetToken);
        _logger.LogInformation("✅ Reset URL completa: http://localhost:5173/reset-password?token={Token}&email={Email}", resetToken, Uri.EscapeDataString(user.Email));

        return Ok(new AuthResponseDto
        {
          Success = true,
          Message = "Si el email existe, se enviaron las instrucciones de restablecimiento"
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en forgot password para email: {Email}", request.Email);
        return StatusCode(500, new AuthResponseDto
        {
          Success = false,
          Message = "Error interno del servidor"
        });
      }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
      try
      {
        if (string.IsNullOrEmpty(request.Token) ||
            string.IsNullOrEmpty(request.Email) ||
            string.IsNullOrEmpty(request.NewPassword))
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "Token, email y nueva contraseña son requeridos"
          });
        }

        var user = await _userService.GetUserEntityByEmailAsync(request.Email);
        if (user == null)
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "Token inválido o expirado"
          });
        }

        // Verificar token de reset
        if (user.PasswordResetToken != request.Token ||
            user.PasswordResetTokenExpiry == null ||
            user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "Token inválido o expirado"
          });
        }

        // Actualizar contraseña
        await _userService.UpdatePasswordAsync(user.Id, request.NewPassword);

        // Limpiar token de reset
        await _userService.ClearPasswordResetTokenAsync(user.Id);

        _logger.LogInformation($"Password reset successfully for user: {user.Email}");

        return Ok(new AuthResponseDto
        {
          Success = true,
          Message = "Contraseña actualizada exitosamente"
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en reset password");
        return StatusCode(500, new AuthResponseDto
        {
          Success = false,
          Message = "Error interno del servidor"
        });
      }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
      try
      {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
          return Unauthorized(new AuthResponseDto
          {
            Success = false,
            Message = "Usuario no autenticado"
          });
        }

        if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "Contraseña actual y nueva contraseña son requeridas"
          });
        }

        var success = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

        if (!success)
        {
          return BadRequest(new AuthResponseDto
          {
            Success = false,
            Message = "Contraseña actual incorrecta"
          });
        }

        _logger.LogInformation("Contraseña cambiada exitosamente para usuario: {UserId}", userId);

        return Ok(new AuthResponseDto
        {
          Success = true,
          Message = "Contraseña actualizada exitosamente"
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error cambiando contraseña");
        return StatusCode(500, new AuthResponseDto
        {
          Success = false,
          Message = "Error interno del servidor"
        });
      }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
      try
      {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
          return Unauthorized(new AuthResponseDto
          {
            Success = false,
            Message = "Usuario no autenticado"
          });
        }

        var userDto = await _userService.GetUserByIdAsync(userId);
        if (userDto == null)
        {
          return NotFound(new AuthResponseDto
          {
            Success = false,
            Message = "Usuario no encontrado"
          });
        }

        return Ok(new { Success = true, Data = userDto });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error obteniendo usuario actual");
        return StatusCode(500, new AuthResponseDto
        {
          Success = false,
          Message = "Error interno del servidor"
        });
      }
    }

    private string GeneratePasswordResetToken()
    {
      // Generar un token seguro de 32 bytes
      var tokenBytes = new byte[32];
      using (var rng = RandomNumberGenerator.Create())
      {
        rng.GetBytes(tokenBytes);
      }
      return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
  }
}
