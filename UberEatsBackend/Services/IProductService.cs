using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Product;

namespace UberEatsBackend.Services
{
  public interface IProductService
  {
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<List<ProductDto>> GetProductsByBusinessIdAsync(int businessId);
    Task<List<ProductDto>> GetProductsByCategoryIdAsync(int categoryId);
    Task<List<ProductDto>> GetProductsByRestaurantIdAsync(int restaurantId);
    Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto productDto);
    Task<bool> DeleteProductAsync(int id);
    Task<bool> UpdateProductImageAsync(int productId, string? imageUrl);
    Task<List<ProductDto>> SearchProductsAsync(string query, int? categoryId);
  }
}
