using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        // ✅ CRÍTICO: Método principal para obtener pedidos de un usuario
        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.User) // ✅ CRÍTICO: Include User para mostrar cliente
                .Include(o => o.Restaurant) // ✅ CRÍTICO: Include Restaurant
                .Include(o => o.DeliveryAddress) // ✅ CRÍTICO: Include DeliveryAddress
                .Include(o => o.DeliveryPerson) // ✅ Include DeliveryPerson
                .Include(o => o.Payment) // ✅ Include Payment
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) // ✅ Include Product en OrderItems
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // ✅ CRÍTICO: Método para obtener pedidos de un restaurante
        public async Task<List<Order>> GetOrdersByRestaurantIdAsync(int restaurantId)
        {
            return await _context.Orders
                .Include(o => o.User) // ✅ CRÍTICO: Include User para mostrar cliente
                .Include(o => o.Restaurant) // ✅ CRÍTICO: Include Restaurant
                .Include(o => o.DeliveryAddress) // ✅ CRÍTICO: Include DeliveryAddress
                .Include(o => o.DeliveryPerson) // ✅ Include DeliveryPerson
                .Include(o => o.Payment) // ✅ Include Payment
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) // ✅ Include Product en OrderItems
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // ✅ CRÍTICO: Método para obtener pedidos de un repartidor
        public async Task<List<Order>> GetOrdersByDeliveryPersonIdAsync(int deliveryPersonId)
        {
            return await _context.Orders
                .Include(o => o.User) // ✅ CRÍTICO: Include User para mostrar cliente
                .Include(o => o.Restaurant) // ✅ CRÍTICO: Include Restaurant
                .Include(o => o.DeliveryAddress) // ✅ CRÍTICO: Include DeliveryAddress
                .Include(o => o.DeliveryPerson) // ✅ Include DeliveryPerson
                .Include(o => o.Payment) // ✅ Include Payment
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) // ✅ Include Product en OrderItems
                .Where(o => o.DeliveryPersonId == deliveryPersonId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // ✅ CRÍTICO: Método para obtener un pedido específico con todos los detalles
        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.User) // ✅ CRÍTICO: Include User para mostrar cliente
                .Include(o => o.Restaurant) // ✅ CRÍTICO: Include Restaurant
                .Include(o => o.DeliveryAddress) // ✅ CRÍTICO: Include DeliveryAddress
                .Include(o => o.DeliveryPerson) // ✅ Include DeliveryPerson
                .Include(o => o.Payment) // ✅ Include Payment
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) // ✅ Include Product en OrderItems
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        // ✅ CRÍTICO: Método para crear un pedido con sus items
        public async Task<Order> CreateOrderAsync(Order order, List<OrderItem> orderItems)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Añadir la orden
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                // Añadir los items de orden
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                    await _context.OrderItems.AddAsync(item);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ✅ Método para actualizar el estado de un pedido
        public async Task UpdateOrderStatusAsync(int orderId, string status, int? deliveryPersonId = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow; // ✅ Usar UTC explícitamente

                if (deliveryPersonId.HasValue)
                {
                    order.DeliveryPersonId = deliveryPersonId;
                }

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Método para obtener pedidos por estado
        public async Task<List<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.User) // ✅ CRÍTICO: Include User para mostrar cliente
                .Include(o => o.Restaurant) // ✅ CRÍTICO: Include Restaurant
                .Include(o => o.DeliveryAddress) // ✅ CRÍTICO: Include DeliveryAddress
                .Include(o => o.DeliveryPerson) // ✅ Include DeliveryPerson
                .Include(o => o.Payment) // ✅ Include Payment
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) // ✅ Include Product en OrderItems
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // ✅ Método para obtener pedidos recientes
        public async Task<List<Order>> GetRecentOrdersAsync(int limit = 10)
        {
            return await _context.Orders
                .Include(o => o.User) // ✅ CRÍTICO: Include User para mostrar cliente
                .Include(o => o.Restaurant) // ✅ CRÍTICO: Include Restaurant
                .Include(o => o.DeliveryAddress) // ✅ CRÍTICO: Include DeliveryAddress
                .Include(o => o.DeliveryPerson) // ✅ Include DeliveryPerson
                .Include(o => o.Payment) // ✅ Include Payment
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) // ✅ Include Product en OrderItems
                .OrderByDescending(o => o.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        // ✅ Método para verificar permisos de acceso a un pedido
        public async Task<bool> CanUserAccessOrderAsync(int orderId, int userId, string userRole)
        {
            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            // Admin puede ver todos
            if (userRole == "Admin") return true;

            // Cliente puede ver sus propios pedidos
            if (userRole == "Customer" && order.UserId == userId) return true;

            // Repartidor puede ver pedidos asignados
            if (userRole == "DeliveryPerson" && order.DeliveryPersonId == userId) return true;

            // Restaurante puede ver sus pedidos - usando claims del JWT
            if (userRole == "Restaurant")
            {
                // Se verifica con claims en el controller, no con tabla RestaurantUsers
                return true; // La validación real se hace en el controller con claims
            }

            return false;
        }

        // ✅ CRÍTICO: Override GetAllAsync para admin panel con TODOS los includes
        public override async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.User) // ✅ CRÍTICO: Include User para admin panel
                .Include(o => o.Restaurant) // ✅ CRÍTICO: Include Restaurant
                .Include(o => o.DeliveryAddress) // ✅ CRÍTICO: Include DeliveryAddress
                .Include(o => o.DeliveryPerson) // ✅ Include DeliveryPerson
                .Include(o => o.Payment) // ✅ Include Payment
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product) // ✅ Include Product en OrderItems
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // ✅ CRÍTICO: Override GetByIdAsync para incluir todas las relaciones
        public override async Task<Order?> GetByIdAsync(int id)
        {
            return await GetOrderWithDetailsAsync(id);
        }

        // ✅ Método adicional para obtener estadísticas de pedidos
        public async Task<Dictionary<string, int>> GetOrderStatisticsAsync()
        {
            var statistics = new Dictionary<string, int>();

            var orders = await _context.Orders.ToListAsync();

            statistics.Add("Total", orders.Count);
            statistics.Add("Pending", orders.Count(o => o.Status == "Pending"));
            statistics.Add("Accepted", orders.Count(o => o.Status == "Accepted"));
            statistics.Add("Preparing", orders.Count(o => o.Status == "Preparing"));
            statistics.Add("ReadyForPickup", orders.Count(o => o.Status == "ReadyForPickup"));
            statistics.Add("OnTheWay", orders.Count(o => o.Status == "OnTheWay"));
            statistics.Add("Delivered", orders.Count(o => o.Status == "Delivered"));
            statistics.Add("Cancelled", orders.Count(o => o.Status == "Cancelled"));

            // Estadísticas adicionales
            var today = DateTime.UtcNow.Date;
            statistics.Add("TodayOrders", orders.Count(o => o.CreatedAt.Date == today));

            var thisWeek = DateTime.UtcNow.AddDays(-7);
            statistics.Add("WeekOrders", orders.Count(o => o.CreatedAt >= thisWeek));

            var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            statistics.Add("MonthOrders", orders.Count(o => o.CreatedAt >= thisMonth));

            return statistics;
        }

        // ✅ Método para obtener ingresos por período
        public async Task<decimal> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt >= startDate &&
                           o.CreatedAt <= endDate &&
                           o.Status == "Delivered")
                .SumAsync(o => o.Total);
        }

        // ✅ Método para obtener pedidos por rango de fechas
        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.User) // ✅ CRÍTICO: Include User
                .Include(o => o.Restaurant) // ✅ CRÍTICO: Include Restaurant
                .Include(o => o.DeliveryAddress) // ✅ CRÍTICO: Include DeliveryAddress
                .Include(o => o.DeliveryPerson) // ✅ Include DeliveryPerson
                .Include(o => o.Payment) // ✅ Include Payment
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // ✅ Método para buscar pedidos por texto
        public async Task<List<Order>> SearchOrdersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            searchTerm = searchTerm.ToLower();

            return await _context.Orders
                .Include(o => o.User) // ✅ CRÍTICO: Include User
                .Include(o => o.Restaurant) // ✅ CRÍTICO: Include Restaurant
                .Include(o => o.DeliveryAddress) // ✅ CRÍTICO: Include DeliveryAddress
                .Include(o => o.DeliveryPerson) // ✅ Include DeliveryPerson
                .Include(o => o.Payment) // ✅ Include Payment
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Id.ToString().Contains(searchTerm) ||
                           (o.User != null && (o.User.FirstName.ToLower().Contains(searchTerm) ||
                                              o.User.LastName.ToLower().Contains(searchTerm) ||
                                              o.User.Email.ToLower().Contains(searchTerm))) ||
                           (o.Restaurant != null && o.Restaurant.Name.ToLower().Contains(searchTerm)) ||
                           o.Status.ToLower().Contains(searchTerm))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}
