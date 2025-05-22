using System.Collections.Generic;
using UberEatsBackend.DTOs.Product;

namespace UberEatsBackend.DTOs.Category
{
  public class CategoryDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
  }
}
