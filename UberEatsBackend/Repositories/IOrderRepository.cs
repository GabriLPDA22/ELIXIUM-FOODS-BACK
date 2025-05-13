using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;
using UberEatsBackend.DTOs.Order;

namespace UberEatsBackend.Repositories
{
  public interface IOrderRepository : IRepository<Order>
  {
    Task<List<Order>> GetOrdersByUserIdAsync(int userId);
    Task<List<Order>> GetOrdersByRestaurantIdAsync(int restaurantId);
    Task<List<Order>> GetOrdersByDeliveryPersonIdAsync(int deliveryPersonId);
    Task<Order?> GetOrderWithDetailsAsync(int orderId);
    Task<Order> CreateOrderAsync(Order order, List<OrderItem> orderItems);
    Task UpdateOrderStatusAsync(int orderId, string status, int? deliveryPersonId = null);
  }
}