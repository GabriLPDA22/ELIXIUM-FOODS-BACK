using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
  [Authorize]
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

    [HttpGet("tipo/{tipo}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantCardDto>>> GetRestaurantsByTipo(int tipo)
    {
      if (tipo < 1 || tipo > 8)
      {
        return BadRequest("El tipo debe estar entre 1 y 8");
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
          .Include(r => r.Business) // Incluimos el Business en lugar del Owner
          .FirstOrDefaultAsync(r => r.Id == id);

      if (restaurant == null)
      {
        return NotFound();
      }

      return Ok(_mapper.Map<RestaurantDto>(restaurant));
    }

    // NUEVO ENDPOINT: GET: api/restaurants/business/{businessId}
    [HttpGet("business/{businessId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetRestaurantsByBusiness(int businessId)
    {
      var restaurants = await _restaurantService.GetRestaurantsByBusinessIdAsync(businessId);

      if (restaurants == null || !restaurants.Any())
      {
        return NotFound($"No se encontraron restaurantes para el negocio con ID {businessId}");
      }

      return Ok(_mapper.Map<IEnumerable<RestaurantDto>>(restaurants));
    }

    // PUT: api/restaurants/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRestaurant(int id, UpdateRestaurantDto updateRestaurantDto)
    {
      var restaurant = await _context.Restaurants
          .Include(r => r.Business) // Incluir Business para verificar permisos
          .FirstOrDefaultAsync(r => r.Id == id);

      if (restaurant == null)
      {
        return NotFound();
      }

      // Verificar autorización
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

      if (!await _restaurantService.IsUserAuthorizedForRestaurant(id, userId, userRole))
      {
        return Forbid();
      }

      // Si hay un cambio en BusinessId, verificar que el usuario tenga permiso
      if (updateRestaurantDto.BusinessId.HasValue &&
          updateRestaurantDto.BusinessId != restaurant.BusinessId &&
          userRole != "Admin")
      {
        // Verificar que el usuario es dueño del negocio al que quiere asociar el restaurante
        var isBusiness = await _context.Businesses
            .AnyAsync(b => b.Id == updateRestaurantDto.BusinessId && b.UserId == userId);

        if (!isBusiness)
        {
          return Forbid("No tienes permiso para asociar este restaurante a ese negocio");
        }
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
    [Authorize(Roles = "Restaurant,Admin")]
    public async Task<ActionResult<RestaurantDto>> CreateRestaurant(CreateRestaurantDto createRestaurantDto)
    {
      try
      {
        // 1. Extraer el ID del usuario del token (este es el admin o el usuario que está creando el restaurante)
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int currentUserId))
        {
          return BadRequest("No se pudo identificar el ID de usuario del token");
        }

        // 2. Verificar rol del usuario
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var isAdmin = userRole == "Admin";

        // 3. Determinar el UserId correcto para el restaurante
        int ownerId;

        // Si es Admin y hay un BusinessId, debemos usar el propietario del Business
        if (isAdmin && createRestaurantDto.BusinessId.HasValue)
        {
          var business = await _context.Businesses.FindAsync(createRestaurantDto.BusinessId.Value);
          if (business == null)
          {
            return BadRequest($"El negocio con ID {createRestaurantDto.BusinessId.Value} no existe");
          }

          // Usar el propietario del negocio como propietario del restaurante
          ownerId = business.UserId;
          Console.WriteLine($"Admin creando restaurante para business ID {createRestaurantDto.BusinessId.Value}, propietario ID: {ownerId}");
        }
        else
        {
          // Si no es admin o no hay BusinessId, usa el usuario actual
          ownerId = currentUserId;

          // Verificar si el usuario ya tiene un restaurante (si no está asociado a un negocio)
          if (createRestaurantDto.BusinessId == null)
          {
            var existingRestaurant = await _context.Restaurants
                .AnyAsync(r => r.UserId == ownerId && r.BusinessId == null);

            if (existingRestaurant)
            {
              return BadRequest("El usuario ya tiene un restaurante registrado sin asociación a un negocio");
            }
          }

          // Verificar permisos para asociar a un negocio
          if (createRestaurantDto.BusinessId.HasValue && !isAdmin)
          {
            var isBusiness = await _context.Businesses
                .AnyAsync(b => b.Id == createRestaurantDto.BusinessId && b.UserId == ownerId);

            if (!isBusiness)
            {
              return Forbid("No tienes permiso para crear un restaurante asociado a este negocio");
            }
          }
        }

        // 4. Verificar que el propietario exista
        var ownerExists = await _context.Users.AnyAsync(u => u.Id == ownerId);
        if (!ownerExists)
        {
          return BadRequest($"El propietario con ID {ownerId} no existe en la base de datos");
        }

        // 5. Obtener datos del propietario
        var owner = await _context.Users.FindAsync(ownerId);
        Console.WriteLine($"Propietario del restaurante: {owner.Email}, {owner.FirstName} {owner.LastName}");

        // 6. Crear dirección y restaurante en una transacción
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
          // Crear la dirección
          var address = new Address
          {
            Name = "Dirección del Restaurante",
            Street = createRestaurantDto.Address.Street,
            City = createRestaurantDto.Address.City,
            State = createRestaurantDto.Address.State,
            ZipCode = createRestaurantDto.Address.ZipCode,
            Latitude = createRestaurantDto.Address.Latitude,
            Longitude = createRestaurantDto.Address.Longitude,
            UserId = ownerId,  // Usar el ID del propietario, no el del admin
            IsDefault = false
          };

          // Mapear propiedades adicionales si existen
          var addressDto = createRestaurantDto.Address;

          // Comprobar propiedades comunes que podrían existir
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

          // Añadir dirección
          _context.Addresses.Add(address);
          await _context.SaveChangesAsync();

          // Crear restaurante asociado al propietario del negocio, no al admin
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
            UserId = ownerId,  // Establecer el propietario del negocio como propietario del restaurante
            AddressId = address.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
          };

          _context.Restaurants.Add(restaurant);
          await _context.SaveChangesAsync();

          await transaction.CommitAsync();
          Console.WriteLine("Restaurante creado exitosamente con propietario correcto");

          var restaurantDto = _mapper.Map<RestaurantDto>(restaurant);
          return CreatedAtAction(nameof(GetRestaurant), new { id = restaurant.Id }, restaurantDto);
        }
        catch (Exception ex)
        {
          await transaction.RollbackAsync();
          Console.WriteLine($"Error al crear restaurante: {ex.Message}");

          if (ex.InnerException != null)
          {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
          }

          throw;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error general: {ex.Message}");

        if (ex.InnerException != null)
        {
          var innerMsg = ex.InnerException.Message;

          if (innerMsg.Contains("23503"))
          {
            return StatusCode(500, $"Error de clave foránea. Por favor, verifica que todos los IDs relacionados existan en la base de datos.");
          }

          return StatusCode(500, $"Error interno: {innerMsg}");
        }

        return StatusCode(500, $"Error interno: {ex.Message}");
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
