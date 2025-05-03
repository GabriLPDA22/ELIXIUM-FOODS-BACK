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

        // GET: api/restaurants
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetAllRestaurants()
        {
            var restaurants = await _context.Restaurants
                .Include(r => r.Address)
                .Include(r => r.Owner)
                .OrderBy(r => r.Name)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<RestaurantDto>>(restaurants));
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

        // GET: api/restaurants/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RestaurantDto>> GetRestaurant(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Address)
                .Include(r => r.Owner)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<RestaurantDto>(restaurant));
        }

        // PUT: api/restaurants/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestaurant(int id, UpdateRestaurantDto updateRestaurantDto)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);

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

            _mapper.Map(updateRestaurantDto, restaurant);

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
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Verificar si el usuario ya tiene un restaurante
            var existingRestaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.UserId == userId);
            if (existingRestaurant != null)
            {
                return BadRequest("El usuario ya tiene un restaurante registrado");
            }

            var address = _mapper.Map<Address>(createRestaurantDto.Address);
            address.UserId = userId;

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            var restaurant = _mapper.Map<Restaurant>(createRestaurantDto);
            restaurant.UserId = userId;
            restaurant.AddressId = address.Id;

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var restaurantDto = _mapper.Map<RestaurantDto>(restaurant);
            return CreatedAtAction(nameof(GetRestaurant), new { id = restaurant.Id }, restaurantDto);
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
