// UberEatsBackend/Services/BusinessService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
    private readonly IMapper _mapper;

    public BusinessService(
        IBusinessRepository businessRepository,
        IOrderRepository orderRepository,
        IPromotionRepository promotionRepository,
        IMapper mapper)
    {
      _businessRepository = businessRepository;
      _orderRepository = orderRepository;
      _promotionRepository = promotionRepository;
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

    public async Task<List<BusinessDto>> GetBusinessesByOwnerIdAsync(int userId)
    {
      var businesses = await _businessRepository.GetByOwnerIdAsync(userId);
      return _mapper.Map<List<BusinessDto>>(businesses);
    }

    public async Task<BusinessDto> CreateBusinessAsync(int userId, CreateBusinessDto createBusinessDto)
    {
      var business = _mapper.Map<Business>(createBusinessDto);
      business.UserId = userId;
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

    public async Task<bool> IsUserAuthorizedForBusiness(int businessId, int userId, string userRole)
    {
      if (userRole == "Admin")
        return true;

      return await _businessRepository.IsOwner(businessId, userId);
    }

    public async Task<BusinessStatsDto> GetBusinessStatsAsync(int businessId)
    {
      // Obtener el negocio con sus restaurantes
      var business = await _businessRepository.GetWithDetailsAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

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

      // Calcular estadísticas para cada restaurante
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
          ProductCount = restaurant.Menus
              .SelectMany(m => m.Categories)
              .SelectMany(c => c.Products)
              .Count()
        };

        stats.RestaurantStats.Add(restaurantStat);
        stats.TotalOrders += restaurantStat.OrderCount;
        stats.TotalRevenue += restaurantStat.Revenue;

        // Acumular para calcular promedio general
        if (restaurant.AverageRating > 0)
        {
          totalRating += restaurant.AverageRating;
          totalRatingCount++;
        }
      }

      // Calcular promedio general si hay calificaciones
      stats.AverageRating = totalRatingCount > 0 ? totalRating / totalRatingCount : 0;

      return stats;
    }

    // Image management methods
    public async Task UpdateLogoAsync(int businessId, string? logoUrl)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      business.LogoUrl = logoUrl ?? string.Empty;
      business.UpdatedAt = DateTime.UtcNow;

      await _businessRepository.UpdateAsync(business);
    }

    public async Task UpdateCoverImageAsync(int businessId, string? coverImageUrl)
    {
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      business.CoverImageUrl = coverImageUrl ?? string.Empty;
      business.UpdatedAt = DateTime.UtcNow;

      await _businessRepository.UpdateAsync(business);
    }

    // Business hours methods
    public async Task<List<BusinessHourDto>> GetBusinessHoursAsync(int businessId)
    {
      // Verificar que el negocio existe
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      // Implementación futura: Obtener los horarios del negocio desde el repositorio
      // Por ahora, retornar lista vacía
      return new List<BusinessHourDto>();
    }

    public async Task<BusinessHourDto> AddBusinessHourAsync(int businessId, CreateBusinessHourDto createBusinessHourDto)
    {
      // Verificar que el negocio existe
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      // Implementación futura: Agregar horario al negocio
      throw new NotImplementedException("Método no implementado todavía");
    }

    public async Task<BusinessHourDto?> UpdateBusinessHourAsync(int businessId, int hourId, UpdateBusinessHourDto updateBusinessHourDto)
    {
      // Verificar que el negocio existe
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      // Implementación futura: Actualizar horario
      throw new NotImplementedException("Método no implementado todavía");
    }

    public async Task<bool> DeleteBusinessHourAsync(int businessId, int hourId)
    {
      // Verificar que el negocio existe
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      // Implementación futura: Eliminar horario
      throw new NotImplementedException("Método no implementado todavía");
    }

    // Promotions methods
    public async Task<List<PromotionDto>> GetBusinessPromotionsAsync(int businessId)
    {
      // Verificar que el negocio existe
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      // Obtener las promociones del negocio
      var promotions = await _promotionRepository.GetByBusinessIdAsync(businessId);
      return _mapper.Map<List<PromotionDto>>(promotions);
    }

    public async Task<PromotionDto> CreatePromotionAsync(int businessId, CreatePromotionDto createPromotionDto)
    {
      // Verificar que el negocio existe
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      // Verificar que el código es único
      if (!await _promotionRepository.IsCodeUniqueAsync(createPromotionDto.Code))
        throw new InvalidOperationException($"El código de promoción '{createPromotionDto.Code}' ya está en uso");

      // Crear la promoción
      var promotion = _mapper.Map<Promotion>(createPromotionDto);
      promotion.BusinessId = businessId;
      promotion.UsageCount = 0;
      promotion.Status = "active";

      var createdPromotion = await _promotionRepository.AddAsync(promotion);
      return _mapper.Map<PromotionDto>(createdPromotion);
    }

    public async Task<PromotionDto?> UpdatePromotionAsync(int businessId, int promotionId, UpdatePromotionDto updatePromotionDto)
    {
      // Verificar que el negocio existe
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      // Verificar que la promoción existe y pertenece al negocio
      var promotion = await _promotionRepository.GetByIdAsync(promotionId);
      if (promotion == null || promotion.BusinessId != businessId)
        return null;

      // Verificar que el código es único si se está actualizando
      if (updatePromotionDto.Code != null && updatePromotionDto.Code != promotion.Code)
      {
        if (!await _promotionRepository.IsCodeUniqueAsync(updatePromotionDto.Code, promotionId))
          throw new InvalidOperationException($"El código de promoción '{updatePromotionDto.Code}' ya está en uso");

        promotion.Code = updatePromotionDto.Code;
      }

      // Actualizar propiedades si están definidas
      if (updatePromotionDto.Name != null)
        promotion.Name = updatePromotionDto.Name;

      if (updatePromotionDto.Description != null)
        promotion.Description = updatePromotionDto.Description;

      if (updatePromotionDto.Type != null)
        promotion.Type = updatePromotionDto.Type;

      if (updatePromotionDto.DiscountType != null)
        promotion.DiscountType = updatePromotionDto.DiscountType;

      if (updatePromotionDto.DiscountValue.HasValue)
        promotion.DiscountValue = updatePromotionDto.DiscountValue.Value;

      if (updatePromotionDto.StartDate.HasValue)
        promotion.StartDate = updatePromotionDto.StartDate.Value;

      if (updatePromotionDto.EndDate.HasValue)
        promotion.EndDate = updatePromotionDto.EndDate.Value;

      if (updatePromotionDto.MinimumOrderValue.HasValue)
        promotion.MinimumOrderValue = updatePromotionDto.MinimumOrderValue.Value;

      if (updatePromotionDto.UsageLimit.HasValue)
        promotion.UsageLimit = updatePromotionDto.UsageLimit.Value;

      await _promotionRepository.UpdateAsync(promotion);
      return _mapper.Map<PromotionDto>(promotion);
    }

    public async Task<bool> DeletePromotionAsync(int businessId, int promotionId)
    {
      // Verificar que el negocio existe
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      // Verificar que la promoción existe y pertenece al negocio
      var promotion = await _promotionRepository.GetByIdAsync(promotionId);
      if (promotion == null || promotion.BusinessId != businessId)
        return false;

      await _promotionRepository.DeleteAsync(promotion);
      return true;
    }

    public async Task<bool> ActivatePromotionAsync(int businessId, int promotionId)
    {
      // Verificar que el negocio existe
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      // Verificar que la promoción existe y pertenece al negocio
      var promotion = await _promotionRepository.GetByIdAsync(promotionId);
      if (promotion == null || promotion.BusinessId != businessId)
        return false;

      promotion.Status = "active";
      await _promotionRepository.UpdateAsync(promotion);
      return true;
    }

    public async Task<bool> DeactivatePromotionAsync(int businessId, int promotionId)
    {
      // Verificar que el negocio existe
      var business = await _businessRepository.GetByIdAsync(businessId);
      if (business == null)
        throw new KeyNotFoundException($"Negocio con ID {businessId} no encontrado");

      // Verificar que la promoción existe y pertenece al negocio
      var promotion = await _promotionRepository.GetByIdAsync(promotionId);
      if (promotion == null || promotion.BusinessId != businessId)
        return false;

      promotion.Status = "inactive";
      await _promotionRepository.UpdateAsync(promotion);
      return true;
    }
  }
}
