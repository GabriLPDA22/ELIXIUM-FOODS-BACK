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
      // Verificar que el usuario tiene permiso para crear productos en esta categoría/restaurante
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
      // Verificar que el producto existe
      var existingProduct = await _productService.GetProductByIdAsync(id);
      if (existingProduct == null)
      {
        return NotFound();
      }

      // Verificar que el usuario tiene permiso para actualizar este producto
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
      // Verificar que el producto existe
      var existingProduct = await _productService.GetProductByIdAsync(id);
      if (existingProduct == null)
      {
        return NotFound();
      }

      // Verificar que el usuario tiene permiso para eliminar este producto
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

    // Método auxiliar para verificar permisos
    private async Task<bool> CanManageProduct(int categoryId)
    {
      // Administradores siempre pueden gestionar productos
      if (User.IsInRole("Admin"))
      {
        return true;
      }

      // Para usuarios de tipo restaurante, verificar si la categoría pertenece a uno de sus restaurantes
      if (User.IsInRole("Restaurant"))
      {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        // Obtener los restaurantes del usuario
        var restaurants = await _restaurantService.GetRestaurantsByOwnerAsync(userId);
        
        // Verificar si la categoría pertenece a alguno de sus restaurantes
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