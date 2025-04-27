using System;
using System.Collections.Generic;

namespace UberEatsBackend.DTOs.Dashboard
{
    public class UserGrowthDto
    {
        public string Date { get; set; }
        public int NewUsers { get; set; }
        public int TotalUsers { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class UserRetentionDto
    {
        public decimal RetentionRate { get; set; }
        public int TotalUsers { get; set; }
        public int RetainedUsers { get; set; }
        public List<CohortDto> Cohorts { get; set; }
    }

    public class CohortDto
    {
        public string Cohort { get; set; }
        public int InitialUsers { get; set; }
        public List<CohortRetentionDto> RetentionData { get; set; }
    }

    public class CohortRetentionDto
    {
        public int Period { get; set; }
        public int ActiveUsers { get; set; }
        public decimal RetentionRate { get; set; }
    }

    public class OrderHeatmapDto
    {
        public int DayOfWeek { get; set; }
        public int Hour { get; set; }
        public int OrderCount { get; set; }
    }

    public class CustomerInsightDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
        public List<string> FavoriteProducts { get; set; }
        public string OrderFrequency { get; set; }
    }

    public class RestaurantPerformanceDto
    {
        public decimal AverageOrderValue { get; set; }
        public decimal OrderFrequency { get; set; }
        public decimal CustomerRetentionRate { get; set; }
        public int AverageDeliveryTime { get; set; }
        public decimal CancellationRate { get; set; }
        public List<MenuPerformanceDto> MenuPerformance { get; set; }
        public List<PeakTimeDto> PeakTimes { get; set; }
    }

    public class MenuPerformanceDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Contribution { get; set; }
    }

    public class PeakTimeDto
    {
        public int DayOfWeek { get; set; }
        public string DayName { get; set; }
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public int OrderCount { get; set; }
    }

    public class DeliveryMetricsDto
    {
        public decimal AverageDeliveryTime { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
        public List<DeliveryPerformanceDto> DeliveryPerformance { get; set; }
        public List<ZoneDeliveryDto> ZoneDelivery { get; set; }
    }

    public class DeliveryPerformanceDto
    {
        public int DeliveryPersonId { get; set; }
        public string DeliveryPersonName { get; set; }
        public int DeliveryCount { get; set; }
        public decimal AverageDeliveryTime { get; set; }
        public decimal OnTimeRate { get; set; }
        public decimal Rating { get; set; }
    }

    public class ZoneDeliveryDto
    {
        public string Zone { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageDeliveryTime { get; set; }
        public decimal AverageDistance { get; set; }
    }
}