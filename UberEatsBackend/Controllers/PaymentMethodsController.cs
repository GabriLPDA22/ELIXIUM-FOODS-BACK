// UberEatsBackend/Controllers/PaymentMethodsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UberEatsBackend.DTOs.PaymentMethod;
using UberEatsBackend.Services;

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
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetUserPaymentMethods()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var paymentMethods = await _paymentMethodService.GetUserPaymentMethodsAsync(userId);

                return Ok(new { Success = true, Data = paymentMethods });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métodos de pago");
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PaymentMethodDto>> CreatePaymentMethod(CreatePaymentMethodDto createDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var paymentMethod = await _paymentMethodService.CreatePaymentMethodAsync(userId, createDto);

                return CreatedAtAction(
                    nameof(GetPaymentMethodById),
                    new { id = paymentMethod.Id },
                    new { Success = true, Data = paymentMethod });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando método de pago");
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> GetPaymentMethodById(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, userId);

                if (paymentMethod == null)
                    return NotFound(new { Success = false, Message = "Método de pago no encontrado" });

                return Ok(new { Success = true, Data = paymentMethod });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo método de pago {Id}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}/default")]
        public async Task<IActionResult> SetAsDefault(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _paymentMethodService.SetAsDefaultAsync(id, userId);

                if (!success)
                    return NotFound(new { Success = false, Message = "Método de pago no encontrado" });

                return Ok(new { Success = true, Message = "Método de pago establecido como predeterminado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo método predeterminado {Id}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var success = await _paymentMethodService.DeletePaymentMethodAsync(id, userId);

                if (!success)
                    return NotFound(new { Success = false, Message = "Método de pago no encontrado" });

                return Ok(new { Success = true, Message = "Método de pago eliminado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando método de pago {Id}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }
    }
}
