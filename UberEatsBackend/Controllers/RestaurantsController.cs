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

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetRestaurants()
    {
      var restaurants = await _restaurantService.GetAllRestaurantsAsync();
      return Ok(_mapper.Map<IEnumerable<RestaurantCardDto>>(restaurants));
    }

    [HttpGet("with-status")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantCardWithStatusDto>>> GetRestaurantsWithStatus()
    {
      try
      {
        var restaurantsWithStatus = await _restaurantService.GetAllRestaurantsWithStatusAsync();
        return Ok(restaurantsWithStatus);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Error al obtener restaurantes: {ex.Message}");
      }
    }

    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetPopularRestaurants([FromQuery] int limit = 10)
    {
      var restaurants = await _restaurantService.GetPopularRestaurantsAsync(limit);
      return Ok(_mapper.Map<IEnumerable<RestaurantCardDto>>(restaurants));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> SearchRestaurants([FromQuery] string query, [FromQuery] int? category = null)
    {
      var restaurants = await _restaurantService.SearchRestaurantsAsync(query, category);
      return Ok(_mapper.Map<IEnumerable<RestaurantCardDto>>(restaurants));
    }

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

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<RestaurantDto>> GetRestaurant(int id)
    {
      var restaurant = await _restaurantService.GetRestaurantWithDetailsAsync(id);

      if (restaurant == null)
      {
        return NotFound($"Restaurante con ID {id} no encontrado.");
      }

      return Ok(_mapper.Map<RestaurantDto>(restaurant));
    }

    [HttpGet("{id}/with-status")]
    [AllowAnonymous]
    public async Task<ActionResult<RestaurantDetailWithStatusDto>> GetRestaurantWithStatus(int id)
    {
      try
      {
        var restaurantWithStatus = await _restaurantService.GetRestaurantWithStatusAsync(id);
        if (restaurantWithStatus == null)
        {
          return NotFound($"Restaurante con ID {id} o su estado no fue encontrado.");
        }
        return Ok(restaurantWithStatus);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Error interno al obtener el estado del restaurante: {ex.Message}");
      }
    }

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
          var address = _mapper.Map<Address>(createRestaurantDto.Address);
          address.Name = "Restaurant Address";
          address.IsDefault = false;
          address.UserId = null;

          _context.Addresses.Add(address);
          await _context.SaveChangesAsync();

          var restaurant = _mapper.Map<Restaurant>(createRestaurantDto);
          restaurant.AddressId = address.Id;
          restaurant.CreatedAt = DateTime.UtcNow;
          restaurant.UpdatedAt = DateTime.UtcNow;

          _context.Restaurants.Add(restaurant);
          await _context.SaveChangesAsync();

          await transaction.CommitAsync();

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
          if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
          throw;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"General error: {ex.Message}");
        if (ex.InnerException != null)
        {
          var innerMsg = ex.InnerException.Message;
          if (innerMsg.Contains("23503")) return StatusCode(500, $"Foreign key error. Please verify that all related IDs exist in the database.");
          return StatusCode(500, $"Internal error: {innerMsg}");
        }
        return StatusCode(500, $"Internal error: {ex.Message}");
      }
    }

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
