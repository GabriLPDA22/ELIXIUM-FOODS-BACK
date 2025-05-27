using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.DTOs.Restaurant;
using UberEatsBackend.Models;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class RestaurantsController : ControllerBase
  {
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IRestaurantService _restaurantService;

    public RestaurantsController(ApplicationDbContext context, IMapper mapper, IRestaurantService restaurantService)
    {
      _context = context;
      _mapper = mapper;
      _restaurantService = restaurantService;
    }

    // GET: api/Restaurants
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetRestaurants()
    {
      var restaurants = await _restaurantService.GetAllRestaurantsAsync();
      return Ok(_mapper.Map<IEnumerable<RestaurantCardDto>>(restaurants));
    }

    // GET: api/restaurants/popular
    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetPopularRestaurants([FromQuery] int limit = 10)
    {
      var restaurants = await _restaurantService.GetPopularRestaurantsAsync(limit);
      return Ok(_mapper.Map<IEnumerable<RestaurantCardDto>>(restaurants));
    }

    // GET: api/restaurants/search
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> SearchRestaurants([FromQuery] string? query = null, [FromQuery] string? cuisine = null)
    {
      var restaurants = await _restaurantService.SearchRestaurantsAsync(query, cuisine);
      return Ok(_mapper.Map<IEnumerable<RestaurantCardDto>>(restaurants));
    }

    // GET: api/restaurants/tipo/{tipo}
    [HttpGet("tipo/{tipo}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetRestaurantsByTipo(int tipo)
    {
      if (tipo < 1 || tipo > 8)
      {
        return BadRequest("Type must be between 1 and 8");
      }

      var restaurants = await _restaurantService.GetRestaurantsByTipoAsync(tipo);
      return Ok(_mapper.Map<IEnumerable<RestaurantCardDto>>(restaurants));
    }

    // GET: api/restaurants/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<RestaurantDto>> GetRestaurant(int id)
    {
      var restaurant = await _context.Restaurants
          .Include(r => r.Address)
          .Include(r => r.Business)
          .FirstOrDefaultAsync(r => r.Id == id);

      if (restaurant == null)
      {
        return NotFound();
      }

      return Ok(_mapper.Map<RestaurantDto>(restaurant));
    }

    // GET: api/restaurants/business/{businessId}
    [HttpGet("business/{businessId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetRestaurantsByBusiness(int businessId)
    {
      var restaurants = await _restaurantService.GetRestaurantsByBusinessIdAsync(businessId);

      if (restaurants == null || !restaurants.Any())
      {
        return NotFound($"No restaurants found for business with ID {businessId}");
      }

      return Ok(_mapper.Map<IEnumerable<RestaurantDto>>(restaurants));
    }

    // PUT: api/restaurants/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRestaurant(int id, UpdateRestaurantDto updateRestaurantDto)
    {
      var restaurant = await _context.Restaurants
          .Include(r => r.Business)
          .FirstOrDefaultAsync(r => r.Id == id);

      if (restaurant == null)
      {
        return NotFound();
      }

      _mapper.Map(updateRestaurantDto, restaurant);
      restaurant.UpdatedAt = DateTime.UtcNow;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!RestaurantExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return NoContent();
    }

   // POST: api/restaurants
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<RestaurantDto>> CreateRestaurant(CreateRestaurantDto createRestaurantDto)
{
  try
  {
    if (createRestaurantDto.BusinessId.HasValue)
    {
      var business = await _context.Businesses.FindAsync(createRestaurantDto.BusinessId.Value);
      if (business == null)
      {
        return BadRequest($"Business with ID {createRestaurantDto.BusinessId.Value} does not exist");
      }
    }

    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
      // Crear la dirección del restaurante (sin UserId ya que es para el restaurante)
      var address = new Address
      {
        Name = "Restaurant Address",
        Street = createRestaurantDto.Address.Street,
        City = createRestaurantDto.Address.City,
        State = createRestaurantDto.Address.State,
        ZipCode = createRestaurantDto.Address.ZipCode,
        Latitude = createRestaurantDto.Address.Latitude,
        Longitude = createRestaurantDto.Address.Longitude,
        IsDefault = false,
        UserId = null // Direcciones de restaurantes no tienen UserId
      };

      // Mapear campos adicionales si existen en el DTO
      var addressDto = createRestaurantDto.Address;

      if (addressDto.GetType().GetProperty("Number") != null)
      {
        var prop = addressDto.GetType().GetProperty("Number");
        address.Number = prop.GetValue(addressDto) as string ?? string.Empty;
      }

      if (addressDto.GetType().GetProperty("Interior") != null)
      {
        var prop = addressDto.GetType().GetProperty("Interior");
        address.Interior = prop.GetValue(addressDto) as string ?? string.Empty;
      }

      if (addressDto.GetType().GetProperty("Neighborhood") != null)
      {
        var prop = addressDto.GetType().GetProperty("Neighborhood");
        address.Neighborhood = prop.GetValue(addressDto) as string ?? string.Empty;
      }

      if (addressDto.GetType().GetProperty("Phone") != null)
      {
        var prop = addressDto.GetType().GetProperty("Phone");
        address.Phone = prop.GetValue(addressDto) as string ?? string.Empty;
      }

      _context.Addresses.Add(address);
      await _context.SaveChangesAsync();

      // Crear el restaurante
      var restaurant = new Restaurant
      {
        Name = createRestaurantDto.Name,
        Description = createRestaurantDto.Description,
        LogoUrl = createRestaurantDto.LogoUrl,
        CoverImageUrl = createRestaurantDto.CoverImageUrl,
        IsOpen = createRestaurantDto.IsOpen,
        DeliveryFee = createRestaurantDto.DeliveryFee,
        EstimatedDeliveryTime = createRestaurantDto.EstimatedDeliveryTime,
        Tipo = createRestaurantDto.Tipo,
        BusinessId = createRestaurantDto.BusinessId,
        AddressId = address.Id,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      _context.Restaurants.Add(restaurant);
      await _context.SaveChangesAsync();

      await transaction.CommitAsync();
      Console.WriteLine("Restaurant created successfully");

      // Cargar el restaurante con sus relaciones para el DTO
      var createdRestaurant = await _context.Restaurants
        .Include(r => r.Address)
        .Include(r => r.Business)
        .FirstAsync(r => r.Id == restaurant.Id);

      var restaurantDto = _mapper.Map<RestaurantDto>(createdRestaurant);
      return CreatedAtAction(nameof(GetRestaurant), new { id = restaurant.Id }, restaurantDto);
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync();
      Console.WriteLine($"Error creating restaurant: {ex.Message}");

      if (ex.InnerException != null)
      {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
      }

      throw;
    }
  }
  catch (Exception ex)
  {
    Console.WriteLine($"General error: {ex.Message}");

    if (ex.InnerException != null)
    {
      var innerMsg = ex.InnerException.Message;

      if (innerMsg.Contains("23503"))
      {
        return StatusCode(500, $"Foreign key error. Please verify that all related IDs exist in the database.");
      }

      return StatusCode(500, $"Internal error: {innerMsg}");
    }

    return StatusCode(500, $"Internal error: {ex.Message}");
  }
}

    // DELETE: api/restaurants/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRestaurant(int id)
    {
      var restaurant = await _context.Restaurants.FindAsync(id);
      if (restaurant == null)
      {
        return NotFound();
      }

      _context.Restaurants.Remove(restaurant);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool RestaurantExists(int id)
    {
      return _context.Restaurants.Any(e => e.Id == id);
    }
  }
}
