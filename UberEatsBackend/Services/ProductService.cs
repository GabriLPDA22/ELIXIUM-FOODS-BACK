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
  public class ProductService : IProductService
  {
    private readonly IProductRepository _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRestaurantProductRepository _restaurantProductRepository;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository,
        IRepository<Category> categoryRepository,
        IRestaurantProductRepository restaurantProductRepository,
        IMapper mapper)
    {
      _productRepository = productRepository;
      _categoryRepository = categoryRepository;
      _restaurantProductRepository = restaurantProductRepository;
      _mapper = mapper;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
      var products = await _productRepository.GetAllAsync();
      return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
      var product = await _productRepository.GetProductWithDetailsAsync(id);
      return product != null ? _mapper.Map<ProductDto>(product) : null;
    }

    public async Task<List<ProductDto>> GetProductsByBusinessIdAsync(int businessId)
    {
      var products = await _productRepository.GetProductsByBusinessIdAsync(businessId);
      return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<List<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
    {
      var products = await _productRepository.GetProductsByCategoryIdAsync(categoryId);
      return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<List<ProductDto>> GetProductsByRestaurantIdAsync(int restaurantId)
    {
      var restaurantProducts = await _restaurantProductRepository.GetByRestaurantIdAsync(restaurantId);
      var productDtos = new List<ProductDto>();

      foreach (var rp in restaurantProducts)
      {
        var productDto = _mapper.Map<ProductDto>(rp.Product);
        // Agregar información específica del restaurante
        productDto.RestaurantPrice = rp.Price;
        productDto.RestaurantAvailability = rp.IsAvailable;
        productDto.StockQuantity = rp.StockQuantity;
        productDtos.Add(productDto);
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
