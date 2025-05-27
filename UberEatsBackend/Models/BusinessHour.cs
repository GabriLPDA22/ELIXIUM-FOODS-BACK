using System;

namespace UberEatsBackend.Models
{
    public class BusinessHour
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = string.Empty; // monday, tuesday, etc.
        public bool IsOpen { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }

        // Relación con Business
        public int BusinessId { get; set; }
        public Business Business { get; set; } = null!;
    }
}
