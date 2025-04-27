using System;

namespace UberEatsBackend.DTOs.Dashboard
{
    public class DashboardFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? RestaurantId { get; set; }
        public string OrderStatus { get; set; }
        public string TimeInterval { get; set; } = "daily"; // daily, weekly, monthly
    }
}