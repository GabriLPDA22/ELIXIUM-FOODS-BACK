using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Order;

namespace UberEatsBackend.Services
{
    public interface IOrderService
    {
        /// <summary>
        /// Obtiene todos los pedidos (solo para administradores)
        /// </summary>
        Task<List<OrderDto>> GetAllOrdersAsync();

        /// <summary>
        /// Obtiene un pedido por su ID
        /// </summary>
        Task<OrderDto?> GetOrderByIdAsync(int id);

        /// <summary>
        /// Obtiene todos los pedidos de un usuario específico
        /// </summary>
        Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId);

        /// <summary>
        /// Obtiene todos los pedidos de un restaurante específico
        /// </summary>
        Task<List<OrderDto>> GetOrdersByRestaurantIdAsync(int restaurantId);

        /// <summary>
        /// Obtiene todos los pedidos asignados a un repartidor específico
        /// </summary>
        Task<List<OrderDto>> GetOrdersByDeliveryPersonIdAsync(int deliveryPersonId);

        /// <summary>
        /// Crea un nuevo pedido
        /// </summary>
        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto createOrderDto);

        /// <summary>
        /// Actualiza el estado de un pedido
        /// </summary>
        Task<OrderDto?> UpdateOrderStatusAsync(int orderId, OrderStatusDto orderStatusDto, int? deliveryPersonId = null);

        /// <summary>
        /// Elimina un pedido (solo para administradores)
        /// </summary>
        Task<bool> DeleteOrderAsync(int id);
    }
}
