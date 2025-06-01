using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        /// <summary>
        /// Obtiene todos los pedidos de un usuario específico
        /// </summary>
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);

        /// <summary>
        /// Obtiene todos los pedidos de un restaurante específico
        /// </summary>
        Task<List<Order>> GetOrdersByRestaurantIdAsync(int restaurantId);

        /// <summary>
        /// Obtiene todos los pedidos asignados a un repartidor específico
        /// </summary>
        Task<List<Order>> GetOrdersByDeliveryPersonIdAsync(int deliveryPersonId);

        /// <summary>
        /// Obtiene un pedido con todos sus detalles relacionados
        /// </summary>
        Task<Order?> GetOrderWithDetailsAsync(int orderId);

        /// <summary>
        /// Crea un pedido junto con sus items en una transacción
        /// </summary>
        Task<Order> CreateOrderAsync(Order order, List<OrderItem> orderItems);

        /// <summary>
        /// Actualiza el estado de un pedido
        /// </summary>
        Task UpdateOrderStatusAsync(int orderId, string status, int? deliveryPersonId = null);

        /// <summary>
        /// Obtiene pedidos por estado
        /// </summary>
        Task<List<Order>> GetOrdersByStatusAsync(string status);

        /// <summary>
        /// Obtiene los pedidos más recientes
        /// </summary>
        Task<List<Order>> GetRecentOrdersAsync(int limit = 10);

        /// <summary>
        /// Verifica si un usuario puede acceder a un pedido específico
        /// </summary>
        Task<bool> CanUserAccessOrderAsync(int orderId, int userId, string userRole);
    }
}
