using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UberEatsBackend.DTOs.Offers;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/product-offers")]
  public class ActiveOffersController : ControllerBase
  {
    private readonly IProductOfferService _productOfferService;
    private readonly ILogger<ActiveOffersController> _logger;

    public ActiveOffersController(
        IProductOfferService productOfferService,
        ILogger<ActiveOffersController> logger)
    {
      _productOfferService = productOfferService;
      _logger = logger;
    }

    /// <summary>
    /// Obtener todas las ofertas activas del sistema
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProductOfferDto>>> GetActiveOffers()
    {
      try
      {
        var offers = await _productOfferService.GetActiveOffersAsync();
        return Ok(offers);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting active offers");
        return StatusCode(500, new { message = "Error interno del servidor" });
      }
    }
  }
}
