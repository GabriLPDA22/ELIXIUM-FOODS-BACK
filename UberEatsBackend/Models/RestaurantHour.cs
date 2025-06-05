// UberEatsBackend/Models/RestaurantHour.cs
using System;

namespace UberEatsBackend.Models
{
    public class RestaurantHour
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = string.Empty; // monday, tuesday, etc.
        public bool IsOpen { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }

        // Relación con Restaurant
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;
    }
}
