using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Data;
using UberEatsBackend.DTOs.Auth;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;
using UberEatsBackend.Services;
using BC = BCrypt.Net.BCrypt;

namespace UberEatsBackend.Services
{
  public class AuthService
  {
    private readonly IUserRepository _userRepository;
    private readonly TokenService _tokenService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, TokenService tokenService, ApplicationDbContext context, ILogger<AuthService> logger)
    {
      _userRepository = userRepository;
      _tokenService = tokenService;
      _context = context;
      _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
      try
      {
        // Validar que el email no exista
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
          return new AuthResponseDto
          {
            Success = false,
            Message = "El correo electrónico ya está registrado"
          };
        }

        // Validar el rol
        string role = request.Role.ToLower() switch
        {
          "admin" => "Admin",
          "restaurant" => "Restaurant",
          "deliveryperson" => "DeliveryPerson",
          "business" => "Business",
          _ => "Customer"
        };

        // Crear el usuario
        var user = new User
        {
          Email = request.Email,
          PasswordHash = BC.HashPassword(request.Password),
          FirstName = request.FirstName,
          LastName = request.LastName,
          PhoneNumber = request.PhoneNumber,
          Role = role,
          CreatedAt = DateTime.UtcNow,
          UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        // Generar tokens
        var (accessToken, refreshToken) = _tokenService.GenerateTokens(user);

        return new AuthResponseDto
        {
          Success = true,
          Message = "Usuario registrado correctamente",
          Token = accessToken,
          RefreshToken = refreshToken,
          UserId = user.Id,
          Email = user.Email,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Role = user.Role,
          PhotoURL = user.PhotoURL,
          GoogleId = user.GoogleId
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en registro para email: {Email}", request.Email);
        return new AuthResponseDto
        {
          Success = false,
          Message = "Error interno del servidor"
        };
      }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
      try
      {
        // Buscar el usuario por email
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
          return new AuthResponseDto
          {
            Success = false,
            Message = "Credenciales inválidas"
          };
        }

        // Verificar la contraseña
        bool isPasswordValid = BC.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
          return new AuthResponseDto
          {
            Success = false,
            Message = "Credenciales inválidas"
          };
        }

        // Generar tokens
        var (accessToken, refreshToken) = _tokenService.GenerateTokens(user);

        // Información adicional para usuarios de tipo Business
        int? businessId = null;
        if (user.Role == "Business")
        {
          // Cargar el negocio asociado al usuario
          var business = await _context.Businesses
              .FirstOrDefaultAsync(b => b.UserId == user.Id);
          businessId = business?.Id;
        }

        return new AuthResponseDto
        {
          Success = true,
          Message = "Inicio de sesión exitoso",
          Token = accessToken,
          RefreshToken = refreshToken,
          UserId = user.Id,
          Email = user.Email,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Role = user.Role,
          BusinessId = businessId,
          PhotoURL = user.PhotoURL,
          GoogleId = user.GoogleId
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en login para email: {Email}", request.Email);
        return new AuthResponseDto
        {
          Success = false,
          Message = "Error interno del servidor"
        };
      }
    }

    // Método para autenticar usuarios de Google
    public async Task<AuthResponseDto> AuthenticateGoogleUserAsync(GoogleUserDto googleUser)
    {
      try
      {
        // Buscar si el usuario ya existe
        var existingUser = await _userRepository.GetByEmailAsync(googleUser.Email);

        User user;

        if (existingUser != null)
        {
          // Usuario existente - actualizar información de Google
          user = existingUser;

          // Actualizar GoogleId y foto si no los tiene o han cambiado
          bool needsUpdate = false;

          if (string.IsNullOrEmpty(user.GoogleId))
          {
            user.GoogleId = googleUser.GoogleId;
            needsUpdate = true;
          }

          // Actualizar foto de perfil si ha cambiado
          if (user.PhotoURL != googleUser.Picture && !string.IsNullOrEmpty(googleUser.Picture))
          {
            user.PhotoURL = googleUser.Picture;
            needsUpdate = true;
          }

          if (needsUpdate)
          {
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
          }

          Console.WriteLine($"Usuario existente encontrado: {user.Email}");
        }
        else
        {
          // Crear nuevo usuario desde Google
          user = new User
          {
            Email = googleUser.Email,
            FirstName = googleUser.FirstName,
            LastName = googleUser.LastName,
            GoogleId = googleUser.GoogleId,
            PhotoURL = googleUser.Picture, // Foto de Google
            Role = "Customer", // Por defecto
            PasswordHash = "", // Los usuarios de Google no tienen contraseña local
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
          };

          await _userRepository.AddAsync(user);
          Console.WriteLine($"Nuevo usuario creado desde Google: {user.Email}");
        }

        // Generar tokens
        var (accessToken, refreshToken) = _tokenService.GenerateTokens(user);

        // Información adicional para usuarios de tipo Business
        int? businessId = null;
        if (user.Role == "Business")
        {
          var business = await _context.Businesses
              .FirstOrDefaultAsync(b => b.UserId == user.Id);
          businessId = business?.Id;
        }

        return new AuthResponseDto
        {
          Success = true,
          Message = "Autenticación con Google exitosa",
          Token = accessToken,
          RefreshToken = refreshToken,
          UserId = user.Id,
          Email = user.Email,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Role = user.Role,
          BusinessId = businessId,
          PhotoURL = user.PhotoURL,
          GoogleId = user.GoogleId
        };
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error en AuthenticateGoogleUserAsync: {ex.Message}");
        return new AuthResponseDto
        {
          Success = false,
          Message = "Error interno durante la autenticación con Google"
        };
      }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
      try
      {
        // Obtener nuevos tokens
        var (accessToken, newRefreshToken) = await _tokenService.RefreshTokenAsync(refreshToken);

        return new AuthResponseDto
        {
          Success = true,
          Message = "Token actualizado correctamente",
          Token = accessToken,
          RefreshToken = newRefreshToken
        };
      }
      catch (Exception ex)
      {
        return new AuthResponseDto
        {
          Success = false,
          Message = $"Error al refrescar token: {ex.Message}"
        };
      }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
      try
      {
        await _tokenService.RevokeTokenAsync(refreshToken);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
      return await _tokenService.ValidateTokenAsync(token);
    }
  }
}
