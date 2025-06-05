using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using UberEatsBackend.DTOs.RestaurantProduct;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;
using System.Linq;

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
      var product = await _productRepository.GetByIdAsync(createDto.ProductId);
      if (product == null)
        throw new KeyNotFoundException($"Product with ID {createDto.ProductId} not found");

      var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
      if (restaurant == null)
        throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

      if (await _restaurantProductRepository.ExistsAsync(restaurantId, createDto.ProductId))
        throw new InvalidOperationException("Product is already assigned to this restaurant");

      var restaurantProduct = _mapper.Map<RestaurantProduct>(createDto);
      restaurantProduct.RestaurantId = restaurantId;
      restaurantProduct.CreatedAt = DateTime.UtcNow;
      restaurantProduct.UpdatedAt = DateTime.UtcNow;

      var createdRestaurantProduct = await _restaurantProductRepository.CreateAsync(restaurantProduct);
      return _mapper.Map<RestaurantProductDto>(createdRestaurantProduct);
    }

    public async Task<RestaurantProductDto?> UpdateRestaurantProductAsync(int restaurantId, int productId, UpdateRestaurantProductDto updateDto)
    {
      var restaurantProduct = await _restaurantProductRepository.GetByRestaurantAndProductAsync(restaurantId, productId);

      if (restaurantProduct == null)
      {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) throw new KeyNotFoundException($"Product with ID {productId} not found");
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
        if (restaurant == null) throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

        var newRp = _mapper.Map<RestaurantProduct>(updateDto);
        newRp.RestaurantId = restaurantId;
        newRp.ProductId = productId;
        newRp.CreatedAt = DateTime.UtcNow;
        newRp.UpdatedAt = DateTime.UtcNow;
        var createdRp = await _restaurantProductRepository.CreateAsync(newRp);
        return _mapper.Map<RestaurantProductDto>(createdRp);
      }
      else
      {
        _mapper.Map(updateDto, restaurantProduct);
        restaurantProduct.UpdatedAt = DateTime.UtcNow;
        await _restaurantProductRepository.UpdateAsync(restaurantProduct);
        return _mapper.Map<RestaurantProductDto>(restaurantProduct);
      }
    }

    public async Task<bool> RemoveProductFromRestaurantAsync(int restaurantId, int productId)
    {
      var restaurantProduct = await _restaurantProductRepository.GetByRestaurantAndProductAsync(restaurantId, productId);
      if (restaurantProduct == null) return false;
      await _restaurantProductRepository.DeleteAsync(restaurantProduct.Id);
      return true;
    }

    public async Task<List<RestaurantProductDto>> BulkAssignProductsAsync(int restaurantId, List<CreateRestaurantProductDto> products)
    {
      var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
      if (restaurant == null)
        throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

      var restaurantProductsToCreate = new List<RestaurantProduct>();
      foreach (var productDto in products)
      {
        var product = await _productRepository.GetByIdAsync(productDto.ProductId);
        if (product == null) throw new KeyNotFoundException($"Product with ID {productDto.ProductId} not found");
        if (await _restaurantProductRepository.ExistsAsync(restaurantId, productDto.ProductId)) continue;

        var rp = _mapper.Map<RestaurantProduct>(productDto);
        rp.RestaurantId = restaurantId;
        rp.CreatedAt = DateTime.UtcNow;
        rp.UpdatedAt = DateTime.UtcNow;
        restaurantProductsToCreate.Add(rp);
      }

      if (restaurantProductsToCreate.Any())
      {
        await _restaurantProductRepository.BulkInsertAsync(restaurantProductsToCreate);
      }
      var createdDtos = _mapper.Map<List<RestaurantProductDto>>(restaurantProductsToCreate);
      return createdDtos;
    }

    public async Task<List<RestaurantProductDto>> GetProductsByBusinessAsync(int businessId)
    {
      var restaurantProducts = await _restaurantProductRepository.GetByBusinessIdAsync(businessId);
      return _mapper.Map<List<RestaurantProductDto>>(restaurantProducts);
    }

    public async Task<List<RestaurantProductOfferingDto>> GetRestaurantOfferingsForProductAsync(int productId)
    {
        var offerings = await _restaurantProductRepository.GetByProductIdAsync(productId);
        return _mapper.Map<List<RestaurantProductOfferingDto>>(offerings);
    }
  }
}
