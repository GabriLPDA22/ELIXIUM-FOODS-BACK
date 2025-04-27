using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.DTOs.Dashboard;
using UberEatsBackend.Models;

namespace UberEatsBackend.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(DashboardFilterDto filter)
        {
            var query = _context.Orders.AsQueryable();

            // Aplicar filtros si se especifican
            if (filter.StartDate.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.EndDate.Value);

            if (filter.RestaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == filter.RestaurantId.Value);

            if (!string.IsNullOrEmpty(filter.OrderStatus))
                query = query.Where(o => o.Status == filter.OrderStatus);

            // Calcular estadísticas generales
            var totalOrders = await query.CountAsync();
            var completedOrders = await query.Where(o => o.Status == \"Delivered\").CountAsync();
            var cancelledOrders = await query.Where(o => o.Status == \"Cancelled\").CountAsync();
            var totalRevenue = await query.Where(o => o.Status != \"Cancelled\").SumAsync(o => o.Total);
            
            var totalUsers = await _context.Users.CountAsync();
            var totalRestaurants = await _context.Restaurants.CountAsync();

            // Estadísticas de hoy
            var today = DateTime.Today;
            var newOrdersToday = await query.Where(o => o.CreatedAt.Date == today).CountAsync();
            var revenueToday = await query.Where(o => o.CreatedAt.Date == today && o.Status != \"Cancelled\").SumAsync(o => o.Total);

            // Obtener top 5 restaurantes
            var topRestaurants = await GetTopRestaurantsAsync(filter, 5);

            // Obtener top 5 productos
            var topProducts = await GetTopProductsAsync(filter, 5);

            // Obtener ingresos por fecha
            var revenueByDate = await GetRevenueByDateAsync(filter);

            // Obtener pedidos por estado
            var ordersByStatus = await GetOrdersByStatusAsync(filter);

            return new DashboardStatsDto
            {
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                TotalRevenue = totalRevenue,
                TotalUsers = totalUsers,
                TotalRestaurants = totalRestaurants,
                NewOrdersToday = newOrdersToday,
                RevenueToday = revenueToday,
                TopRestaurants = topRestaurants,
                TopProducts = topProducts,
                RevenueByDate = revenueByDate,
                OrdersByStatus = ordersByStatus
            };
        }

        public async Task<List<TopRestaurantDto>> GetTopRestaurantsAsync(DashboardFilterDto filter, int count = 10)
        {
            var query = _context.Orders.AsQueryable();

            // Aplicar filtros
            if (filter.StartDate.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.OrderStatus))
                query = query.Where(o => o.Status == filter.OrderStatus);

            var topRestaurants = await query
                .Where(o => o.Status != \"Cancelled\")
                .GroupBy(o => new { o.RestaurantId, o.Restaurant.Name, o.Restaurant.LogoUrl })
                .Select(g => new TopRestaurantDto
                {
                    RestaurantId = g.Key.RestaurantId,
                    RestaurantName = g.Key.Name,
                    LogoUrl = g.Key.LogoUrl,
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.Total)
                })
                .OrderByDescending(r => r.TotalRevenue)
                .Take(count)
                .ToListAsync();

            return topRestaurants;
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(DashboardFilterDto filter, int count = 10)
        {
            var query = _context.OrderItems.AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(oi => oi.Order.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(oi => oi.Order.CreatedAt <= filter.EndDate.Value);

            if (filter.RestaurantId.HasValue)
                query = query.Where(oi => oi.Order.RestaurantId == filter.RestaurantId.Value);

            if (!string.IsNullOrEmpty(filter.OrderStatus))
                query = query.Where(oi => oi.Order.Status == filter.OrderStatus);

            var topProducts = await query
                .Where(oi => oi.Order.Status != \"Cancelled\")
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.Restaurant.Name })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    RestaurantName = g.Key.Name,
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(p => p.OrderCount)
                .Take(count)
                .ToListAsync();

            return topProducts;
        }

        public async Task<List<RevenueByDateDto>> GetRevenueByDateAsync(DashboardFilterDto filter)
        {
            var query = _context.Orders.AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.EndDate.Value);

            if (filter.RestaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == filter.RestaurantId.Value);

            string dateFormat;
            switch (filter.TimeInterval?.ToLower())
            {
                case \"weekly\":
                    dateFormat = \"yyyy-'W'ww\"; // Formato para semana
                    break;
                case \"monthly\":
                    dateFormat = \"yyyy-MM\"; // Formato para mes
                    break;
                default:
                    dateFormat = \"yyyy-MM-dd\"; // Formato para día
                    break;
            }

            var revenueByDate = await query
                .Where(o => o.Status != \"Cancelled\")
                .GroupBy(o => o.CreatedAt.ToString(dateFormat))
                .Select(g => new RevenueByDateDto
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(o => o.Total),
                    OrderCount = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            return revenueByDate;
        }

        public async Task<List<OrdersByStatusDto>> GetOrdersByStatusAsync(DashboardFilterDto filter)
        {
            var query = _context.Orders.AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.EndDate.Value);

            if (filter.RestaurantId.HasValue)
                query = query.Where(o => o.RestaurantId == filter.RestaurantId.Value);

            var ordersByStatus = await query
                .GroupBy(o => o.Status)
                .Select(g => new OrdersByStatusDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return ordersByStatus;
        }

        public async Task<RestaurantStatsDto> GetRestaurantStatsAsync(DashboardFilterDto filter)
        {
            if (!filter.RestaurantId.HasValue)
                throw new ArgumentException(\"Restaurant ID is required\");

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == filter.RestaurantId.Value);

            if (restaurant == null)
                return null;

            var query = _context.Orders.Where(o => o.RestaurantId == filter.RestaurantId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.OrderStatus))
                query = query.Where(o => o.Status == filter.OrderStatus);

            // Estadísticas generales
            var totalOrders = await query.CountAsync();
            var completedOrders = await query.Where(o => o.Status == \"Delivered\").CountAsync();
            var cancelledOrders = await query.Where(o => o.Status == \"Cancelled\").CountAsync();
            var totalRevenue = await query.Where(o => o.Status != \"Cancelled\").SumAsync(o => o.Total);

            // Estadísticas de hoy
            var today = DateTime.Today;
            var newOrdersToday = await query.Where(o => o.CreatedAt.Date == today).CountAsync();
            var revenueToday = await query.Where(o => o.CreatedAt.Date == today && o.Status != \"Cancelled\").SumAsync(o => o.Total);

            // Número total de clientes únicos
            var totalCustomers = await query
                .Select(o => o.UserId)
                .Distinct()
                .CountAsync();

            // Rating promedio
            var averageRating = restaurant.AverageRating;

            // Top 5 productos
            var topProducts = await GetTopProductsAsync(filter, 5);

            // Ingresos por fecha
            var revenueByDate = await GetRevenueByDateAsync(filter);

            // Pedidos por hora
            var ordersByHour = await query
                .GroupBy(o => o.CreatedAt.Hour)
                .Select(g => new OrdersByHourDto
                {
                    Hour = g.Key,
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Hour)
                .ToListAsync();

            // Distribución de pedidos (por tipo de producto o categoría)
            var orderDistribution = await _context.OrderItems
                .Where(oi => oi.Order.RestaurantId == filter.RestaurantId.Value)
                .Where(oi => oi.Order.Status != \"Cancelled\")
                .GroupBy(oi => oi.Product.Category.Name)
                .Select(g => new OrderDistributionDto
                {
                    Category = g.Key,
                    Amount = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                    Percentage = 0 // Se calculará a continuación
                })
                .ToListAsync();

            // Calcular porcentajes para la distribución
            if (orderDistribution.Any())
            {
                var total = orderDistribution.Sum(od => od.Amount);
                foreach (var od in orderDistribution)
                {
                    od.Percentage = Math.Round((od.Amount / total) * 100, 2);
                }
            }

            return new RestaurantStatsDto
            {
                RestaurantId = restaurant.Id,
                RestaurantName = restaurant.Name,
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                TotalRevenue = totalRevenue,
                AverageRating = averageRating,
                NewOrdersToday = newOrdersToday,
                RevenueToday = revenueToday,
                TotalCustomers = totalCustomers,
                TopProducts = topProducts,
                RevenueByDate = revenueByDate,
                OrdersByHour = ordersByHour,
                OrderDistribution = orderDistribution
            };
        }
    }
}