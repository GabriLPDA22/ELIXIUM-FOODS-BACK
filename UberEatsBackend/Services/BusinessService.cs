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
    private readonly IPromotionRepository _promotionRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public BusinessService(
        IBusinessRepository businessRepository,
        IOrderRepository orderRepository,
        IPromotionRepository promotionRepository,
        ApplicationDbContext context,
        IMapper mapper)
    {
      _businessRepository = businessRepository;
      _orderRepository = orderRepository;
      _promotionRepository = promotionRepository;
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
      var createdBusiness = await _businessRepository.AddAsync(business);
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

      await _businessRepository.DeleteAsync(business);
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
        var restaurantStat = new RestaurantStatsDto
        {
          RestaurantId = restaurant.Id,
          RestaurantName = restaurant.Name,
          OrderCount = orders.Count,
          Revenue = orders.Sum(o => o.Total),
          AverageRating = restaurant.AverageRating,
          ProductCount = restaurant.Menus?.SelectMany(m => m.Categories)?.SelectMany(c => c.Products)?.Count() ?? 0
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

    public async Task<List<PromotionDto>> GetBusinessPromotionsAsync(int businessId)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      var promotions = await _promotionRepository.GetByBusinessIdAsync(businessId);
      return _mapper.Map<List<PromotionDto>>(promotions);
    }

    public async Task<PromotionDto> CreatePromotionAsync(int businessId, CreatePromotionDto createPromotionDto)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      if (!await _promotionRepository.IsCodeUniqueAsync(createPromotionDto.Code))
        throw new InvalidOperationException($"Promotion code '{createPromotionDto.Code}' is already in use");
      var promotion = _mapper.Map<Promotion>(createPromotionDto);
      promotion.BusinessId = businessId;
      promotion.UsageCount = 0;
      promotion.Status = "active";
      var createdPromotion = await _promotionRepository.AddAsync(promotion);
      return _mapper.Map<PromotionDto>(createdPromotion);
    }

    public async Task<PromotionDto?> UpdatePromotionAsync(int businessId, int promotionId, UpdatePromotionDto updatePromotionDto)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      var promotion = await _promotionRepository.GetByIdAsync(promotionId);
      if (promotion == null || promotion.BusinessId != businessId)
        return null;
      if (updatePromotionDto.Code != null && updatePromotionDto.Code != promotion.Code)
      {
        if (!await _promotionRepository.IsCodeUniqueAsync(updatePromotionDto.Code, promotionId))
          throw new InvalidOperationException($"Promotion code '{updatePromotionDto.Code}' is already in use");
        promotion.Code = updatePromotionDto.Code;
      }
      if (updatePromotionDto.Name != null) promotion.Name = updatePromotionDto.Name;
      if (updatePromotionDto.Description != null) promotion.Description = updatePromotionDto.Description;
      if (updatePromotionDto.Type != null) promotion.Type = updatePromotionDto.Type;
      if (updatePromotionDto.DiscountType != null) promotion.DiscountType = updatePromotionDto.DiscountType;
      if (updatePromotionDto.DiscountValue.HasValue) promotion.DiscountValue = updatePromotionDto.DiscountValue.Value;
      if (updatePromotionDto.StartDate.HasValue) promotion.StartDate = updatePromotionDto.StartDate.Value;
      if (updatePromotionDto.EndDate.HasValue) promotion.EndDate = updatePromotionDto.EndDate.Value;
      if (updatePromotionDto.MinimumOrderValue.HasValue) promotion.MinimumOrderValue = updatePromotionDto.MinimumOrderValue.Value;
      if (updatePromotionDto.UsageLimit.HasValue) promotion.UsageLimit = updatePromotionDto.UsageLimit.Value;
      await _promotionRepository.UpdateAsync(promotion);
      return _mapper.Map<PromotionDto>(promotion);
    }

    public async Task<bool> DeletePromotionAsync(int businessId, int promotionId)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      var promotion = await _promotionRepository.GetByIdAsync(promotionId);
      if (promotion == null || promotion.BusinessId != businessId)
        return false;
      await _promotionRepository.DeleteAsync(promotion);
      return true;
    }

    public async Task<bool> ActivatePromotionAsync(int businessId, int promotionId)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      var promotion = await _promotionRepository.GetByIdAsync(promotionId);
      if (promotion == null || promotion.BusinessId != businessId)
        return false;
      promotion.Status = "active";
      await _promotionRepository.UpdateAsync(promotion);
      return true;
    }

    public async Task<bool> DeactivatePromotionAsync(int businessId, int promotionId)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Business with ID {businessId} not found");
      var promotion = await _promotionRepository.GetByIdAsync(promotionId);
      if (promotion == null || promotion.BusinessId != businessId)
        return false;
      promotion.Status = "inactive";
      await _promotionRepository.UpdateAsync(promotion);
      return true;
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
