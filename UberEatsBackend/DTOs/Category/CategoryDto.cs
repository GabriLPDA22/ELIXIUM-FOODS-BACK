using System.Collections.Generic;
using UberEatsBackend.DTOs.Product;

namespace UberEatsBackend.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MenuId { get; set; }
        public List<ProductDto> Products { get; set; }
    }
}