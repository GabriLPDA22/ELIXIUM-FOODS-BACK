using System;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Auth;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;
using BC = BCrypt.Net.BCrypt;

namespace UberEatsBackend.Services
{
  public class AuthService
  {
    private readonly IUserRepository _userRepository;
    private readonly TokenService _tokenService;

    public AuthService(IUserRepository userRepository, TokenService tokenService)
    {
      _userRepository = userRepository;
      _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
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

      // Generar token
      string token = _tokenService.GenerateToken(user);

      return new AuthResponseDto
      {
        Success = true,
        Message = "Usuario registrado correctamente",
        Token = token,
        UserId = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role
      };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
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

      // Generar token
      string token = _tokenService.GenerateToken(user);

      return new AuthResponseDto
      {
        Success = true,
        Message = "Inicio de sesión exitoso",
        Token = token,
        UserId = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role
      };
    }
  }
}
