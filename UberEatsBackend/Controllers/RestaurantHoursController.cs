// UberEatsBackend/Controllers/RestaurantHoursController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UberEatsBackend.DTOs.Restaurant;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    public class RestaurantHoursController : ControllerBase
    {
        private readonly IRestaurantHourService _restaurantHourService;
        private readonly IRestaurantService _restaurantService;

        public RestaurantHoursController(IRestaurantHourService restaurantHourService, IRestaurantService restaurantService)
        {
            _restaurantHourService = restaurantHourService;
            _restaurantService = restaurantService;
        }

        // ===== RUTAS ORIGINALES PARA RESTAURANTES =====

        [HttpGet("api/restaurants/{restaurantId}/hours")]
        [AllowAnonymous]
        public async Task<ActionResult<List<RestaurantHourDto>>> GetRestaurantHours(int restaurantId)
        {
            try
            {
                var hours = await _restaurantHourService.GetRestaurantHoursAsync(restaurantId);
                return Ok(hours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener horarios: {ex.Message}");
            }
        }

        [HttpPost("api/restaurants/{restaurantId}/hours")]
        [Authorize(Roles = "Admin,Business")]
        public async Task<ActionResult> UpdateRestaurantHours(int restaurantId, [FromBody] BulkUpdateRestaurantHoursDto hoursDto)
        {
            try
            {
                // Verificar autorización
                if (!await IsAuthorizedForRestaurant(restaurantId))
                {
                    return Forbid("No tienes permisos para modificar este restaurante");
                }

                var success = await _restaurantHourService.BulkUpdateRestaurantHoursAsync(restaurantId, hoursDto);

                if (success)
                {
                    return Ok(new { message = "Horarios actualizados correctamente" });
                }
                else
                {
                    return BadRequest("Error al actualizar los horarios");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar horarios: {ex.Message}");
            }
        }

        [HttpGet("api/restaurants/{restaurantId}/hours/status")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetRestaurantStatus(int restaurantId)
        {
            try
            {
                var isOpen = await _restaurantHourService.IsRestaurantOpenAsync(restaurantId);
                var status = await _restaurantHourService.GetRestaurantStatusAsync(restaurantId);

                return Ok(new
                {
                    restaurantId = restaurantId,
                    isOpen = isOpen,
                    status = status,
                    message = isOpen ? "El restaurante está abierto" : $"El restaurante está cerrado. {status}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener estado: {ex.Message}");
            }
        }

        [HttpGet("api/restaurants/{restaurantId}/hours/check")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> CheckRestaurantOpenAt(int restaurantId, [FromQuery] DateTime? dateTime = null)
        {
            try
            {
                var checkTime = dateTime ?? DateTime.Now;
                var isOpen = await _restaurantHourService.IsRestaurantOpenAtTimeAsync(restaurantId, checkTime);

                return Ok(new
                {
                    restaurantId = restaurantId,
                    checkTime = checkTime,
                    isOpen = isOpen,
                    message = isOpen ?
                        $"El restaurante estará abierto el {checkTime:dd/MM/yyyy} a las {checkTime:HH:mm}" :
                        $"El restaurante estará cerrado el {checkTime:dd/MM/yyyy} a las {checkTime:HH:mm}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al verificar horario: {ex.Message}");
            }
        }

        // ===== NUEVAS RUTAS PARA BUSINESS (LO QUE NECESITA TU FRONTEND) =====

        [HttpGet("api/Business/{businessId}/hours")]
        [Authorize(Roles = "Admin,Business")]
        public async Task<ActionResult<List<RestaurantHourDto>>> GetBusinessRestaurantHours(int businessId)
        {
            try
            {
                // Verificar autorización
                if (!await IsAuthorizedForBusiness(businessId))
                {
                    return Forbid("No tienes permisos para acceder a este negocio");
                }

                // Obtener el primer restaurante del negocio
                var restaurants = await _restaurantService.GetRestaurantsByBusinessIdAsync(businessId);
                var restaurant = restaurants.FirstOrDefault();

                if (restaurant == null)
                {
                    return NotFound("No se encontraron restaurantes para este negocio");
                }

                var hours = await _restaurantHourService.GetRestaurantHoursAsync(restaurant.Id);
                return Ok(hours);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener horarios: {ex.Message}");
            }
        }

        [HttpPost("api/Business/{businessId}/hours")]
        [Authorize(Roles = "Admin,Business")]
        public async Task<ActionResult> UpdateBusinessRestaurantHours(int businessId, [FromBody] BulkUpdateRestaurantHoursDto hoursDto)
        {
            try
            {
                // Verificar autorización
                if (!await IsAuthorizedForBusiness(businessId))
                {
                    return Forbid("No tienes permisos para modificar este negocio");
                }

                // Obtener todos los restaurantes del negocio
                var restaurants = await _restaurantService.GetRestaurantsByBusinessIdAsync(businessId);

                if (!restaurants.Any())
                {
                    return NotFound("No se encontraron restaurantes para este negocio");
                }

                // Actualizar horarios para todos los restaurantes del negocio
                bool allSuccess = true;
                var errors = new List<string>();

                foreach (var restaurant in restaurants)
                {
                    try
                    {
                        var success = await _restaurantHourService.BulkUpdateRestaurantHoursAsync(restaurant.Id, hoursDto);
                        if (!success)
                        {
                            allSuccess = false;
                            errors.Add($"Error al actualizar horarios del restaurante {restaurant.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        allSuccess = false;
                        errors.Add($"Error en restaurante {restaurant.Name}: {ex.Message}");
                    }
                }

                if (allSuccess)
                {
                    return Ok(new {
                        message = "Horarios actualizados correctamente para todos los restaurantes",
                        updatedRestaurants = restaurants.Count
                    });
                }
                else
                {
                    return BadRequest(new {
                        message = "Algunos restaurantes no pudieron actualizarse",
                        errors = errors
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar horarios: {ex.Message}");
            }
        }

        [HttpGet("api/Business/{businessId}/hours/status")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetBusinessRestaurantStatus(int businessId)
        {
            try
            {
                // Obtener el primer restaurante del negocio
                var restaurants = await _restaurantService.GetRestaurantsByBusinessIdAsync(businessId);
                var restaurant = restaurants.FirstOrDefault();

                if (restaurant == null)
                {
                    return NotFound("No se encontraron restaurantes para este negocio");
                }

                var isOpen = await _restaurantHourService.IsRestaurantOpenAsync(restaurant.Id);
                var status = await _restaurantHourService.GetRestaurantStatusAsync(restaurant.Id);

                return Ok(new
                {
                    businessId = businessId,
                    restaurantId = restaurant.Id,
                    restaurantName = restaurant.Name,
                    isOpen = isOpen,
                    status = status,
                    message = isOpen ? "El restaurante está abierto" : $"El restaurante está cerrado. {status}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener estado: {ex.Message}");
            }
        }

        // ===== MÉTODOS DE AUTORIZACIÓN =====

        private async Task<bool> IsAuthorizedForRestaurant(int restaurantId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return false;

            if (userRole == "Admin")
                return true;

            if (userRole == "Business")
            {
                return await _restaurantService.IsBusinessOwner(restaurantId, userId);
            }

            return false;
        }

        private async Task<bool> IsAuthorizedForBusiness(int businessId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return false;

            if (userRole == "Admin")
                return true;

            if (userRole == "Business")
            {
                return await _restaurantService.IsBusinessOwner(businessId, userId);
            }

            return false;
        }
    }
}
