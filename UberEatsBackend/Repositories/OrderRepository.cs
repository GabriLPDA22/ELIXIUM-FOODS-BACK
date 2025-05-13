using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.Models;
using UberEatsBackend.DTOs.Order;

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
        
        if (deliveryPersonId.HasValue)
        {
          order.DeliveryPersonId = deliveryPersonId;
        }

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
      }
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