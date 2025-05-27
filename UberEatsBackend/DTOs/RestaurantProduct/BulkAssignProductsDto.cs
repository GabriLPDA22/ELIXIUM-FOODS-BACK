using System.Collections.Generic;

namespace UberEatsBackend.DTOs.RestaurantProduct
{
    public class BulkAssignProductsDto
    {
        public List<CreateRestaurantProductDto> Products { get; set; } = new List<CreateRestaurantProductDto>();
    }
}
