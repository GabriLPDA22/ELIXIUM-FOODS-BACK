using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Order;

namespace UberEatsBackend.Services
{
  public interface IOrderService
  {
    Task<List<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId);
    Task<List<OrderDto>> GetOrdersByRestaurantIdAsync(int restaurantId);
    Task<List<OrderDto>> GetOrdersByDeliveryPersonIdAsync(int deliveryPersonId);
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto createOrderDto);
    Task<OrderDto?> UpdateOrderStatusAsync(int orderId, OrderStatusDto orderStatusDto, int? deliveryPersonId = null);
    Task<bool> DeleteOrderAsync(int id);
  }
}