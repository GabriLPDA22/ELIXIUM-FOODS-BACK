using System;
using System.Threading.Tasks;
using AutoMapper;
using UberEatsBackend.DTOs.User;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;
using BC = BCrypt.Net.BCrypt;

namespace UberEatsBackend.Services
{
  public class UserService : IUserService
  {
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
      _userRepository = userRepository;
      _mapper = mapper;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
      return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
      return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<User> CreateUserAsync(CreateUserDto userDto)
    {
      var user = _mapper.Map<User>(userDto);
      user.PasswordHash = BC.HashPassword(userDto.Password);
      user.CreatedAt = DateTime.UtcNow;
      user.UpdatedAt = DateTime.UtcNow;

      await _userRepository.AddAsync(user);
      return user;
    }

    public async Task<User?> UpdateUserAsync(int userId, UpdateUserDto userDto)
    {
      var user = await _userRepository.GetByIdAsync(userId);
      if (user == null)
        return null;

      _mapper.Map(userDto, user);
      if (!string.IsNullOrEmpty(userDto.Role))
      {
        user.Role = userDto.Role;
      }

      user.UpdatedAt = DateTime.UtcNow;

      await _userRepository.UpdateAsync(user);
      return user;
    }

    public async Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileDto profileDto)
    {
      var user = await _userRepository.GetByIdAsync(userId);
      if (user == null)
        return false;

      _mapper.Map(profileDto, user);
      user.UpdatedAt = DateTime.UtcNow;

      await _userRepository.UpdateAsync(user);
      return true;
    }

    public async Task<bool> UpdateUserPasswordAsync(int userId, string currentPassword, string newPassword)
    {
      var user = await _userRepository.GetByIdAsync(userId);
      if (user == null)
        return false;

      // Verificar la contraseña actual
      if (!BC.Verify(currentPassword, user.PasswordHash))
        return false;

      // Actualizar a la nueva contraseña
      user.PasswordHash = BC.HashPassword(newPassword);
      user.UpdatedAt = DateTime.UtcNow;

      await _userRepository.UpdateAsync(user);
      return true;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
      var user = await _userRepository.GetByIdAsync(userId);
      if (user == null)
        return false;

      await _userRepository.DeleteAsync(user);
      return true;
    }

    public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
    {
      var user = await _userRepository.GetByIdAsync(userId);
      if (user == null)
        return false;

      user.Role = newRole;
      user.UpdatedAt = DateTime.UtcNow;

      await _userRepository.UpdateAsync(user);
      return true;
    }
  }
}
