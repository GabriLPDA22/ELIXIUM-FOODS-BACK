using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;
using UberEatsBackend.Data;
using UberEatsBackend.DTOs.Restaurant;
using AutoMapper;
using System.Linq;

namespace UberEatsBackend.Services
{
  public class RestaurantService : IRestaurantService
  {
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IRestaurantHourService _restaurantHourService;

    public RestaurantService(
        IRestaurantRepository restaurantRepository,
        ApplicationDbContext context,
        IMapper mapper,
        IRestaurantHourService restaurantHourService)
    {
      _restaurantRepository = restaurantRepository;
      _context = context;
      _mapper = mapper;
      _restaurantHourService = restaurantHourService;
    }

    public async Task<List<Restaurant>> GetAllRestaurantsAsync()
    {
      return await _context.Restaurants
          .OrderBy(r => r.Name)
          .ToListAsync();
    }

    public async Task<List<Restaurant>> GetPopularRestaurantsAsync(int limit = 10)
    {
      return await _restaurantRepository.GetPopularRestaurantsAsync(limit);
    }

    public async Task<List<Restaurant>> SearchRestaurantsAsync(string query, int? categoryId)
    {
      return await _restaurantRepository.SearchRestaurantsAsync(query, categoryId);
    }

    public async Task<Restaurant?> GetRestaurantWithDetailsAsync(int id)
    {
      return await _restaurantRepository.GetWithDetailsAsync(id);
    }

    public async Task<bool> IsUserAuthorizedForRestaurant(int restaurantId, int userId, string userRole)
    {
      if (userRole == "Admin")
        return true;

      if (userRole == "Business")
      {
        var restaurant = await _context.Restaurants
            .Include(r => r.Business)
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

        return restaurant?.Business?.UserId == userId;
      }

      return false;
    }

    public async Task<List<Restaurant>> GetRestaurantsByTipoAsync(int tipo)
    {
      return await _restaurantRepository.GetByTipoAsync(tipo);
    }

    public async Task<List<Restaurant>> GetRestaurantsByBusinessIdAsync(int businessId)
    {
      return await _restaurantRepository.GetByBusinessIdAsync(businessId);
    }

    public async Task<bool> IsBusinessOwner(int businessId, int userId)
    {
      var business = await _context.Businesses.FindAsync(businessId);
      return business?.UserId == userId;
    }

    public async Task<List<Restaurant>> GetRestaurantsForAdminAsync(int userId)
    {
      return await _context.Restaurants
          .Include(r => r.Address)
          .Include(r => r.Business)
          .OrderBy(r => r.Name)
          .ToListAsync();
    }
    public async Task<List<RestaurantCardWithStatusDto>> GetAllRestaurantsWithStatusAsync()
    {
      var restaurants = await _context.Restaurants
          .Include(r => r.Business)
          .OrderBy(r => r.Name)
          .ToListAsync();

      var restaurantsWithStatus = new List<RestaurantCardWithStatusDto>();

      foreach (var restaurant in restaurants)
      {
        var restaurantCard = _mapper.Map<RestaurantCardDto>(restaurant);
        var isOpen = await _restaurantHourService.IsRestaurantOpenAsync(restaurant.Id);
        var status = await _restaurantHourService.GetRestaurantStatusAsync(restaurant.Id);

        var restaurantWithStatus = new RestaurantCardWithStatusDto
        {
          Id = restaurantCard.Id,
          Name = restaurantCard.Name,
          Description = restaurantCard.Description,
          LogoUrl = restaurantCard.LogoUrl,
          CoverImageUrl = restaurantCard.CoverImageUrl,
          AverageRating = restaurantCard.AverageRating,
          IsOpen = restaurantCard.IsOpen,
          DeliveryFee = restaurantCard.DeliveryFee,
          EstimatedDeliveryTime = restaurantCard.EstimatedDeliveryTime,
          Cuisine = restaurantCard.Cuisine,
          ReviewCount = restaurantCard.ReviewCount,
          Distance = restaurantCard.Distance,
          Featured = restaurantCard.Featured,
          IsNew = restaurantCard.IsNew,
          PromoText = restaurantCard.PromoText,
          PriceRange = restaurantCard.PriceRange,
          OrderCount = restaurantCard.OrderCount,
          Tipo = restaurantCard.Tipo,
          BusinessId = restaurantCard.BusinessId,
          BusinessName = restaurantCard.BusinessName,
          IsCurrentlyOpen = isOpen,
          Status = isOpen ? "Abierto" : "Cerrado",
          StatusMessage = status
        };

        restaurantsWithStatus.Add(restaurantWithStatus);
      }

      return restaurantsWithStatus;
    }

    public async Task<RestaurantDetailWithStatusDto?> GetRestaurantWithStatusAsync(int id)
    {
      var restaurant = await _context.Restaurants
          .Include(r => r.Address)
          .Include(r => r.Business)
          .FirstOrDefaultAsync(r => r.Id == id);

      if (restaurant == null) return null;

      var restaurantDto = _mapper.Map<RestaurantDto>(restaurant);
      var isOpen = await _restaurantHourService.IsRestaurantOpenAsync(id);
      var status = await _restaurantHourService.GetRestaurantStatusAsync(id);
      var hours = await _restaurantHourService.GetRestaurantHoursAsync(id);

      return new RestaurantDetailWithStatusDto
      {
        Id = restaurantDto.Id,
        Name = restaurantDto.Name,
        Description = restaurantDto.Description,
        LogoUrl = restaurantDto.LogoUrl,
        CoverImageUrl = restaurantDto.CoverImageUrl,
        AverageRating = restaurantDto.AverageRating,
        IsOpen = restaurantDto.IsOpen,
        DeliveryFee = restaurantDto.DeliveryFee,
        EstimatedDeliveryTime = restaurantDto.EstimatedDeliveryTime,
        Address = restaurantDto.Address,
        CreatedAt = restaurantDto.CreatedAt,
        UpdatedAt = restaurantDto.UpdatedAt,
        Tipo = restaurantDto.Tipo,
        BusinessId = restaurantDto.BusinessId,
        BusinessName = restaurantDto.BusinessName,
        IsCurrentlyOpen = isOpen,
        Status = isOpen ? "Abierto" : "Cerrado",
        StatusMessage = status,
        Hours = hours
      };
    }

    public async Task<bool> IsRestaurantCurrentlyOpenAsync(int restaurantId)
    {
      return await _restaurantHourService.IsRestaurantOpenAsync(restaurantId);
    }

    public async Task<string> GetRestaurantStatusMessageAsync(int restaurantId)
    {
      return await _restaurantHourService.GetRestaurantStatusAsync(restaurantId);
    }
  }
}
