using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Product;
using UberEatsBackend.Services;
using System.Linq;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ProductsController : ControllerBase
  {
    private readonly IProductService _productService;
    private readonly IRestaurantService _restaurantService;

    public ProductsController(IProductService productService, IRestaurantService restaurantService)
    {
      _productService = productService;
      _restaurantService = restaurantService;
    }

    // GET: api/Products
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
      var products = await _productService.GetAllProductsAsync();
      return Ok(products);
    }

    // GET: api/Products/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
      var product = await _productService.GetProductByIdAsync(id);

      if (product == null)
      {
        return NotFound();
      }

      return Ok(product);
    }

    // GET: api/Products/Restaurant/5
    [HttpGet("Restaurant/{restaurantId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByRestaurant(int restaurantId)
    {
      var products = await _productService.GetProductsByRestaurantIdAsync(restaurantId);
      return Ok(products);
    }

    // GET: api/Products/Category/5
    [HttpGet("Category/{categoryId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
    {
      var products = await _productService.GetProductsByCategoryIdAsync(categoryId);
      return Ok(products);
    }

    // POST: api/Products
    [HttpPost]
    [Authorize(Roles = "Restaurant,Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
      // Verify that the user has permission to create products in this category
      if (!await CanManageProduct(createProductDto.CategoryId))
      {
        return Forbid();
      }

      var createdProduct = await _productService.CreateProductAsync(createProductDto);
      return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
    }

    // PUT: api/Products/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Restaurant,Admin")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
      // Verify that the product exists
      var existingProduct = await _productService.GetProductByIdAsync(id);
      if (existingProduct == null)
      {
        return NotFound();
      }

      // Verify that the user has permission to update this product
      if (!await CanManageProduct(existingProduct.CategoryId))
      {
        return Forbid();
      }

      var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDto);
      if (updatedProduct == null)
      {
        return NotFound();
      }

      return Ok(updatedProduct);
    }

    // DELETE: api/Products/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Restaurant,Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
      // Verify that the product exists
      var existingProduct = await _productService.GetProductByIdAsync(id);
      if (existingProduct == null)
      {
        return NotFound();
      }

      // Verify that the user has permission to delete this product
      if (!await CanManageProduct(existingProduct.CategoryId))
      {
        return Forbid();
      }

      var result = await _productService.DeleteProductAsync(id);
      if (!result)
      {
        return NotFound();
      }

      return NoContent();
    }

    // Helper method to verify permissions
    private async Task<bool> CanManageProduct(int categoryId)
    {
      // Administrators always can manage products
      if (User.IsInRole("Admin"))
      {
        return true;
      }

      // For restaurant users, check if the category belongs to one of their restaurants
      if (User.IsInRole("Restaurant"))
      {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Get all accessible restaurants for this user
        // Note: In the new architecture, we use GetRestaurantsForAdminAsync instead of GetRestaurantsByOwnerAsync
        var restaurants = await _restaurantService.GetRestaurantsForAdminAsync(userId);

        // Verify if the category belongs to any of these restaurants
        foreach (var restaurant in restaurants)
        {
          var products = await _productService.GetProductsByRestaurantIdAsync(restaurant.Id);
          if (products.Any(p => p.CategoryId == categoryId))
          {
            return true;
          }
        }
      }

      return false;
    }
  }
}
