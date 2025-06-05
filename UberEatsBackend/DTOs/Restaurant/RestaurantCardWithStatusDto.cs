// UberEatsBackend/DTOs/Restaurant/RestaurantCardWithStatusDto.cs
namespace UberEatsBackend.DTOs.Restaurant
{
    public class RestaurantCardWithStatusDto : RestaurantCardDto
    {
        public bool IsCurrentlyOpen { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
    }

    public class RestaurantDetailWithStatusDto : RestaurantDto
    {
        public bool IsCurrentlyOpen { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public List<RestaurantHourDto> Hours { get; set; } = new List<RestaurantHourDto>();
    }
}
