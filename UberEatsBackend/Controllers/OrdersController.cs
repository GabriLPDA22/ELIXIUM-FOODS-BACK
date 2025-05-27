using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UberEatsBackend.DTOs.Order;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class OrdersController : ControllerBase
  {
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
      _orderService = orderService;
    }

    // GET: api/Orders
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
    {
      var orders = await _orderService.GetAllOrdersAsync();
      return Ok(orders);
    }

    // GET: api/Orders/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
      var order = await _orderService.GetOrderByIdAsync(id);

      if (order == null)
      {
        return NotFound();
      }

      // Verificar si el usuario actual tiene permiso para ver este pedido
      if (!User.IsInRole("Admin") && 
          User.FindFirstValue(ClaimTypes.NameIdentifier) != order.UserId.ToString() &&
          User.FindFirstValue(ClaimTypes.NameIdentifier) != order.DeliveryPersonId?.ToString() &&
          !User.HasClaim(c => c.Type == "RestaurantId" && c.Value == order.RestaurantId.ToString()))
      {
        return Forbid();
      }

      return Ok(order);
    }

    // GET: api/Orders/User
    [HttpGet("User")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
    {
      int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      var orders = await _orderService.GetOrdersByUserIdAsync(userId);
      return Ok(orders);
    }

    // GET: api/Orders/Restaurant/{restaurantId}
    [HttpGet("Restaurant/{restaurantId}")]
    [Authorize(Roles = "Admin,Restaurant")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetRestaurantOrders(int restaurantId)
    {
      // Verificar que el usuario tiene permiso para ver pedidos de este restaurante
      if (!User.IsInRole("Admin") &&
          !User.HasClaim(c => c.Type == "RestaurantId" && c.Value == restaurantId.ToString()))
      {
        return Forbid();
      }

      var orders = await _orderService.GetOrdersByRestaurantIdAsync(restaurantId);
      return Ok(orders);
    }

    // GET: api/Orders/DeliveryPerson
    [HttpGet("DeliveryPerson")]
    [Authorize(Roles = "DeliveryPerson")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetDeliveryPersonOrders()
    {
      int deliveryPersonId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      var orders = await _orderService.GetOrdersByDeliveryPersonIdAsync(deliveryPersonId);
      return Ok(orders);
    }

    // POST: api/Orders
    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
    {
      try
      {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var createdOrder = await _orderService.CreateOrderAsync(userId, createOrderDto);
        return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    // PUT: api/Orders/5/status
    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatusDto orderStatusDto)
    {
      // Verificar permisos según el estado que se quiere establecer
      var currentOrder = await _orderService.GetOrderByIdAsync(id);
      if (currentOrder == null)
      {
        return NotFound();
      }

      int? deliveryPersonId = null;

      // Validar permisos según el rol y el estado solicitado
      if (!CanUpdateOrderStatus(currentOrder, orderStatusDto.Status))
      {
        return Forbid();
      }

      // Si se actualiza a "OnTheWay", asignar automáticamente al repartidor actual
      if (orderStatusDto.Status == "OnTheWay" && User.IsInRole("DeliveryPerson"))
      {
        deliveryPersonId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      }

      try
      {
        var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, orderStatusDto, deliveryPersonId);
        if (updatedOrder == null)
        {
          return NotFound();
        }
        return Ok(updatedOrder);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = ex.Message });
      }
    }

    // DELETE: api/Orders/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
      var success = await _orderService.DeleteOrderAsync(id);
      if (!success)
      {
        return NotFound();
      }

      return NoContent();
    }

    // Método auxiliar para validar si el usuario puede actualizar el estado
    private bool CanUpdateOrderStatus(OrderDto order, string newStatus)
    {
      string role = User.FindFirst(ClaimTypes.Role)?.Value;
      int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

      // Administradores pueden hacer cualquier cambio
      if (role == "Admin")
      {
        return true;
      }

      // Los restaurantes solo pueden actualizar a Accepted, Preparing, ReadyForPickup o Cancelled
      if (role == "Restaurant" && User.HasClaim(c => c.Type == "RestaurantId" && c.Value == order.RestaurantId.ToString()))
      {
        string[] allowedStatuses = { "Accepted", "Preparing", "ReadyForPickup", "Cancelled" };
        return allowedStatuses.Contains(newStatus);
      }

      // Los repartidores solo pueden actualizar a OnTheWay o Delivered
      if (role == "DeliveryPerson")
      {
        // Si el pedido ya está asignado, solo ese repartidor puede actualizarlo
        if (order.DeliveryPersonId.HasValue && order.DeliveryPersonId.Value != userId)
        {
          return false;
        }

        string[] allowedStatuses = { "OnTheWay", "Delivered" };
        return allowedStatuses.Contains(newStatus);
      }

      // Los clientes solo pueden cancelar sus propios pedidos y solo si están en estado Pending
      if (role == "Customer" && order.UserId == userId && newStatus == "Cancelled" && order.Status == "Pending")
      {
        return true;
      }

      return false;
    }
  }
}