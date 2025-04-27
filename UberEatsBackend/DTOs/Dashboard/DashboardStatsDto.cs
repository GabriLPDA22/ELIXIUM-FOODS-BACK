using System.Collections.Generic;

namespace UberEatsBackend.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalUsers { get; set; }
        public int TotalRestaurants { get; set; }
        public int NewOrdersToday { get; set; }
        public decimal RevenueToday { get; set; }
        public List<TopRestaurantDto> TopRestaurants { get; set; }
        public List<TopProductDto> TopProducts { get; set; }
        public List<RevenueByDateDto> RevenueByDate { get; set; }
        public List<OrdersByStatusDto> OrdersByStatus { get; set; }
    }

    public class TopRestaurantDto
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string LogoUrl { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string RestaurantName { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RevenueByDateDto
    {
        public string Date { get; set; }
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }
    }

    public class OrdersByStatusDto
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }
}