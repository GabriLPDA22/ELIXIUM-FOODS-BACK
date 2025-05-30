using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Data;
using UberEatsBackend.Models;

namespace UberEatsBackend.Repositories
{
    public class ProductOfferRepository : IProductOfferRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductOfferRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductOffer>> GetAllAsync()
        {
            return await _context.ProductOffers
                .Include(po => po.Restaurant)
                .Include(po => po.Product)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductOffer?> GetByIdAsync(int id)
        {
            return await _context.ProductOffers
                .Include(po => po.Restaurant)
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task<List<ProductOffer>> GetByRestaurantIdAsync(int restaurantId)
        {
            return await _context.ProductOffers
                .Include(po => po.Restaurant)
                .Include(po => po.Product)
                .Where(po => po.RestaurantId == restaurantId)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ProductOffer>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductOffers
                .Include(po => po.Restaurant)
                .Include(po => po.Product)
                .Where(po => po.ProductId == productId)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ProductOffer>> GetByRestaurantAndProductAsync(int restaurantId, int productId)
        {
            return await _context.ProductOffers
                .Include(po => po.Restaurant)
                .Include(po => po.Product)
                .Where(po => po.RestaurantId == restaurantId && po.ProductId == productId)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ProductOffer>> GetActiveOffersAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.ProductOffers
                .Include(po => po.Restaurant)
                .Include(po => po.Product)
                .Where(po => po.Status == "active" &&
                           po.StartDate <= now &&
                           po.EndDate >= now &&
                           (po.UsageLimit == 0 || po.UsageCount < po.UsageLimit))
                .ToListAsync();
        }

        public async Task<List<ProductOffer>> GetActiveOffersByRestaurantAsync(int restaurantId)
        {
            var now = DateTime.UtcNow;
            return await _context.ProductOffers
                .Include(po => po.Restaurant)
                .Include(po => po.Product)
                .Where(po => po.RestaurantId == restaurantId &&
                           po.Status == "active" &&
                           po.StartDate <= now &&
                           po.EndDate >= now &&
                           (po.UsageLimit == 0 || po.UsageCount < po.UsageLimit))
                .ToListAsync();
        }

        public async Task<List<ProductOffer>> GetActiveOffersByProductAsync(int productId)
        {
            var now = DateTime.UtcNow;
            return await _context.ProductOffers
                .Include(po => po.Restaurant)
                .Include(po => po.Product)
                .Where(po => po.ProductId == productId &&
                           po.Status == "active" &&
                           po.StartDate <= now &&
                           po.EndDate >= now &&
                           (po.UsageLimit == 0 || po.UsageCount < po.UsageLimit))
                .ToListAsync();
        }

        public async Task<ProductOffer> CreateAsync(ProductOffer productOffer)
        {
            productOffer.CreatedAt = DateTime.UtcNow;
            productOffer.UpdatedAt = DateTime.UtcNow;

            _context.ProductOffers.Add(productOffer);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(productOffer.Id) ?? productOffer;
        }

        public async Task<ProductOffer> UpdateAsync(ProductOffer productOffer)
        {
            productOffer.UpdatedAt = DateTime.UtcNow;

            _context.ProductOffers.Update(productOffer);
            await _context.SaveChangesAsync();

            return productOffer;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var productOffer = await _context.ProductOffers.FindAsync(id);
            if (productOffer == null)
                return false;

            _context.ProductOffers.Remove(productOffer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProductOffer>> GetExpiredOffersAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.ProductOffers
                .Where(po => po.Status == "active" && po.EndDate < now)
                .ToListAsync();
        }

        public async Task<bool> IncrementUsageCountAsync(int offerId)
        {
            var offer = await _context.ProductOffers.FindAsync(offerId);
            if (offer == null)
                return false;

            offer.UsageCount++;
            offer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
