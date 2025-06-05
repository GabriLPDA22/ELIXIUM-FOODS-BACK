// UberEatsBackend/Controllers/PaymentMethodsController.cs - CON HARD DELETE
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UberEatsBackend.DTOs.PaymentMethod;
using UberEatsBackend.Services;
using System.Text.Json;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ILogger<PaymentMethodsController> _logger;

        public PaymentMethodsController(
            IPaymentMethodService paymentMethodService,
            ILogger<PaymentMethodsController> logger)
        {
            _paymentMethodService = paymentMethodService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPaymentMethods()
        {
            try
            {
                _logger.LogInformation("=== INICIO GET PAYMENT METHODS ===");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Usuario no autenticado en GET");
                    return Unauthorized(new { Success = false, Message = "Usuario no autenticado" });
                }

                _logger.LogInformation("UserId para GET: {UserId}", userId);

                var paymentMethods = await _paymentMethodService.GetUserPaymentMethodsAsync(userId);

                _logger.LogInformation("Métodos encontrados: {Count}", paymentMethods?.Count() ?? 0);

                if (paymentMethods != null)
                {
                    foreach (var method in paymentMethods)
                    {
                        _logger.LogInformation("Método: Id={Id}, Nickname={Nickname}, Type={Type}, IsDefault={IsDefault}",
                            method.Id, method.Nickname, method.Type, method.IsDefault);
                    }
                }

                var response = new { Success = true, Data = paymentMethods };
                _logger.LogInformation("Respuesta GET: {@Response}", response);
                _logger.LogInformation("=== FIN GET PAYMENT METHODS ===");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métodos de pago");
                return StatusCode(500, new { Success = false, Message = "Error del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodDto createDto)
        {
            try
            {
                _logger.LogInformation("=== INICIO CREATE PAYMENT METHOD ===");
                _logger.LogInformation("DTO recibido: {@CreateDto}", createDto);

                if (createDto == null)
                {
                    _logger.LogWarning("DTO es null");
                    return BadRequest(new { Success = false, Message = "Datos no válidos - payload vacío" });
                }

                if (string.IsNullOrWhiteSpace(createDto.Nickname))
                {
                    _logger.LogWarning("Nickname está vacío");
                    return BadRequest(new { Success = false, Message = "Nombre del método es requerido" });
                }

                if (string.IsNullOrWhiteSpace(createDto.Type))
                {
                    _logger.LogWarning("Type está vacío");
                    return BadRequest(new { Success = false, Message = "Tipo de método es requerido" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Usuario no autenticado. Claim: {Claim}", userIdClaim);
                    return Unauthorized(new { Success = false, Message = "Usuario no autenticado" });
                }

                _logger.LogInformation("Creando método para UserId: {UserId}", userId);

                var paymentMethod = await _paymentMethodService.CreatePaymentMethodAsync(userId, createDto);

                if (paymentMethod == null)
                {
                    _logger.LogError("El servicio retornó null");
                    return StatusCode(500, new { Success = false, Message = "Error interno: el servicio no pudo crear el método de pago" });
                }

                var response = new
                {
                    Success = true,
                    Data = paymentMethod,
                    Message = "Método de pago creado exitosamente"
                };

                _logger.LogInformation("✅ Método creado: {@PaymentMethod}", paymentMethod);
                _logger.LogInformation("📤 Respuesta CREATE: {@Response}", response);
                _logger.LogInformation("=== FIN CREATE PAYMENT METHOD ===");

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Error de validación en servicio: {Message}", ex.Message);
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado creando método de pago");
                return StatusCode(500, new { Success = false, Message = $"Error del servidor: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentMethodById(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { Success = false, Message = "Usuario no autenticado" });
                }

                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, userId);

                if (paymentMethod == null)
                {
                    return NotFound(new { Success = false, Message = "Método de pago no encontrado" });
                }

                return Ok(new { Success = true, Data = paymentMethod });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo método de pago {Id}", id);
                return StatusCode(500, new { Success = false, Message = "Error del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] UpdatePaymentMethodDto updateDto)
        {
            try
            {
                if (updateDto == null || string.IsNullOrWhiteSpace(updateDto.Nickname))
                {
                    return BadRequest(new { Success = false, Message = "Datos no válidos" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { Success = false, Message = "Usuario no autenticado" });
                }

                var paymentMethod = await _paymentMethodService.UpdatePaymentMethodAsync(id, userId, updateDto);

                return Ok(new
                {
                    Success = true,
                    Data = paymentMethod,
                    Message = "Método actualizado exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando método de pago");
                return StatusCode(500, new { Success = false, Message = "Error del servidor" });
            }
        }

        [HttpPut("{id}/default")]
        public async Task<IActionResult> SetAsDefault(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { Success = false, Message = "Usuario no autenticado" });
                }

                var success = await _paymentMethodService.SetAsDefaultAsync(id, userId);

                if (!success)
                {
                    return NotFound(new { Success = false, Message = "Método de pago no encontrado" });
                }

                return Ok(new { Success = true, Message = "Método establecido como predeterminado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo método predeterminado {Id}", id);
                return StatusCode(500, new { Success = false, Message = "Error del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            try
            {
                _logger.LogInformation("=== INICIO DELETE PAYMENT METHOD ===");
                _logger.LogInformation("Eliminando método con ID: {Id}", id);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Usuario no autenticado en DELETE");
                    return Unauthorized(new { Success = false, Message = "Usuario no autenticado" });
                }

                _logger.LogInformation("Usuario autenticado: {UserId}", userId);

                // 🔥 HARD DELETE - Eliminación física de la base de datos
                var success = await _paymentMethodService.DeletePaymentMethodAsync(id, userId);

                if (!success)
                {
                    _logger.LogWarning("Método de pago no encontrado: {Id}", id);
                    return NotFound(new { Success = false, Message = "Método de pago no encontrado" });
                }

                _logger.LogInformation("✅ Método de pago eliminado exitosamente: {Id}", id);
                _logger.LogInformation("=== FIN DELETE PAYMENT METHOD ===");

                return Ok(new { Success = true, Message = "Método eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error eliminando método de pago {Id}", id);
                return StatusCode(500, new { Success = false, Message = "Error del servidor" });
            }
        }
    }
}
