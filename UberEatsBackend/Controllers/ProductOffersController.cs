using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UberEatsBackend.DTOs.Offers;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/restaurants/{restaurantId}/offers")]
  [Authorize]
  public class ProductOffersController : ControllerBase
  {
    private readonly IProductOfferService _productOfferService;
    private readonly ILogger<ProductOffersController> _logger;

    public ProductOffersController(
        IProductOfferService productOfferService,
        ILogger<ProductOffersController> logger)
    {
      _productOfferService = productOfferService;
      _logger = logger;
    }

    /// <summary>
    /// Obtener todas las ofertas de un restaurante
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductOfferDto>>> GetOffersByRestaurant(int restaurantId)
    {
      try
      {
        var offers = await _productOfferService.GetOffersByRestaurantAsync(restaurantId);
        return Ok(offers);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting offers for restaurant {RestaurantId}", restaurantId);
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }

    /// <summary>
    /// Obtener solo las ofertas activas de un restaurante
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous] // Permitir acceso público para mostrar ofertas en el menú
    public async Task<ActionResult<List<ProductOfferDto>>> GetActiveOffersByRestaurant(int restaurantId)
    {
      try
      {
        var offers = await _productOfferService.GetActiveOffersByRestaurantAsync(restaurantId);
        return Ok(offers);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting active offers for restaurant {RestaurantId}", restaurantId);
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }

    /// <summary>
    /// Obtener una oferta específica
    /// </summary>
    [HttpGet("{offerId}")]
    public async Task<ActionResult<ProductOfferDto>> GetOfferById(int restaurantId, int offerId)
    {
      try
      {
        var offer = await _productOfferService.GetOfferByIdAsync(offerId);
        if (offer == null || offer.RestaurantId != restaurantId)
        {
          return NotFound(new { message = "Oferta no encontrada" });
        }

        return Ok(offer);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting offer {OfferId} for restaurant {RestaurantId}", offerId, restaurantId);
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }

    /// <summary>
    /// Crear una nueva oferta
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Business,Restaurant")]
    public async Task<ActionResult<ProductOfferDto>> CreateOffer(int restaurantId, [FromBody] CreateProductOfferDto createDto)
    {
      try
      {
        if (!ModelState.IsValid)
        {
          return BadRequest(ModelState);
        }

        var offer = await _productOfferService.CreateOfferAsync(restaurantId, createDto);
        return CreatedAtAction(nameof(GetOfferById), new { restaurantId, offerId = offer.Id }, offer);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(new { message = ex.Message });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating offer for restaurant {RestaurantId}", restaurantId);
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }

    /// <summary>
    /// Actualizar una oferta existente
    /// </summary>
    [HttpPut("{offerId}")]
    [Authorize(Roles = "Admin,Business,Restaurant")]
    public async Task<ActionResult<ProductOfferDto>> UpdateOffer(int restaurantId, int offerId, [FromBody] UpdateProductOfferDto updateDto)
    {
      try
      {
        if (!ModelState.IsValid)
        {
          return BadRequest(ModelState);
        }

        var offer = await _productOfferService.UpdateOfferAsync(restaurantId, offerId, updateDto);
        if (offer == null)
        {
          return NotFound(new { message = "Oferta no encontrada" });
        }

        return Ok(offer);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating offer {OfferId} for restaurant {RestaurantId}", offerId, restaurantId);
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }

    /// <summary>
    /// Eliminar una oferta
    /// </summary>
    [HttpDelete("{offerId}")]
    [Authorize(Roles = "Admin,Business,Restaurant")]
    public async Task<ActionResult> DeleteOffer(int restaurantId, int offerId)
    {
      try
      {
        var deleted = await _productOfferService.DeleteOfferAsync(restaurantId, offerId);
        if (!deleted)
        {
          return NotFound(new { message = "Oferta no encontrada" });
        }

        return NoContent();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting offer {OfferId} for restaurant {RestaurantId}", offerId, restaurantId);
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }

    /// <summary>
    /// Activar una oferta
    /// </summary>
    [HttpPost("{offerId}/activate")]
    [Authorize(Roles = "Admin,Business,Restaurant")]
    public async Task<ActionResult> ActivateOffer(int restaurantId, int offerId)
    {
      try
      {
        var activated = await _productOfferService.ActivateOfferAsync(restaurantId, offerId);
        if (!activated)
        {
          return NotFound(new { message = "Oferta no encontrada" });
        }

        return Ok(new { message = "Oferta activada exitosamente" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error activating offer {OfferId} for restaurant {RestaurantId}", offerId, restaurantId);
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }

    /// <summary>
    /// Desactivar una oferta
    /// </summary>
    [HttpPost("{offerId}/deactivate")]
    [Authorize(Roles = "Admin,Business,Restaurant")]
    public async Task<ActionResult> DeactivateOffer(int restaurantId, int offerId)
    {
      try
      {
        var deactivated = await _productOfferService.DeactivateOfferAsync(restaurantId, offerId);
        if (!deactivated)
        {
          return NotFound(new { message = "Oferta no encontrada" });
        }

        return Ok(new { message = "Oferta desactivada exitosamente" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deactivating offer {OfferId} for restaurant {RestaurantId}", offerId, restaurantId);
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }

    /// <summary>
    /// Validar ofertas para un pedido (sin crearlo)
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<List<OfferValidationDto>>> ValidateOffersForOrder(
        int restaurantId,
        [FromBody] ValidateOffersRequestDto request)
    {
      try
      {
        var orderItems = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        var validations = await _productOfferService.ValidateOffersForOrder(restaurantId, orderItems, request.OrderSubtotal);
        return Ok(validations);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error validating offers for restaurant {RestaurantId}", restaurantId);
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }

    /// <summary>
    /// Calcular descuentos aplicables para productos específicos
    /// </summary>
    [HttpPost("calculate")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProductOfferSummaryDto>>> CalculateOffersForProducts(
        int restaurantId,
        [FromBody] CalculateOffersRequestDto request)
    {
      try
      {
        var products = request.Products.Select(p => (p.ProductId, p.Quantity, p.UnitPrice)).ToList();
        var calculations = await _productOfferService.CalculateOffersForProducts(restaurantId, products, request.OrderSubtotal);
        return Ok(calculations);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error calculating offers for restaurant {RestaurantId}", restaurantId);
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }
  }
}
