// UberEatsBackend/Repositories/IProductOfferRepository.cs
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
  public interface IProductOfferRepository
  {
    Task<List<ProductOffer>> GetAllAsync();
    Task<ProductOffer?> GetByIdAsync(int id);
    Task<List<ProductOffer>> GetByRestaurantIdAsync(int restaurantId);
    Task<List<ProductOffer>> GetByProductIdAsync(int productId);
    Task<List<ProductOffer>> GetActiveOffersAsync();
    Task<List<ProductOffer>> GetActiveOffersByRestaurantAsync(int restaurantId);

    // ✅ NUEVO: Método para obtener oferta activa de un producto específico en un restaurante
    Task<ProductOffer?> GetActiveOfferForProductInRestaurantAsync(int restaurantId, int productId);

    Task<ProductOffer> CreateAsync(ProductOffer productOffer);
    Task<ProductOffer> UpdateAsync(ProductOffer productOffer);
    Task<bool> DeleteAsync(int id);
    Task<bool> IncrementUsageCountAsync(int offerId);
  }
}
