using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using UberEatsBackend.DTOs.RestaurantProduct;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;

namespace UberEatsBackend.Services
{
  public class RestaurantProductService : IRestaurantProductService
  {
    private readonly IRestaurantProductRepository _restaurantProductRepository;
    private readonly IProductRepository _productRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public RestaurantProductService(
        IRestaurantProductRepository restaurantProductRepository,
        IProductRepository productRepository,
        IRestaurantRepository restaurantRepository,
        IMapper mapper)
    {
      _restaurantProductRepository = restaurantProductRepository;
      _productRepository = productRepository;
      _restaurantRepository = restaurantRepository;
      _mapper = mapper;
    }

    public async Task<List<RestaurantProductDto>> GetRestaurantProductsAsync(int restaurantId)
    {
      var restaurantProducts = await _restaurantProductRepository.GetByRestaurantIdAsync(restaurantId);
      return _mapper.Map<List<RestaurantProductDto>>(restaurantProducts);
    }

    public async Task<RestaurantProductDto?> GetRestaurantProductAsync(int restaurantId, int productId)
    {
      var restaurantProduct = await _restaurantProductRepository.GetByRestaurantAndProductAsync(restaurantId, productId);
      return restaurantProduct != null ? _mapper.Map<RestaurantProductDto>(restaurantProduct) : null;
    }

    public async Task<RestaurantProductDto> AssignProductToRestaurantAsync(int restaurantId, CreateRestaurantProductDto createDto)
    {
      // Verificar que el producto y restaurante existan
      var product = await _productRepository.GetByIdAsync(createDto.ProductId);
      if (product == null)
        throw new KeyNotFoundException($"Product with ID {createDto.ProductId} not found");

      var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
      if (restaurant == null)
        throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

      // Verificar que no exista ya la asignación
      if (await _restaurantProductRepository.ExistsAsync(restaurantId, createDto.ProductId))
        throw new InvalidOperationException("Product is already assigned to this restaurant");

      var restaurantProduct = new RestaurantProduct
      {
        RestaurantId = restaurantId,
        ProductId = createDto.ProductId,
        Price = createDto.Price,
        IsAvailable = createDto.IsAvailable,
        StockQuantity = createDto.StockQuantity,
        Notes = createDto.Notes,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      var createdRestaurantProduct = await _restaurantProductRepository.AddAsync(restaurantProduct);
      return _mapper.Map<RestaurantProductDto>(createdRestaurantProduct);
    }

    public async Task<RestaurantProductDto?> UpdateRestaurantProductAsync(int restaurantId, int productId, UpdateRestaurantProductDto updateDto)
    {
      // Buscar la relación existente
      var restaurantProduct = await _restaurantProductRepository.GetByRestaurantAndProductAsync(restaurantId, productId);

      if (restaurantProduct == null)
      {
        // Si no existe, verificar que el producto y restaurante existan antes de crear
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
          throw new KeyNotFoundException($"Product with ID {productId} not found");

        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
        if (restaurant == null)
          throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

        // Crear nueva relación
        restaurantProduct = new RestaurantProduct
        {
          RestaurantId = restaurantId,
          ProductId = productId,
          Price = updateDto.Price,
          IsAvailable = updateDto.IsAvailable,
          StockQuantity = updateDto.StockQuantity,
          Notes = updateDto.Notes,
          CreatedAt = DateTime.UtcNow,
          UpdatedAt = DateTime.UtcNow
        };

        var createdRestaurantProduct = await _restaurantProductRepository.AddAsync(restaurantProduct);
        return _mapper.Map<RestaurantProductDto>(createdRestaurantProduct);
      }
      else
      {
        // Actualizar relación existente
        restaurantProduct.Price = updateDto.Price;
        restaurantProduct.IsAvailable = updateDto.IsAvailable;
        restaurantProduct.StockQuantity = updateDto.StockQuantity;
        restaurantProduct.Notes = updateDto.Notes;
        restaurantProduct.UpdatedAt = DateTime.UtcNow;

        await _restaurantProductRepository.UpdateAsync(restaurantProduct);
        return _mapper.Map<RestaurantProductDto>(restaurantProduct);
      }
    }

    public async Task<bool> RemoveProductFromRestaurantAsync(int restaurantId, int productId)
    {
      var restaurantProduct = await _restaurantProductRepository.GetByRestaurantAndProductAsync(restaurantId, productId);
      if (restaurantProduct == null)
        return false;

      await _restaurantProductRepository.DeleteAsync(restaurantProduct);
      return true;
    }

    public async Task<List<RestaurantProductDto>> BulkAssignProductsAsync(int restaurantId, List<CreateRestaurantProductDto> products)
    {
      var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
      if (restaurant == null)
        throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

      var restaurantProducts = new List<RestaurantProduct>();

      foreach (var productDto in products)
      {
        var product = await _productRepository.GetByIdAsync(productDto.ProductId);
        if (product == null)
          throw new KeyNotFoundException($"Product with ID {productDto.ProductId} not found");

        if (await _restaurantProductRepository.ExistsAsync(restaurantId, productDto.ProductId))
          continue; // Skip if already assigned

        restaurantProducts.Add(new RestaurantProduct
        {
          RestaurantId = restaurantId,
          ProductId = productDto.ProductId,
          Price = productDto.Price,
          IsAvailable = productDto.IsAvailable,
          StockQuantity = productDto.StockQuantity,
          Notes = productDto.Notes,
          CreatedAt = DateTime.UtcNow,
          UpdatedAt = DateTime.UtcNow
        });
      }

      if (restaurantProducts.Any())
      {
        await _restaurantProductRepository.BulkInsertAsync(restaurantProducts);
      }

      return _mapper.Map<List<RestaurantProductDto>>(restaurantProducts);
    }

    public async Task<List<RestaurantProductDto>> GetProductsByBusinessAsync(int businessId)
    {
      var restaurantProducts = await _restaurantProductRepository.GetByBusinessIdAsync(businessId);
      return _mapper.Map<List<RestaurantProductDto>>(restaurantProducts);
    }
  }
}
