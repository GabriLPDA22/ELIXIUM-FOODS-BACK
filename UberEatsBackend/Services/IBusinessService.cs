using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Business;
using UberEatsBackend.Models;

namespace UberEatsBackend.Services
{
    public interface IBusinessService
    {
        Task<List<BusinessDto>> GetAllBusinessesAsync();
        Task<BusinessDto?> GetBusinessByIdAsync(int id);
        Task<BusinessDto> CreateBusinessAsync(CreateBusinessDto createBusinessDto);
        Task<BusinessDto?> UpdateBusinessAsync(int id, UpdateBusinessDto updateBusinessDto);
        Task<bool> DeleteBusinessAsync(int id);

        bool IsAdministrator(string userRole);
        Task<bool> IsUserAuthorizedForBusiness(int businessId, int userId, string userRole);
        Task<BusinessDto?> GetBusinessByAssignedUserIdAsync(int userId);

        Task<BusinessStatsDto> GetBusinessStatsAsync(int businessId);

        Task UpdateLogoAsync(int businessId, string? logoUrl);
        Task UpdateCoverImageAsync(int businessId, string? coverImageUrl);

        Task<List<BusinessHourDto>> GetBusinessHoursAsync(int businessId);
        Task<BusinessHourDto> AddBusinessHourAsync(int businessId, CreateBusinessHourDto createBusinessHourDto);
        Task<BusinessHourDto?> UpdateBusinessHourAsync(int businessId, int hourId, UpdateBusinessHourDto updateBusinessHourDto);
        Task<bool> DeleteBusinessHourAsync(int businessId, int hourId);

    }
}
