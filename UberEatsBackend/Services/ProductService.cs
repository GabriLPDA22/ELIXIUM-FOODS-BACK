using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using UberEatsBackend.DTOs.Product;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace UberEatsBackend.Services
{
  public interface IProductService
  {
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<List<ProductDto>> GetProductsByRestaurantIdAsync(int restaurantId);
    Task<List<ProductDto>> GetProductsByCategoryIdAsync(int categoryId);
    Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto productDto);
    Task<bool> DeleteProductAsync(int id);
  }

  public class ProductService : IProductService
  {
    private readonly IProductRepository _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository,
        IRepository<Category> categoryRepository,
        IRestaurantRepository restaurantRepository,
        IMapper mapper)
    {
      _productRepository = productRepository;
      _categoryRepository = categoryRepository;
      _restaurantRepository = restaurantRepository;
      _mapper = mapper;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
      var products = await _productRepository.GetAllAsync();
      var productDtos = _mapper.Map<List<ProductDto>>(products);

      // Enriquecer con información de restaurante
      foreach (var product in productDtos)
      {
        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
        if (category?.MenuId != null)
        {
          var menu = await _categoryRepository.Entities
              .Where(c => c.Id == product.CategoryId)
              .Select(c => c.Menu)
              .FirstOrDefaultAsync();

          if (menu != null)
          {
            product.RestaurantId = menu.RestaurantId;
            
            var restaurant = await _restaurantRepository.GetByIdAsync(menu.RestaurantId);
            if (restaurant != null)
            {
              product.RestaurantName = restaurant.Name;
              product.RestaurantLogo = restaurant.LogoUrl;
            }
          }
        }
      }

      return productDtos;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
      var product = await _productRepository.GetProductWithDetailsAsync(id);
      if (product == null)
        return null;

      var productDto = _mapper.Map<ProductDto>(product);
      
      // Enriquecer con información adicional
      if (product.Category?.Menu?.Restaurant != null)
      {
        var restaurant = product.Category.Menu.Restaurant;
        productDto.RestaurantId = restaurant.Id;
        productDto.RestaurantName = restaurant.Name;
        productDto.RestaurantLogo = restaurant.LogoUrl;
      }

      return productDto;
    }

    public async Task<List<ProductDto>> GetProductsByRestaurantIdAsync(int restaurantId)
    {
      var products = await _productRepository.GetProductsByRestaurantIdAsync(restaurantId);
      var productDtos = _mapper.Map<List<ProductDto>>(products);

      // Obtener información del restaurante
      var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
      
      // Enriquecer DTOs con información del restaurante
      if (restaurant != null)
      {
        foreach (var product in productDtos)
        {
          product.RestaurantId = restaurant.Id;
          product.RestaurantName = restaurant.Name;
          product.RestaurantLogo = restaurant.LogoUrl;
        }
      }

      return productDtos;
    }

    public async Task<List<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
    {
      var products = await _productRepository.GetProductsByCategoryIdAsync(categoryId);
      var productDtos = _mapper.Map<List<ProductDto>>(products);

      // Obtener categoría y su menú
      var category = await _categoryRepository.Entities
          .Include(c => c.Menu)
              .ThenInclude(m => m.Restaurant)
          .FirstOrDefaultAsync(c => c.Id == categoryId);

      // Enriquecer DTOs
      if (category?.Menu?.Restaurant != null)
      {
        var restaurant = category.Menu.Restaurant;
        foreach (var product in productDtos)
        {
          product.RestaurantId = restaurant.Id;
          product.RestaurantName = restaurant.Name;
          product.RestaurantLogo = restaurant.LogoUrl;
          product.CategoryName = category.Name;
        }
      }

      return productDtos;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
    {
      var product = _mapper.Map<Product>(productDto);
      var createdProduct = await _productRepository.AddAsync(product);
      
      return await GetProductByIdAsync(createdProduct.Id);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto productDto)
    {
      var product = await _productRepository.GetByIdAsync(id);
      if (product == null)
        return null;

      _mapper.Map(productDto, product);
      await _productRepository.UpdateAsync(product);
      
      return await GetProductByIdAsync(id);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
      var product = await _productRepository.GetByIdAsync(id);
      if (product == null)
        return false;

      await _productRepository.DeleteAsync(product);
      return true;
    }
  }
}