using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Product;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ProductsController : ControllerBase
  {
    private readonly IProductService _productService;
    private readonly IBusinessService _businessService;

    public ProductsController(IProductService productService, IBusinessService businessService)
    {
      _productService = productService;
      _businessService = businessService;
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
        return NotFound();

      return Ok(product);
    }

    // GET: api/Products/Business/5
    [HttpGet("Business/{businessId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByBusiness(int businessId)
    {
      var products = await _productService.GetProductsByBusinessIdAsync(businessId);
      return Ok(products);
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
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
      // Verify authorization for the business that owns the category
      if (!await IsAuthorizedForProductCategory(createProductDto.CategoryId))
        return Forbid();

      try
      {
        var createdProduct = await _productService.CreateProductAsync(createProductDto);
        return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(ex.Message);
      }
    }

    // PUT: api/Products/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
      var existingProduct = await _productService.GetProductByIdAsync(id);
      if (existingProduct == null)
        return NotFound();

      // Verify authorization
      if (!await IsAuthorizedForBusiness(existingProduct.BusinessId))
        return Forbid();

      var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDto);
      if (updatedProduct == null)
        return NotFound();

      return Ok(updatedProduct);
    }

    // DELETE: api/Products/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
      var existingProduct = await _productService.GetProductByIdAsync(id);
      if (existingProduct == null)
        return NotFound();

      // Verify authorization
      if (!await IsAuthorizedForBusiness(existingProduct.BusinessId))
        return Forbid();

      var result = await _productService.DeleteProductAsync(id);
      if (!result)
        return NotFound();

      return NoContent();
    }

    private async Task<bool> IsAuthorizedForBusiness(int businessId)
    {
      var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userRole = User.FindFirstValue(ClaimTypes.Role);

      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        return false;

      return await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole ?? "");
    }

    private async Task<bool> IsAuthorizedForProductCategory(int categoryId)
    {
      var userRole = User.FindFirstValue(ClaimTypes.Role);
      return userRole == "Admin" || userRole == "Business";
    }
  }
}
