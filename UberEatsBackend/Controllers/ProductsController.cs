using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Product;
using UberEatsBackend.DTOs.RestaurantProduct;
using UberEatsBackend.Services;
using System;
using UberEatsBackend.Repositories;
using UberEatsBackend.Models;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ProductsController : ControllerBase
  {
    private readonly IProductService _productService;
    private readonly IBusinessService _businessService;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRestaurantProductService _restaurantProductService;


    public ProductsController(
        IProductService productService,
        IBusinessService businessService,
        IRepository<Category> categoryRepository,
        IRestaurantProductService restaurantProductService)
    {
      _productService = productService;
      _businessService = businessService;
      _categoryRepository = categoryRepository;
      _restaurantProductService = restaurantProductService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
      var products = await _productService.GetAllProductsAsync();
      return Ok(products);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
      var product = await _productService.GetProductByIdAsync(id);
      if (product == null)
        return NotFound();

      return Ok(product);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] string query, [FromQuery] int? category = null)
    {
      try
      {
        var products = await _productService.SearchProductsAsync(query, category);
        return Ok(products);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("{productId}/restaurants")]
    [AllowAnonymous]
    public async Task<ActionResult<List<RestaurantProductOfferingDto>>> GetRestaurantsForProduct(int productId)
    {
        var offerings = await _restaurantProductService.GetRestaurantOfferingsForProductAsync(productId);
        if (offerings == null || !offerings.Any())
        {
            return NotFound("No restaurants found offering this product.");
        }
        return Ok(offerings);
    }

    [HttpGet("Business/{businessId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByBusiness(int businessId)
    {
      var products = await _productService.GetProductsByBusinessIdAsync(businessId);
      return Ok(products);
    }

    [HttpGet("Restaurant/{restaurantId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByRestaurant(int restaurantId)
    {
      var products = await _productService.GetProductsByRestaurantIdAsync(restaurantId);
      return Ok(products);
    }

    [HttpGet("Category/{categoryId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
    {
      var products = await _productService.GetProductsByCategoryIdAsync(categoryId);
      return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
      if (!await IsAuthorizedForProductCreation(createProductDto.BusinessId, createProductDto.CategoryId))
        return Forbid("User not authorized for this business or category does not belong to the business.");

      try
      {
        var createdProduct = await _productService.CreateProductAsync(createProductDto);
        return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
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

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
      var existingProduct = await _productService.GetProductByIdAsync(id);
      if (existingProduct == null)
        return NotFound();

      if (!await IsAuthorizedForBusiness(existingProduct.BusinessId))
        return Forbid();

      try
      {
        var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDto);
        if (updatedProduct == null)
          return NotFound();
        return Ok(updatedProduct);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
      var existingProduct = await _productService.GetProductByIdAsync(id);
      if (existingProduct == null)
        return NotFound();

      if (!await IsAuthorizedForBusiness(existingProduct.BusinessId))
        return Forbid();

      try
      {
        var result = await _productService.DeleteProductAsync(id);
        if (!result)
          return NotFound();
        return NoContent();
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    private async Task<bool> IsAuthorizedForBusiness(int businessId)
    {
      var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userRole = User.FindFirstValue(ClaimTypes.Role);

      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        return false;

      return await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole ?? "");
    }

    private async Task<bool> IsAuthorizedForProductCreation(int businessId, int categoryId)
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userRole == "Admin") return true;

        if (userRole == "Business")
        {
            if (!await IsAuthorizedForBusiness(businessId)) return false;

            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null || category.BusinessId != businessId) return false;
            return true;
        }
        return false;
    }
  }
}
