using AutoMapper;
using UberEatsBackend.DTOs.Offers;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;

namespace UberEatsBackend.Services
{
  public class ProductOfferService : IProductOfferService
  {
    private readonly IProductOfferRepository _productOfferRepository;
    private readonly IRepository<Restaurant> _restaurantRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRestaurantProductRepository _restaurantProductRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductOfferService> _logger;

    public ProductOfferService(
        IProductOfferRepository productOfferRepository,
        IRepository<Restaurant> restaurantRepository,
        IRepository<Product> productRepository,
        IRestaurantProductRepository restaurantProductRepository,
        IMapper mapper,
        ILogger<ProductOfferService> logger)
    {
      _productOfferRepository = productOfferRepository;
      _restaurantRepository = restaurantRepository;
      _productRepository = productRepository;
      _restaurantProductRepository = restaurantProductRepository;
      _mapper = mapper;
      _logger = logger;
    }

    public async Task<List<ProductOfferDto>> GetAllOffersAsync()
    {
      var offers = await _productOfferRepository.GetAllAsync();
      return MapToProductOfferDtos(offers);
    }

    public async Task<ProductOfferDto?> GetOfferByIdAsync(int id)
    {
      var offer = await _productOfferRepository.GetByIdAsync(id);
      return offer != null ? MapToProductOfferDto(offer) : null;
    }

    public async Task<List<ProductOfferDto>> GetOffersByRestaurantAsync(int restaurantId)
    {
      var offers = await _productOfferRepository.GetByRestaurantIdAsync(restaurantId);
      return MapToProductOfferDtos(offers);
    }

    public async Task<List<ProductOfferDto>> GetOffersByProductAsync(int productId)
    {
      var offers = await _productOfferRepository.GetByProductIdAsync(productId);
      return MapToProductOfferDtos(offers);
    }

    public async Task<List<ProductOfferDto>> GetActiveOffersAsync()
    {
      var offers = await _productOfferRepository.GetActiveOffersAsync();
      return MapToProductOfferDtos(offers);
    }

    public async Task<List<ProductOfferDto>> GetActiveOffersByRestaurantAsync(int restaurantId)
    {
      var offers = await _productOfferRepository.GetActiveOffersByRestaurantAsync(restaurantId);
      return MapToProductOfferDtos(offers);
    }

    public async Task<ProductOfferDto> CreateOfferAsync(int restaurantId, CreateProductOfferDto createDto)
    {
      // Validar que el restaurante existe
      var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
      if (restaurant == null)
        throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

      // Validar que el producto existe
      var product = await _productRepository.GetByIdAsync(createDto.ProductId);
      if (product == null)
        throw new KeyNotFoundException($"Product with ID {createDto.ProductId} not found");

      // Validar que el producto esté asignado al restaurante
      var restaurantProduct = await _restaurantProductRepository.GetByRestaurantAndProductAsync(restaurantId, createDto.ProductId);
      if (restaurantProduct == null)
        throw new ArgumentException("Product is not available in this restaurant");

      // Validar fechas
      if (createDto.StartDate >= createDto.EndDate)
        throw new ArgumentException("Start date must be before end date");

      if (createDto.EndDate < DateTime.UtcNow)
        throw new ArgumentException("End date cannot be in the past");

      // Validar descuento
      if (createDto.DiscountValue <= 0)
        throw new ArgumentException("Discount value must be greater than 0");

      if (createDto.DiscountType == "percentage" && createDto.DiscountValue > 100)
        throw new ArgumentException("Percentage discount cannot be greater than 100%");

      var productOffer = new ProductOffer
      {
        Name = createDto.Name,
        Description = createDto.Description,
        DiscountType = createDto.DiscountType,
        DiscountValue = createDto.DiscountValue,
        MinimumOrderAmount = createDto.MinimumOrderAmount,
        MinimumQuantity = createDto.MinimumQuantity,
        StartDate = createDto.StartDate,
        EndDate = createDto.EndDate,
        UsageLimit = createDto.UsageLimit,
        RestaurantId = restaurantId,
        ProductId = createDto.ProductId,
        Status = "active"
      };

      var createdOffer = await _productOfferRepository.CreateAsync(productOffer);
      _logger.LogInformation("Product offer created: {OfferId} for restaurant {RestaurantId}", createdOffer.Id, restaurantId);

      return MapToProductOfferDto(createdOffer);
    }

    public async Task<ProductOfferDto?> UpdateOfferAsync(int restaurantId, int offerId, UpdateProductOfferDto updateDto)
    {
      var offer = await _productOfferRepository.GetByIdAsync(offerId);
      if (offer == null || offer.RestaurantId != restaurantId)
        return null;

      // Actualizar campos si se proporcionan
      if (!string.IsNullOrEmpty(updateDto.Name))
        offer.Name = updateDto.Name;

      if (!string.IsNullOrEmpty(updateDto.Description))
        offer.Description = updateDto.Description;

      if (!string.IsNullOrEmpty(updateDto.DiscountType))
      {
        if (updateDto.DiscountType != "percentage" && updateDto.DiscountType != "fixed")
          throw new ArgumentException("Discount type must be 'percentage' or 'fixed'");
        offer.DiscountType = updateDto.DiscountType;
      }

      if (updateDto.DiscountValue.HasValue)
      {
        if (updateDto.DiscountValue.Value <= 0)
          throw new ArgumentException("Discount value must be greater than 0");
        if (offer.DiscountType == "percentage" && updateDto.DiscountValue.Value > 100)
          throw new ArgumentException("Percentage discount cannot be greater than 100%");
        offer.DiscountValue = updateDto.DiscountValue.Value;
      }

      if (updateDto.MinimumOrderAmount.HasValue)
        offer.MinimumOrderAmount = updateDto.MinimumOrderAmount.Value;

      if (updateDto.MinimumQuantity.HasValue)
        offer.MinimumQuantity = updateDto.MinimumQuantity.Value;

      if (updateDto.StartDate.HasValue)
        offer.StartDate = updateDto.StartDate.Value;

      if (updateDto.EndDate.HasValue)
      {
        if (updateDto.EndDate.Value < DateTime.UtcNow)
          throw new ArgumentException("End date cannot be in the past");
        offer.EndDate = updateDto.EndDate.Value;
      }

      if (updateDto.UsageLimit.HasValue)
        offer.UsageLimit = updateDto.UsageLimit.Value;

      if (!string.IsNullOrEmpty(updateDto.Status))
      {
        if (updateDto.Status != "active" && updateDto.Status != "inactive")
          throw new ArgumentException("Status must be 'active' or 'inactive'");
        offer.Status = updateDto.Status;
      }

      // Validar fechas después de actualizar
      if (offer.StartDate >= offer.EndDate)
        throw new ArgumentException("Start date must be before end date");

      var updatedOffer = await _productOfferRepository.UpdateAsync(offer);
      _logger.LogInformation("Product offer updated: {OfferId}", offerId);

      return MapToProductOfferDto(updatedOffer);
    }

    public async Task<bool> DeleteOfferAsync(int restaurantId, int offerId)
    {
      var offer = await _productOfferRepository.GetByIdAsync(offerId);
      if (offer == null || offer.RestaurantId != restaurantId)
        return false;

      var deleted = await _productOfferRepository.DeleteAsync(offerId);
      if (deleted)
        _logger.LogInformation("Product offer deleted: {OfferId}", offerId);

      return deleted;
    }

    public async Task<bool> ActivateOfferAsync(int restaurantId, int offerId)
    {
      var offer = await _productOfferRepository.GetByIdAsync(offerId);
      if (offer == null || offer.RestaurantId != restaurantId)
        return false;

      offer.Status = "active";
      await _productOfferRepository.UpdateAsync(offer);

      _logger.LogInformation("Product offer activated: {OfferId}", offerId);
      return true;
    }

    public async Task<bool> DeactivateOfferAsync(int restaurantId, int offerId)
    {
      var offer = await _productOfferRepository.GetByIdAsync(offerId);
      if (offer == null || offer.RestaurantId != restaurantId)
        return false;

      offer.Status = "inactive";
      await _productOfferRepository.UpdateAsync(offer);

      _logger.LogInformation("Product offer deactivated: {OfferId}", offerId);
      return true;
    }

    public async Task<List<OfferValidationDto>> ValidateOffersForOrder(int restaurantId, List<(int productId, int quantity)> orderItems, decimal orderSubtotal)
    {
      var validations = new List<OfferValidationDto>();
      var activeOffers = await _productOfferRepository.GetActiveOffersByRestaurantAsync(restaurantId);

      foreach (var item in orderItems)
      {
        var productOffers = activeOffers.Where(o => o.ProductId == item.productId).ToList();

        foreach (var offer in productOffers)
        {
          var validation = new OfferValidationDto
          {
            OfferId = offer.Id,
            ProductId = item.productId,
            Quantity = item.quantity
          };

          // Validar condiciones
          if (item.quantity < offer.MinimumQuantity)
          {
            validation.CanApply = false;
            validation.ValidationMessage = $"Minimum quantity required: {offer.MinimumQuantity}";
          }
          else if (orderSubtotal < offer.MinimumOrderAmount)
          {
            validation.CanApply = false;
            validation.ValidationMessage = $"Minimum order amount required: ${offer.MinimumOrderAmount}";
          }
          else if (!offer.IsActive())
          {
            validation.CanApply = false;
            validation.ValidationMessage = "Offer is not active or has expired";
          }
          else
          {
            validation.CanApply = true;
            validation.ValidationMessage = "Offer can be applied";
          }

          validations.Add(validation);
        }
      }

      return validations;
    }

    public async Task<List<ProductOfferSummaryDto>> CalculateOffersForProducts(int restaurantId, List<(int productId, int quantity, decimal unitPrice)> products, decimal orderSubtotal)
    {
      var offerSummaries = new List<ProductOfferSummaryDto>();
      var activeOffers = await _productOfferRepository.GetActiveOffersByRestaurantAsync(restaurantId);

      foreach (var product in products)
      {
        var productOffers = activeOffers.Where(o => o.ProductId == product.productId).ToList();

        foreach (var offer in productOffers)
        {
          var summary = new ProductOfferSummaryDto
          {
            OfferId = offer.Id,
            Name = offer.Name,
            DiscountType = offer.DiscountType,
            DiscountValue = offer.DiscountValue,
            OriginalPrice = product.unitPrice
          };

          // Validar si se puede aplicar
          if (product.quantity >= offer.MinimumQuantity &&
              orderSubtotal >= offer.MinimumOrderAmount &&
              offer.IsActive())
          {
            summary.Applied = true;
            summary.CalculatedDiscount = offer.CalculateDiscount(product.unitPrice, product.quantity);
            summary.FinalPrice = product.unitPrice - summary.CalculatedDiscount;
          }
          else
          {
            summary.Applied = false;
            summary.CalculatedDiscount = 0;
            summary.FinalPrice = product.unitPrice;

            if (product.quantity < offer.MinimumQuantity)
              summary.ReasonNotApplied = $"Minimum quantity: {offer.MinimumQuantity}";
            else if (orderSubtotal < offer.MinimumOrderAmount)
              summary.ReasonNotApplied = $"Minimum order: ${offer.MinimumOrderAmount}";
            else
              summary.ReasonNotApplied = "Offer not active";
          }

          offerSummaries.Add(summary);
        }
      }

      return offerSummaries;
    }

    public async Task<bool> ApplyOfferUsage(int offerId)
    {
      return await _productOfferRepository.IncrementUsageCountAsync(offerId);
    }

    // Métodos auxiliares de mapeo
    private ProductOfferDto MapToProductOfferDto(ProductOffer offer)
    {
      return new ProductOfferDto
      {
        Id = offer.Id,
        Name = offer.Name,
        Description = offer.Description,
        DiscountType = offer.DiscountType,
        DiscountValue = offer.DiscountValue,
        MinimumOrderAmount = offer.MinimumOrderAmount,
        MinimumQuantity = offer.MinimumQuantity,
        StartDate = offer.StartDate,
        EndDate = offer.EndDate,
        UsageLimit = offer.UsageLimit,
        UsageCount = offer.UsageCount,
        Status = offer.Status,
        RestaurantId = offer.RestaurantId,
        RestaurantName = offer.Restaurant?.Name ?? "",
        ProductId = offer.ProductId,
        ProductName = offer.Product?.Name ?? "",
        ProductImageUrl = offer.Product?.ImageUrl ?? "",
        CreatedAt = offer.CreatedAt,
        UpdatedAt = offer.UpdatedAt,
        IsActive = offer.IsActive(),
        IsExpired = offer.EndDate < DateTime.UtcNow,
        RemainingUses = offer.UsageLimit > 0 ? Math.Max(0, offer.UsageLimit - offer.UsageCount) : -1
      };
    }

    private List<ProductOfferDto> MapToProductOfferDtos(List<ProductOffer> offers)
    {
      return offers.Select(MapToProductOfferDto).ToList();
    }
  }
}
