// UberEatsBackend/DTOs/Restaurant/RestaurantHourDto.cs
namespace UberEatsBackend.DTOs.Restaurant
{
    public class RestaurantHourDto
    {
        public int Id { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
    }

    public class CreateRestaurantHourDto
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;
    }

    // CORREGIDO: Ahora coincide con tu frontend
    public class UpdateRestaurantHourDto
    {
        public string DayOfWeek { get; set; } = string.Empty; // 'monday', 'tuesday', etc.
        public bool IsOpen { get; set; }                     // true/false (no IsClosed)
        public string OpenTime { get; set; } = string.Empty; // '10:00' (no TimeSpan)
        public string CloseTime { get; set; } = string.Empty; // '22:00' (no TimeSpan)
    }

    public class BulkUpdateRestaurantHoursDto
    {
        public List<UpdateRestaurantHourDto> Hours { get; set; } = new List<UpdateRestaurantHourDto>();
    }
}
