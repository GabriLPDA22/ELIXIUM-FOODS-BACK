using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Data;
using UberEatsBackend.DTOs.Business;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;

namespace UberEatsBackend.Services
{
  public class BusinessService : IBusinessService
  {
    private readonly IBusinessRepository _businessRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public BusinessService(
        IBusinessRepository businessRepository,
        IOrderRepository orderRepository,
        ApplicationDbContext context,
        IMapper mapper)
    {
      _businessRepository = businessRepository;
      _orderRepository = orderRepository;
      _context = context;
      _mapper = mapper;
    }

    public async Task<List<BusinessDto>> GetAllBusinessesAsync()
    {
      var businesses = await _businessRepository.GetAllAsync();
      return _mapper.Map<List<BusinessDto>>(businesses);
    }

    public async Task<BusinessDto?> GetBusinessByIdAsync(int id)
    {
      var business = await _businessRepository.GetWithDetailsAsync(id);
      return business != null ? _mapper.Map<BusinessDto>(business) : null;
    }

    public async Task<BusinessDto> CreateBusinessAsync(CreateBusinessDto createBusinessDto)
    {
      var business = _mapper.Map<Business>(createBusinessDto);
      business.CreatedAt = DateTime.UtcNow;
      business.UpdatedAt = DateTime.UtcNow;
      var createdBusiness = await _businessRepository.CreateAsync(business);
      return _mapper.Map<BusinessDto>(createdBusiness);
    }

    public async Task<BusinessDto?> UpdateBusinessAsync(int id, UpdateBusinessDto updateBusinessDto)
    {
      var business = await _businessRepository.GetByIdAsync(id);
      if (business == null)
        return null;

      _mapper.Map(updateBusinessDto, business);
      business.UpdatedAt = DateTime.UtcNow;

      await _businessRepository.UpdateAsync(business);
      return _mapper.Map<BusinessDto>(business);
    }

    public async Task<bool> DeleteBusinessAsync(int id)
    {
      var business = await _businessRepository.GetByIdAsync(id);
      if (business == null)
        return false;

      await _businessRepository.DeleteAsync(business.Id);
      return true;
    }

    public bool IsAdministrator(string userRole)
    {
      return userRole == "Admin";
    }

    public async Task<bool> IsUserAuthorizedForBusiness(int businessId, int userId, string userRole)
    {
      if (userRole == "Admin")
        return true;

      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        return false;

      return business.UserId.HasValue && business.UserId.Value == userId;
    }

    public async Task<BusinessDto?> GetBusinessByAssignedUserIdAsync(int userId)
    {
        var business = await _context.Businesses
            .Include(b => b.User)
            .Include(b => b.Restaurants)
            .FirstOrDefaultAsync(b => b.UserId == userId);

        return business != null ? _mapper.Map<BusinessDto>(business) : null;
    }

    public async Task<BusinessStatsDto> GetBusinessStatsAsync(int businessId)
    {
      var business = await _businessRepository.GetWithDetailsAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");

      var stats = new BusinessStatsDto
      {
        BusinessId = business.Id,
        BusinessName = business.Name,
        TotalRestaurants = business.Restaurants.Count,
        TotalOrders = 0,
        TotalRevenue = 0,
        AverageRating = 0,
        RestaurantStats = new List<RestaurantStatsDto>()
      };

      double totalRating = 0;
      int totalRatingCount = 0;

      foreach (var restaurant in business.Restaurants)
      {
        var orders = await _orderRepository.GetOrdersByRestaurantIdAsync(restaurant.Id);

        // Obtener productos del restaurante a través de RestaurantProducts
        var productCount = await _context.RestaurantProducts
            .Where(rp => rp.RestaurantId == restaurant.Id)
            .CountAsync();

        var restaurantStat = new RestaurantStatsDto
        {
          RestaurantId = restaurant.Id,
          RestaurantName = restaurant.Name,
          OrderCount = orders.Count,
          Revenue = orders.Sum(o => o.Total),
          AverageRating = restaurant.AverageRating,
          ProductCount = productCount // FIXED: Ya no usamos Menus
        };

        stats.RestaurantStats.Add(restaurantStat);
        stats.TotalOrders += restaurantStat.OrderCount;
        stats.TotalRevenue += restaurantStat.Revenue;

        if (restaurant.AverageRating > 0)
        {
          totalRating += restaurant.AverageRating;
          totalRatingCount++;
        }
      }

      stats.AverageRating = totalRatingCount > 0 ? totalRating / totalRatingCount : 0;
      return stats;
    }

    public async Task UpdateLogoAsync(int businessId, string? logoUrl)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      business.LogoUrl = logoUrl ?? string.Empty;
      business.UpdatedAt = DateTime.UtcNow;
      await _businessRepository.UpdateAsync(business);
    }

    public async Task UpdateCoverImageAsync(int businessId, string? coverImageUrl)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      business.CoverImageUrl = coverImageUrl ?? string.Empty;
      business.UpdatedAt = DateTime.UtcNow;
      await _businessRepository.UpdateAsync(business);
    }

    public async Task<List<BusinessHourDto>> GetBusinessHoursAsync(int businessId)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      return new List<BusinessHourDto>();
    }

    public async Task<BusinessHourDto> AddBusinessHourAsync(int businessId, CreateBusinessHourDto createBusinessHourDto)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      throw new NotImplementedException("Method not implemented yet");
    }

    public async Task<BusinessHourDto?> UpdateBusinessHourAsync(int businessId, int hourId, UpdateBusinessHourDto updateBusinessHourDto)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      throw new NotImplementedException("Method not implemented yet");
    }

    public async Task<bool> DeleteBusinessHourAsync(int businessId, int hourId)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      throw new NotImplementedException("Method not implemented yet");
    }
  }
}
