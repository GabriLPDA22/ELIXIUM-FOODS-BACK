using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.DTOs.RestaurantProduct;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/restaurants/{restaurantId}/products")]
  public class RestaurantProductsController : ControllerBase
  {
    private readonly IRestaurantProductService _restaurantProductService;
    private readonly IBusinessService _businessService;
    private readonly ApplicationDbContext _context;

    public RestaurantProductsController(
        IRestaurantProductService restaurantProductService,
        IBusinessService businessService,
        ApplicationDbContext context)
    {
      _restaurantProductService = restaurantProductService;
      _businessService = businessService;
      _context = context;
    }

    // GET: api/restaurants/5/products
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<RestaurantProductDto>>> GetRestaurantProducts(int restaurantId)
    {
      try
      {
        var products = await _restaurantProductService.GetRestaurantProductsAsync(restaurantId);
        return Ok(products);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    // GET: api/restaurants/5/products/10
    [HttpGet("{productId}")]
    [AllowAnonymous]
    public async Task<ActionResult<RestaurantProductDto>> GetRestaurantProduct(int restaurantId, int productId)
    {
      try
      {
        var product = await _restaurantProductService.GetRestaurantProductAsync(restaurantId, productId);
        if (product == null)
          return NotFound($"Product with ID {productId} not found in restaurant with ID {restaurantId}");

        return Ok(product);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    // POST: api/restaurants/5/products
    [HttpPost]
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<RestaurantProductDto>> AssignProductToRestaurant(
        int restaurantId,
        CreateRestaurantProductDto createDto)
    {
      if (!await IsAuthorizedForRestaurant(restaurantId))
        return Forbid("You are not authorized to manage products for this restaurant");

      try
      {
        var assignedProduct = await _restaurantProductService.AssignProductToRestaurantAsync(restaurantId, createDto);
        return CreatedAtAction(nameof(GetRestaurantProduct),
            new { restaurantId, productId = createDto.ProductId },
            assignedProduct);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(ex.Message);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    // PUT: api/restaurants/5/products/10
    [HttpPut("{productId}")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<RestaurantProductDto>> UpdateRestaurantProduct(
        int restaurantId,
        int productId,
        UpdateRestaurantProductDto updateDto)
    {
      if (!await IsAuthorizedForRestaurant(restaurantId))
        return Forbid("You are not authorized to manage products for this restaurant");

      try
      {
        var updatedProduct = await _restaurantProductService.UpdateRestaurantProductAsync(restaurantId, productId, updateDto);
        if (updatedProduct == null)
          return NotFound($"Product with ID {productId} not found in restaurant with ID {restaurantId}");

        return Ok(updatedProduct);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    // DELETE: api/restaurants/5/products/10
    [HttpDelete("{productId}")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<IActionResult> RemoveProductFromRestaurant(int restaurantId, int productId)
    {
      if (!await IsAuthorizedForRestaurant(restaurantId))
        return Forbid("You are not authorized to manage products for this restaurant");

      try
      {
        var result = await _restaurantProductService.RemoveProductFromRestaurantAsync(restaurantId, productId);
        if (!result)
          return NotFound($"Product with ID {productId} not found in restaurant with ID {restaurantId}");

        return NoContent();
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    // POST: api/restaurants/5/products/bulk
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<List<RestaurantProductDto>>> BulkAssignProducts(
        int restaurantId,
        BulkAssignProductsDto bulkDto)
    {
      if (!await IsAuthorizedForRestaurant(restaurantId))
        return Forbid("You are not authorized to manage products for this restaurant");

      try
      {
        var assignedProducts = await _restaurantProductService.BulkAssignProductsAsync(restaurantId, bulkDto.Products);
        return Ok(assignedProducts);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    // Método privado para verificar autorización
    private async Task<bool> IsAuthorizedForRestaurant(int restaurantId)
    {
      var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userRole = User.FindFirstValue(ClaimTypes.Role);

      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        return false;

      // Los administradores pueden acceder a cualquier restaurante
      if (userRole == "Admin")
        return true;

      // Para usuarios Business, verificar que el restaurante pertenezca a su negocio
      if (userRole == "Business")
      {
        try
        {
          var restaurant = await _context.Restaurants
            .Include(r => r.Business)
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

          if (restaurant?.Business?.UserId == userId)
            return true;
        }
        catch (Exception)
        {
          return false;
        }
      }

      return false;
    }
  }
}
