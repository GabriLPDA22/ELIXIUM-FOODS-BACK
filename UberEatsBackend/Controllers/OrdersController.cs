using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UberEatsBackend.DTOs.Order;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los pedidos (solo administradores)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los pedidos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un pedido específico por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound(new { message = "Pedido no encontrado" });
                }

                // Verificar autorización
                if (!CanUserAccessOrder(order))
                {
                    return Forbid();
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedido {OrderId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los pedidos del usuario autenticado
        /// </summary>
        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
        {
            try
            {
                int userId = GetCurrentUserId();
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos del usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene pedidos de un restaurante específico
        /// </summary>
        [HttpGet("restaurant/{restaurantId}")]
        [Authorize(Roles = "Admin,Restaurant,Business")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetRestaurantOrders(int restaurantId)
        {
            try
            {
                // Verificar autorización para ver pedidos del restaurante
                if (!User.IsInRole("Admin") && !CanAccessRestaurantOrders(restaurantId))
                {
                    return Forbid();
                }

                var orders = await _orderService.GetOrdersByRestaurantIdAsync(restaurantId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos del restaurante {RestaurantId}", restaurantId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene pedidos asignados al repartidor autenticado
        /// </summary>
        [HttpGet("my-deliveries")]
        [Authorize(Roles = "DeliveryPerson")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyDeliveries()
        {
            try
            {
                int deliveryPersonId = GetCurrentUserId();
                var orders = await _orderService.GetOrdersByDeliveryPersonIdAsync(deliveryPersonId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entregas del repartidor");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo pedido
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int userId = GetCurrentUserId();
                var createdOrder = await _orderService.CreateOrderAsync(userId, createOrderDto);

                _logger.LogInformation("Pedido {OrderId} creado por usuario {UserId}", createdOrder.Id, userId);

                return CreatedAtAction(
                    nameof(GetOrder),
                    new { id = createdOrder.Id },
                    createdOrder);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Error de datos no encontrados al crear pedido: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Error de validación al crear pedido: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Operación inválida al crear pedido: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Acceso no autorizado al crear pedido: {Message}", ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al crear pedido");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza el estado de un pedido
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] OrderStatusDto orderStatusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar que el pedido existe y el usuario puede modificarlo
                var currentOrder = await _orderService.GetOrderByIdAsync(id);
                if (currentOrder == null)
                {
                    return NotFound(new { message = "Pedido no encontrado" });
                }

                // Verificar permisos para actualizar estado
                if (!CanUpdateOrderStatus(currentOrder, orderStatusDto.Status))
                {
                    return Forbid();
                }

                int? deliveryPersonId = null;

                // Si se actualiza a "OnTheWay", asignar automáticamente al repartidor actual
                if (orderStatusDto.Status == "OnTheWay" && User.IsInRole("DeliveryPerson"))
                {
                    deliveryPersonId = GetCurrentUserId();
                }

                var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, orderStatusDto, deliveryPersonId);

                if (updatedOrder == null)
                {
                    return NotFound(new { message = "Pedido no encontrado" });
                }

                _logger.LogInformation("Estado del pedido {OrderId} actualizado a {Status}", id, orderStatusDto.Status);

                return Ok(updatedOrder);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Error de validación al actualizar estado: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del pedido {OrderId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cancela un pedido (solo clientes en estado Pending)
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<OrderDto>> CancelOrder(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "Pedido no encontrado" });
                }

                // Solo el cliente propietario o admin pueden cancelar
                if (!User.IsInRole("Admin") && order.UserId != GetCurrentUserId())
                {
                    return Forbid();
                }

                // Solo se puede cancelar si está en estado Pending
                if (order.Status != "Pending")
                {
                    return BadRequest(new { message = "Solo se pueden cancelar pedidos en estado 'Pending'" });
                }

                var cancelledOrder = await _orderService.UpdateOrderStatusAsync(id, new OrderStatusDto { Status = "Cancelled" });

                _logger.LogInformation("Pedido {OrderId} cancelado", id);

                return Ok(cancelledOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar pedido {OrderId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina un pedido (solo administradores)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var success = await _orderService.DeleteOrderAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Pedido no encontrado" });
                }

                _logger.LogInformation("Pedido {OrderId} eliminado", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pedido {OrderId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // Métodos auxiliares privados
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Usuario no autenticado correctamente");
            }
            return userId;
        }

        private bool CanUserAccessOrder(OrderDto order)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = GetCurrentUserId();

            // Administradores pueden ver todos
            if (userRole == "Admin") return true;

            // Clientes pueden ver sus propios pedidos
            if (userRole == "Customer" && order.UserId == userId) return true;

            // Repartidores pueden ver pedidos asignados
            if (userRole == "DeliveryPerson" && order.DeliveryPersonId == userId) return true;

            // Restaurantes pueden ver sus pedidos
            if ((userRole == "Restaurant" || userRole == "Business") &&
                User.HasClaim(c => c.Type == "RestaurantId" && c.Value == order.RestaurantId.ToString()))
            {
                return true;
            }

            return false;
        }

        private bool CanAccessRestaurantOrders(int restaurantId)
        {
            return User.HasClaim(c => c.Type == "RestaurantId" && c.Value == restaurantId.ToString());
        }

        private bool CanUpdateOrderStatus(OrderDto order, string newStatus)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = GetCurrentUserId();

            // Administradores pueden hacer cualquier cambio
            if (userRole == "Admin") return true;

            // Restaurantes solo pueden actualizar ciertos estados
            if ((userRole == "Restaurant" || userRole == "Business") &&
                User.HasClaim(c => c.Type == "RestaurantId" && c.Value == order.RestaurantId.ToString()))
            {
                string[] allowedStatuses = { "Accepted", "Preparing", "ReadyForPickup", "Cancelled" };
                return allowedStatuses.Contains(newStatus);
            }

            // Repartidores solo pueden actualizar a OnTheWay o Delivered
            if (userRole == "DeliveryPerson")
            {
                // Si el pedido ya está asignado, solo ese repartidor puede actualizarlo
                if (order.DeliveryPersonId.HasValue && order.DeliveryPersonId.Value != userId)
                {
                    return false;
                }

                string[] allowedStatuses = { "OnTheWay", "Delivered" };
                return allowedStatuses.Contains(newStatus);
            }

            // Clientes solo pueden cancelar sus propios pedidos en estado Pending
            if (userRole == "Customer" && order.UserId == userId &&
                newStatus == "Cancelled" && order.Status == "Pending")
            {
                return true;
            }

            return false;
        }
    }
}
