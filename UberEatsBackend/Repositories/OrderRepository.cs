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

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.DeliveryPerson)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByRestaurantIdAsync(int restaurantId)
        {
            return await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.DeliveryPerson)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByDeliveryPersonIdAsync(int deliveryPersonId)
        {
            return await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.DeliveryPerson)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .Where(o => o.DeliveryPersonId == deliveryPersonId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.DeliveryPerson)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

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

        public async Task UpdateOrderStatusAsync(int orderId, string status, int? deliveryPersonId = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                if (deliveryPersonId.HasValue)
                {
                    order.DeliveryPersonId = deliveryPersonId;
                }

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.DeliveryPerson)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int limit = 10)
        {
            return await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.DeliveryPerson)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

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

        // Sobrescribir GetAllAsync para incluir todas las relaciones
        public override async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.User)
                .Include(o => o.DeliveryPerson)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // Sobrescribir GetByIdAsync para incluir todas las relaciones
        public override async Task<Order?> GetByIdAsync(int id)
        {
            return await GetOrderWithDetailsAsync(id);
        }
    }
}
