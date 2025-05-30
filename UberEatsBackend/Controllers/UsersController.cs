using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UberEatsBackend.DTOs.User;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(new { Success = true, Data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todos los usuarios");
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                // Solo admins pueden ver cualquier usuario, otros solo pueden verse a sí mismos
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                var currentUserRole = User.FindFirst("Role")?.Value;

                if (currentUserRole != "Admin" &&
                    (!int.TryParse(currentUserIdClaim, out int currentUserId) || currentUserId != id))
                {
                    return Forbid();
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { Success = false, Message = "Usuario no encontrado" });
                }

                return Ok(new { Success = true, Data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario por ID: {UserId}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("by-email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { Success = false, Message = "Usuario no encontrado" });
                }

                return Ok(new { Success = true, Data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario por email: {Email}", email);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("by-role/{role}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            try
            {
                var users = await _userService.GetUsersByRoleAsync(role);
                return Ok(new { Success = true, Data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios por rol: {Role}", role);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Success = false, Message = "Datos inválidos" });
                }

                var createdUser = await _userService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id },
                    new { Success = true, Data = createdUser });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario");
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                // Solo admins pueden actualizar cualquier usuario, otros solo pueden actualizarse a sí mismos
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                var currentUserRole = User.FindFirst("Role")?.Value;

                if (currentUserRole != "Admin" &&
                    (!int.TryParse(currentUserIdClaim, out int currentUserId) || currentUserId != id))
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Success = false, Message = "Datos inválidos" });
                }

                await _userService.UpdateUserAsync(id, updateUserDto);
                return Ok(new { Success = true, Message = "Usuario actualizado exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando usuario: {UserId}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                // Solo el propio usuario puede actualizar su perfil
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(currentUserIdClaim, out int currentUserId) || currentUserId != id)
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Success = false, Message = "Datos inválidos" });
                }

                await _userService.UpdateProfileAsync(id, updateProfileDto);
                return Ok(new { Success = true, Message = "Perfil actualizado exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando perfil del usuario: {UserId}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}/password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordDto updatePasswordDto)
        {
            try
            {
                // Solo el propio usuario puede cambiar su contraseña
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(currentUserIdClaim, out int currentUserId) || currentUserId != id)
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Success = false, Message = "Datos inválidos" });
                }

                var success = await _userService.ChangePasswordAsync(id, updatePasswordDto.CurrentPassword, updatePasswordDto.NewPassword);
                if (!success)
                {
                    return BadRequest(new { Success = false, Message = "Contraseña actual incorrecta" });
                }

                return Ok(new { Success = true, Message = "Contraseña actualizada exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando contraseña del usuario: {UserId}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return Ok(new { Success = true, Message = "Usuario eliminado exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando usuario: {UserId}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}/with-addresses")]
        public async Task<IActionResult> GetUserWithAddresses(int id)
        {
            try
            {
                // Solo admins pueden ver cualquier usuario, otros solo pueden verse a sí mismos
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                var currentUserRole = User.FindFirst("Role")?.Value;

                if (currentUserRole != "Admin" &&
                    (!int.TryParse(currentUserIdClaim, out int currentUserId) || currentUserId != id))
                {
                    return Forbid();
                }

                var user = await _userService.GetUserWithAddressesAsync(id);
                if (user == null)
                {
                    return NotFound(new { Success = false, Message = "Usuario no encontrado" });
                }

                return Ok(new { Success = true, Data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario con direcciones: {UserId}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}/with-business")]
        public async Task<IActionResult> GetUserWithBusiness(int id)
        {
            try
            {
                // Solo admins pueden ver cualquier usuario, otros solo pueden verse a sí mismos
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                var currentUserRole = User.FindFirst("Role")?.Value;

                if (currentUserRole != "Admin" &&
                    (!int.TryParse(currentUserIdClaim, out int currentUserId) || currentUserId != id))
                {
                    return Forbid();
                }

                var user = await _userService.GetUserWithBusinessAsync(id);
                if (user == null)
                {
                    return NotFound(new { Success = false, Message = "Usuario no encontrado" });
                }

                return Ok(new { Success = true, Data = user });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario con negocio: {UserId}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("stats/count-by-role/{role}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserCountByRole(string role)
        {
            try
            {
                var count = await _userService.GetUserCountByRoleAsync(role);
                return Ok(new { Success = true, Data = new { Role = role, Count = count } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo conteo de usuarios por rol: {Role}", role);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetActiveUsers()
        {
            try
            {
                var users = await _userService.GetActiveUsersAsync();
                return Ok(new { Success = true, Data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios activos");
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("check-email/{email}")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            try
            {
                var exists = await _userService.EmailExistsAsync(email);
                return Ok(new { Success = true, Data = new { Email = email, Exists = exists } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando existencia de email: {Email}", email);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }
    }
}
