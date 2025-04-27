using UberEatsBackend.DTOs.Category;
using System.Collections.Generic;

namespace UberEatsBackend.DTOs.Menu
{
    public class MenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public List<CategoryDto> Categories { get; set; }
    }
}