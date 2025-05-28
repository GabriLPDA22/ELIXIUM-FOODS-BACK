using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UberEatsBackend.DTOs.User;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;

namespace UberEatsBackend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        #region Métodos básicos CRUD

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null) return null;

                var userDto = _mapper.Map<UserDto>(user);

                // Deserializar dietary preferences si existen
                if (!string.IsNullOrEmpty(user.DietaryPreferencesJson))
                {
                    try
                    {
                        userDto.DietaryPreferences = JsonConvert.DeserializeObject<List<string>>(user.DietaryPreferencesJson);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Error deserializando dietary preferences para user {UserId}", id);
                        userDto.DietaryPreferences = new List<string>();
                    }
                }

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario por ID: {UserId}", id);
                throw;
            }
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null) return null;

                var userDto = _mapper.Map<UserDto>(user);

                // Deserializar dietary preferences si existen
                if (!string.IsNullOrEmpty(user.DietaryPreferencesJson))
                {
                    try
                    {
                        userDto.DietaryPreferences = JsonConvert.DeserializeObject<List<string>>(user.DietaryPreferencesJson);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Error deserializando dietary preferences para user {Email}", email);
                        userDto.DietaryPreferences = new List<string>();
                    }
                }

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario por email: {Email}", email);
                throw;
            }
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                return _mapper.Map<List<UserDto>>(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todos los usuarios");
                throw;
            }
        }

        public async Task<List<UserDto>> GetUsersByRoleAsync(string role)
        {
            try
            {
                var users = await _userRepository.GetUsersByRoleAsync(role);
                return _mapper.Map<List<UserDto>>(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios por rol: {Role}", role);
                throw;
            }
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Verificar si el email ya existe
                if (await _userRepository.EmailExistsAsync(createUserDto.Email))
                {
                    throw new ArgumentException("El email ya está registrado");
                }

                var user = _mapper.Map<User>(createUserDto);
                user.PasswordHash = HashPassword(createUserDto.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                var createdUser = await _userRepository.CreateAsync(user);
                _logger.LogInformation("Usuario creado exitosamente: {Email}", createdUser.Email);

                return _mapper.Map<UserDto>(createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario: {Email}", createUserDto.Email);
                throw;
            }
        }

        public async Task UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new ArgumentException("Usuario no encontrado");
                }

                _mapper.Map(updateUserDto, user);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Usuario actualizado exitosamente: {UserId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando usuario: {UserId}", id);
                throw;
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new ArgumentException("Usuario no encontrado");
                }

                await _userRepository.DeleteAsync(id);
                _logger.LogInformation("Usuario eliminado exitosamente: {UserId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando usuario: {UserId}", id);
                throw;
            }
        }

        #endregion

        #region Métodos de autenticación

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return null;
                }

                if (!ValidatePassword(password, user.PasswordHash))
                {
                    return null;
                }

                _logger.LogInformation("Usuario autenticado exitosamente: {Email}", email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en autenticación para email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await _userRepository.EmailExistsAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando existencia de email: {Email}", email);
                throw;
            }
        }

        public async Task<User?> GetUserEntityByEmailAsync(string email)
        {
            try
            {
                return await _userRepository.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo entidad de usuario por email: {Email}", email);
                throw;
            }
        }

        public async Task<User?> GetUserEntityByIdAsync(int id)
        {
            try
            {
                return await _userRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo entidad de usuario por ID: {UserId}", id);
                throw;
            }
        }

        #endregion

        #region Métodos de perfil

        public async Task UpdateProfileAsync(int userId, UpdateProfileDto updateProfileDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("Usuario no encontrado");
                }

                // Mapear campos básicos
                user.FirstName = updateProfileDto.FirstName;
                user.LastName = updateProfileDto.LastName;
                user.PhoneNumber = updateProfileDto.PhoneNumber;
                user.Bio = updateProfileDto.Bio;
                user.PhotoURL = updateProfileDto.PhotoURL;

                // Manejar birthdate
                if (!string.IsNullOrEmpty(updateProfileDto.Birthdate))
                {
                    if (DateTime.TryParse(updateProfileDto.Birthdate, out DateTime birthdate))
                    {
                        user.Birthdate = birthdate;
                    }
                }

                // Serializar dietary preferences
                if (updateProfileDto.DietaryPreferences != null && updateProfileDto.DietaryPreferences.Any())
                {
                    user.DietaryPreferencesJson = JsonConvert.SerializeObject(updateProfileDto.DietaryPreferences);
                }
                else
                {
                    user.DietaryPreferencesJson = null;
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Perfil actualizado exitosamente para usuario: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando perfil del usuario: {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDto?> GetUserWithAddressesAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserWithAddressesAsync(userId);
                if (user == null) return null;

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario con direcciones: {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDto?> GetUserWithBusinessAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetUserWithBusinessAsync(userId);
                if (user == null) return null;

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario con negocio: {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Métodos de contraseña

        public async Task UpdatePasswordAsync(int userId, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("Usuario no encontrado");
                }

                user.PasswordHash = HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                // Por seguridad, limpiar tokens de refresh existentes
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Contraseña actualizada para usuario: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando contraseña del usuario: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Verificar contraseña actual
                if (!ValidatePassword(currentPassword, user.PasswordHash))
                {
                    return false;
                }

                await UpdatePasswordAsync(userId, newPassword);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando contraseña del usuario: {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateUserRoleAsync(int userId, string newRole)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("Usuario no encontrado");
                }

                user.Role = newRole;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Rol actualizado para usuario: {UserId} a {Role}", userId, newRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando rol del usuario: {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Métodos de refresh token

        public async Task SetRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("Usuario no encontrado");
                }

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = expiry;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Refresh token establecido para usuario: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo refresh token para usuario: {UserId}", userId);
                throw;
            }
        }

        public async Task ClearRefreshTokenAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("Usuario no encontrado");
                }

                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Refresh token limpiado para usuario: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error limpiando refresh token para usuario: {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            try
            {
                return await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario por refresh token");
                throw;
            }
        }

        #endregion

        #region Métodos de password reset

        public async Task SetPasswordResetTokenAsync(int userId, string resetToken, DateTime expiry)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("Usuario no encontrado");
                }

                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpiry = expiry;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Password reset token establecido para usuario: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo password reset token para usuario: {UserId}", userId);
                throw;
            }
        }

        public async Task ClearPasswordResetTokenAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("Usuario no encontrado");
                }

                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Password reset token limpiado para usuario: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error limpiando password reset token para usuario: {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetUserByPasswordResetTokenAsync(string resetToken)
        {
            try
            {
                return await _userRepository.GetUserByPasswordResetTokenAsync(resetToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario por password reset token");
                throw;
            }
        }

        #endregion

        #region Métodos de estadísticas

        public async Task<int> GetUserCountByRoleAsync(string role)
        {
            try
            {
                return await _userRepository.GetUserCountByRoleAsync(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo conteo de usuarios por rol: {Role}", role);
                throw;
            }
        }

        public async Task<List<UserDto>> GetActiveUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetActiveUsersAsync();
                return _mapper.Map<List<UserDto>>(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios activos");
                throw;
            }
        }

        #endregion

        #region Métodos de validación

        public bool ValidatePassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando contraseña");
                return false;
            }
        }

        public string HashPassword(string password)
        {
            try
            {
                return BCrypt.Net.BCrypt.HashPassword(password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hasheando contraseña");
                throw;
            }
        }

        #endregion
    }
}
