using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
    public interface IProductOfferRepository
    {
        Task<List<ProductOffer>> GetAllAsync();
        Task<ProductOffer?> GetByIdAsync(int id);
        Task<List<ProductOffer>> GetByRestaurantIdAsync(int restaurantId);
        Task<List<ProductOffer>> GetByProductIdAsync(int productId);
        Task<List<ProductOffer>> GetByRestaurantAndProductAsync(int restaurantId, int productId);
        Task<List<ProductOffer>> GetActiveOffersAsync();
        Task<List<ProductOffer>> GetActiveOffersByRestaurantAsync(int restaurantId);
        Task<List<ProductOffer>> GetActiveOffersByProductAsync(int productId);
        Task<ProductOffer> CreateAsync(ProductOffer productOffer);
        Task<ProductOffer> UpdateAsync(ProductOffer productOffer);
        Task<bool> DeleteAsync(int id);
        Task<List<ProductOffer>> GetExpiredOffersAsync();
        Task<bool> IncrementUsageCountAsync(int offerId);
    }
}
