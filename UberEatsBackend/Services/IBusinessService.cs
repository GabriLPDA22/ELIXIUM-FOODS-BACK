using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Business;
using UberEatsBackend.Models;

namespace UberEatsBackend.Services
{
    public interface IBusinessService
    {
        // Métodos básicos CRUD
        Task<List<BusinessDto>> GetAllBusinessesAsync();
        Task<BusinessDto?> GetBusinessByIdAsync(int id);
        Task<List<BusinessDto>> GetBusinessesByOwnerIdAsync(int userId);
        Task<BusinessDto> CreateBusinessAsync(int userId, CreateBusinessDto createBusinessDto);
        Task<BusinessDto?> UpdateBusinessAsync(int id, UpdateBusinessDto updateBusinessDto);
        Task<bool> DeleteBusinessAsync(int id);

        // Métodos de autorización
        Task<bool> IsUserAuthorizedForBusiness(int businessId, int userId, string userRole);

        // Métodos de estadísticas
        Task<BusinessStatsDto> GetBusinessStatsAsync(int businessId);

        // Métodos para gestión de imágenes
        Task UpdateLogoAsync(int businessId, string? logoUrl);
        Task UpdateCoverImageAsync(int businessId, string? coverImageUrl);

        // Métodos para horarios de negocio (opcional)
        Task<List<BusinessHourDto>> GetBusinessHoursAsync(int businessId);
        Task<BusinessHourDto> AddBusinessHourAsync(int businessId, CreateBusinessHourDto createBusinessHourDto);
        Task<BusinessHourDto?> UpdateBusinessHourAsync(int businessId, int hourId, UpdateBusinessHourDto updateBusinessHourDto);
        Task<bool> DeleteBusinessHourAsync(int businessId, int hourId);

        // Métodos para promociones (opcional)
        Task<List<PromotionDto>> GetBusinessPromotionsAsync(int businessId);
        Task<PromotionDto> CreatePromotionAsync(int businessId, CreatePromotionDto createPromotionDto);
        Task<PromotionDto?> UpdatePromotionAsync(int businessId, int promotionId, UpdatePromotionDto updatePromotionDto);
        Task<bool> DeletePromotionAsync(int businessId, int promotionId);
        Task<bool> ActivatePromotionAsync(int businessId, int promotionId);
        Task<bool> DeactivatePromotionAsync(int businessId, int promotionId);
    }
}
