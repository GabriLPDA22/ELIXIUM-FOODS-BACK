// UberEatsBackend/Services/IProductOfferService.cs
using UberEatsBackend.DTOs.Offers;

using UberEatsBackend.DTOs.Offers;

namespace UberEatsBackend.Services
{
  public interface IProductOfferService
  {
    // ✅ BÁSICOS
    Task<List<ProductOfferDto>> GetAllOffersAsync();
    Task<ProductOfferDto?> GetOfferByIdAsync(int id);
    Task<List<ProductOfferDto>> GetOffersByRestaurantAsync(int restaurantId);
    Task<List<ProductOfferDto>> GetOffersByProductAsync(int productId);
    Task<List<ProductOfferDto>> GetActiveOffersAsync();
    Task<List<ProductOfferDto>> GetActiveOffersByRestaurantAsync(int restaurantId);

    // ✅ PRINCIPAL - Para tu endpoint
    Task<ActiveOfferResponseDto?> GetActiveOfferForProductInRestaurantAsync(int restaurantId, int productId);

    // ✅ CRUD - Para ProductOffersController
    Task<ProductOfferDto> CreateOfferAsync(int restaurantId, CreateProductOfferDto createDto);
    Task<ProductOfferDto?> UpdateOfferAsync(int restaurantId, int offerId, UpdateProductOfferDto updateDto);
    Task<bool> DeleteOfferAsync(int restaurantId, int offerId);
    Task<bool> ActivateOfferAsync(int restaurantId, int offerId);
    Task<bool> DeactivateOfferAsync(int restaurantId, int offerId);

    // ✅ VALIDACIÓN - Para cálculos
    Task<List<OfferValidationDto>> ValidateOffersForOrder(int restaurantId, List<(int productId, int quantity)> orderItems, decimal orderSubtotal);
    Task<List<ProductOfferSummaryDto>> CalculateOffersForProducts(int restaurantId, List<(int productId, int quantity, decimal unitPrice)> products, decimal orderSubtotal);

    // ✅ USAGE - Para OrderService
    Task<bool> ApplyOfferUsage(int offerId);
  }
}

