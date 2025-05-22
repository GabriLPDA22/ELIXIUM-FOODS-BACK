using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
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

    public RestaurantProductsController(
        IRestaurantProductService restaurantProductService,
        IBusinessService businessService)
    {
      _restaurantProductService = restaurantProductService;
      _businessService = businessService;
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
    }

    // GET: api/restaurants/5/products/10
    [HttpGet("{productId}")]
    [AllowAnonymous]
    public async Task<ActionResult<RestaurantProductDto>> GetRestaurantProduct(int restaurantId, int productId)
    {
      var product = await _restaurantProductService.GetRestaurantProductAsync(restaurantId, productId);
      if (product == null)
        return NotFound();

      return Ok(product);
    }

    // POST: api/restaurants/5/products
    [HttpPost]
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<RestaurantProductDto>> AssignProductToRestaurant(
        int restaurantId,
        CreateRestaurantProductDto createDto)
    {
      if (!await IsAuthorizedForRestaurant(restaurantId))
        return Forbid();

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
        return Forbid();

      var updatedProduct = await _restaurantProductService.UpdateRestaurantProductAsync(restaurantId, productId, updateDto);
      if (updatedProduct == null)
        return NotFound();

      return Ok(updatedProduct);
    }

    // DELETE: api/restaurants/5/products/10
    [HttpDelete("{productId}")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<IActionResult> RemoveProductFromRestaurant(int restaurantId, int productId)
    {
      if (!await IsAuthorizedForRestaurant(restaurantId))
        return Forbid();

      var result = await _restaurantProductService.RemoveProductFromRestaurantAsync(restaurantId, productId);
      if (!result)
        return NotFound();

      return NoContent();
    }

    // POST: api/restaurants/5/products/bulk
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<List<RestaurantProductDto>>> BulkAssignProducts(
        int restaurantId,
        BulkAssignProductsDto bulkDto)
    {
      if (!await IsAuthorizedForRestaurant(restaurantId))
        return Forbid();

      try
      {
        var assignedProducts = await _restaurantProductService.BulkAssignProductsAsync(restaurantId, bulkDto.Products);
        return Ok(assignedProducts);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
    }

    private async Task<bool> IsAuthorizedForRestaurant(int restaurantId)
    {
      var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userRole = User.FindFirstValue(ClaimTypes.Role);

      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        return false;

      if (userRole == "Admin")
        return true;

      // For Business users, check if they own the restaurant through business
      // This would need additional logic to get businessId from restaurantId
      return false; // Simplified for now
    }
  }
}
