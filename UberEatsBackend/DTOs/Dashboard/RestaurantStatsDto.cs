using System.Collections.Generic;

namespace UberEatsBackend.DTOs.Dashboard
{
    public class RestaurantStatsDto
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
        public int NewOrdersToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int TotalCustomers { get; set; }
        public List<TopProductDto> TopProducts { get; set; }
        public List<RevenueByDateDto> RevenueByDate { get; set; }
        public List<OrdersByHourDto> OrdersByHour { get; set; }
        public List<OrderDistributionDto> OrderDistribution { get; set; }
    }

    public class OrdersByHourDto
    {
        public int Hour { get; set; }
        public int OrderCount { get; set; }
    }

    public class OrderDistributionDto
    {
        public string Category { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
    }
}