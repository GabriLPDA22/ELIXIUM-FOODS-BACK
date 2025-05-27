using System;
using System.Collections.Generic;

namespace UberEatsBackend.DTOs.Business
{
    public class BusinessStatsDto
    {
        // Información básica
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;

        // Resumen general
        public int TotalRestaurants { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }

        // Cambios porcentuales respecto al periodo anterior
        public decimal RevenueChange { get; set; }
        public decimal OrdersChange { get; set; }
        public decimal AverageOrderValueChange { get; set; } // AOV

        // Valores promedio
        public decimal AverageOrderValue { get; set; } // AOV

        // Datos de tendencia para gráficos
        public List<DailyRevenueDto> RevenueTrend { get; set; } = new List<DailyRevenueDto>();
        public List<DailyRevenueDto> PreviousPeriodRevenueTrend { get; set; } = new List<DailyRevenueDto>();
        public List<OrdersByDayDto> OrdersByDay { get; set; } = new List<OrdersByDayDto>();

        // Datos de clientes
        public CustomerStatsDto CustomerStats { get; set; } = new CustomerStatsDto();

        // Productos más vendidos
        public List<TopProductDto> TopProducts { get; set; } = new List<TopProductDto>();

        // Ventas por categoría
        public List<CategorySalesDto> CategorySales { get; set; } = new List<CategorySalesDto>();

        // Estadísticas específicas por restaurante
        public List<RestaurantStatsDto> RestaurantStats { get; set; } = new List<RestaurantStatsDto>();
    }

    public class RestaurantStatsDto
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public double AverageRating { get; set; }
        public int ProductCount { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    public class OrdersByDayDto
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public int OrderCount { get; set; }
    }

    public class CustomerStatsDto
    {
        public int NewCustomers { get; set; }
        public int NewCustomersChange { get; set; }
        public int ReturningCustomers { get; set; }
        public int ReturningCustomersChange { get; set; }
        public int TotalCustomers { get; set; }
        public int CustomerSatisfaction { get; set; }
        public int AverageDeliveryTime { get; set; }
        public int DeliveryTimeChange { get; set; }
    }

    public class TopProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CategorySalesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public decimal Percentage { get; set; }
    }
}
