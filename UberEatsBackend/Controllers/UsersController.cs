using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.User;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;
using UberEatsBackend.Services;
using BC = BCrypt.Net.BCrypt;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class UsersController : ControllerBase
  {
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
      _userRepository = userRepository;
      _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
      var users = await _userRepository.GetAllAsync();
      return Ok(_mapper.Map<IEnumerable<UserDto>>(users));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
    {
      // Verificar si el email ya existe
      var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
      if (existingUser != null)
        return BadRequest(new { message = "El correo electrónico ya está registrado" });

      // Validar el rol
      string role = createUserDto.Role.ToLower() switch
      {
        "admin" => "Admin",
        "restaurant" => "Restaurant",
        "deliveryperson" => "DeliveryPerson",
        _ => "Customer"
      };

      // Crear el usuario
      var user = new User
      {
        Email = createUserDto.Email,
        PasswordHash = BC.HashPassword(createUserDto.Password),
        FirstName = createUserDto.FirstName,
        LastName = createUserDto.LastName,
        PhoneNumber = createUserDto.PhoneNumber,
        Role = role
      };

      var createdUser = await _userRepository.AddAsync(user);

      return CreatedAtAction(
          nameof(GetUserById),
          new { id = createdUser.Id },
          _mapper.Map<UserDto>(createdUser)
      );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
      // Verificar que el usuario logueado pueda acceder a este recurso
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

      if (userId != id && userRole != "Admin")
        return Forbid();

      var user = await _userRepository.GetWithAddressesAsync(id);

      if (user == null)
        return NotFound();

      return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
    {
      // Verificar que el usuario logueado pueda actualizar este recurso
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

      if (userId != id && userRole != "Admin")
        return Forbid();

      var user = await _userRepository.GetByIdAsync(id);

      if (user == null)
        return NotFound();

      user.FirstName = updateUserDto.FirstName;
      user.LastName = updateUserDto.LastName;
      user.PhoneNumber = updateUserDto.PhoneNumber;
      user.UpdatedAt = System.DateTime.UtcNow;

      await _userRepository.UpdateAsync(user);

      return NoContent();
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileDto updateProfileDto)
    {
      // Obtener ID del usuario actual
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var user = await _userRepository.GetByIdAsync(userId);

      if (user == null)
        return NotFound(new { message = "Usuario no encontrado" });

      // Actualizar campos básicos
      user.FirstName = updateProfileDto.FirstName;
      user.LastName = updateProfileDto.LastName;
      user.PhoneNumber = updateProfileDto.PhoneNumber;

      // Campos opcionales
      if (!string.IsNullOrEmpty(updateProfileDto.Birthdate))
      {
        if (DateTime.TryParse(updateProfileDto.Birthdate, out DateTime birthdate))
        {
          user.Birthdate = birthdate;
        }
      }

      user.Bio = updateProfileDto.Bio;
      user.PhotoURL = updateProfileDto.PhotoURL;

      // Preferencias alimentarias (guardar como JSON)
      if (updateProfileDto.DietaryPreferences != null && updateProfileDto.DietaryPreferences.Count > 0)
      {
        user.DietaryPreferencesJson = JsonSerializer.Serialize(updateProfileDto.DietaryPreferences);
      }
      else
      {
        user.DietaryPreferencesJson = null;
      }

      user.UpdatedAt = DateTime.UtcNow;
      await _userRepository.UpdateAsync(user);

      // Convertir a DTO y devolver
      var userDto = _mapper.Map<UserDto>(user);

      // Agregar campos adicionales si es necesario
      if (user.Birthdate.HasValue)
      {
        userDto.Birthdate = user.Birthdate.Value.ToString("yyyy-MM-dd");
      }

      userDto.Bio = user.Bio;
      userDto.PhotoURL = user.PhotoURL;

      // Deserializar preferencias alimentarias
      if (!string.IsNullOrEmpty(user.DietaryPreferencesJson))
      {
        userDto.DietaryPreferences = JsonSerializer.Deserialize<List<string>>(user.DietaryPreferencesJson);
      }

      return Ok(userDto);
    }

    [HttpPut("{id}/password")]
    public async Task<IActionResult> UpdatePassword(int id, UpdatePasswordDto updatePasswordDto)
    {
      // Verificar que el usuario logueado pueda actualizar la contraseña
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

      if (userId != id)
        return Forbid();

      var user = await _userRepository.GetByIdAsync(id);

      if (user == null)
        return NotFound();

      // Verificar contraseña actual
      bool isCurrentPasswordValid = BC.Verify(updatePasswordDto.CurrentPassword, user.PasswordHash);
      if (!isCurrentPasswordValid)
        return BadRequest(new { message = "La contraseña actual es incorrecta" });

      // Actualizar contraseña
      user.PasswordHash = BC.HashPassword(updatePasswordDto.NewPassword);
      user.UpdatedAt = System.DateTime.UtcNow;
      await _userRepository.UpdateAsync(user);

      return NoContent();
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
      // Obtener ID del usuario actual
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var user = await _userRepository.GetByIdAsync(userId);

      if (user == null)
        return NotFound(new { message = "Usuario no encontrado" });

      // Verificar contraseña actual
      bool isCurrentPasswordValid = BC.Verify(changePasswordDto.CurrentPassword, user.PasswordHash);
      if (!isCurrentPasswordValid)
        return BadRequest(new { message = "La contraseña actual es incorrecta" });

      // Actualizar contraseña
      user.PasswordHash = BC.HashPassword(changePasswordDto.NewPassword);
      user.UpdatedAt = System.DateTime.UtcNow;
      await _userRepository.UpdateAsync(user);

      return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
      var user = await _userRepository.GetByIdAsync(id);

      if (user == null)
        return NotFound();

      await _userRepository.DeleteAsync(user);

      return NoContent();
    }

    [HttpGet("byRole/{role}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(string role)
    {
      var users = await _userRepository.GetByRoleAsync(role);
      return Ok(_mapper.Map<IEnumerable<UserDto>>(users));
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var user = await _userRepository.GetWithAddressesAsync(userId);

      if (user == null)
        return NotFound();

      var userDto = _mapper.Map<UserDto>(user);

      // Agregar campos adicionales
      if (user.Birthdate.HasValue)
      {
        userDto.Birthdate = user.Birthdate.Value.ToString("yyyy-MM-dd");
      }

      userDto.Bio = user.Bio;
      userDto.PhotoURL = user.PhotoURL;

      // Deserializar preferencias alimentarias
      if (!string.IsNullOrEmpty(user.DietaryPreferencesJson))
      {
        userDto.DietaryPreferences = JsonSerializer.Deserialize<List<string>>(user.DietaryPreferencesJson);
      }

      return Ok(userDto);
    }
  }
}
