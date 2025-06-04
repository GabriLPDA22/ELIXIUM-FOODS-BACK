// UberEatsBackend/Controllers/ReviewsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Review;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpGet("restaurant/{restaurantId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ReviewDto>>> GetRestaurantReviews(
            int restaurantId,
            [FromQuery] ReviewFilterDto? filter = null)
        {
            try
            {
                var reviews = await _reviewService.GetRestaurantReviewsAsync(restaurantId, filter);
                return Ok(new { Success = true, Data = reviews });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reseñas del restaurante {RestaurantId}", restaurantId);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ReviewDto>>> GetProductReviews(
            int productId,
            [FromQuery] ReviewFilterDto? filter = null)
        {
            try
            {
                var reviews = await _reviewService.GetProductReviewsAsync(productId, filter);
                return Ok(new { Success = true, Data = reviews });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reseñas del producto {ProductId}", productId);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<List<ReviewDto>>> GetUserReviews(
            int userId,
            [FromQuery] ReviewFilterDto? filter = null)
        {
            try
            {
                // Verificar autorización
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                var currentUserRole = User.FindFirst("Role")?.Value;

                if (currentUserRole != "Admin" &&
                    (!int.TryParse(currentUserIdClaim, out int currentUserId) || currentUserId != userId))
                {
                    return Forbid();
                }

                var reviews = await _reviewService.GetUserReviewsAsync(userId, filter);
                return Ok(new { Success = true, Data = reviews });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reseñas del usuario {UserId}", userId);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReviewDto>> GetReview(int id)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review == null)
                    return NotFound(new { Success = false, Message = "Reseña no encontrada" });

                return Ok(new { Success = true, Data = review });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reseña {ReviewId}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto createDto)
        {
            try
            {
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(currentUserIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Success = false, Message = "Datos inválidos" });
                }

                var review = await _reviewService.CreateReviewAsync(userId, createDto);
                return CreatedAtAction(nameof(GetReview), new { id = review.Id },
                    new { Success = true, Data = review });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando reseña");
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ReviewDto>> UpdateReview(int id, [FromBody] UpdateReviewDto updateDto)
        {
            try
            {
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(currentUserIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Success = false, Message = "Datos inválidos" });
                }

                var review = await _reviewService.UpdateReviewAsync(id, userId, updateDto);
                if (review == null)
                    return NotFound(new { Success = false, Message = "Reseña no encontrada" });

                return Ok(new { Success = true, Data = review });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando reseña {ReviewId}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                var currentUserRole = User.FindFirst("Role")?.Value ?? "";
                
                if (!int.TryParse(currentUserIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                var result = await _reviewService.DeleteReviewAsync(id, userId, currentUserRole);
                if (!result)
                    return NotFound(new { Success = false, Message = "Reseña no encontrada" });

                return Ok(new { Success = true, Message = "Reseña eliminada exitosamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando reseña {ReviewId}", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("restaurant/{restaurantId}/summary")]
        [AllowAnonymous]
        public async Task<ActionResult<RestaurantReviewSummaryDto>> GetRestaurantReviewSummary(int restaurantId)
        {
            try
            {
                var summary = await _reviewService.GetRestaurantReviewSummaryAsync(restaurantId);
                if (summary == null)
                    return NotFound(new { Success = false, Message = "Restaurante no encontrado" });

                return Ok(new { Success = true, Data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo resumen de reseñas del restaurante {RestaurantId}", restaurantId);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("product/{productId}/summary")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductReviewSummaryDto>> GetProductReviewSummary(int productId)
        {
            try
            {
                var summary = await _reviewService.GetProductReviewSummaryAsync(productId);
                if (summary == null)
                    return NotFound(new { Success = false, Message = "Producto no encontrado" });

                return Ok(new { Success = true, Data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo resumen de reseñas del producto {ProductId}", productId);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpPost("{id}/helpful")]
        [Authorize]
        public async Task<IActionResult> MarkAsHelpful(int id)
        {
            try
            {
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(currentUserIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                var result = await _reviewService.MarkReviewAsHelpfulAsync(id, userId);
                if (!result)
                    return BadRequest(new { Success = false, Message = "No se pudo marcar como útil" });

                return Ok(new { Success = true, Message = "Marcado como útil" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marcando reseña {ReviewId} como útil", id);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("can-review/restaurant/{restaurantId}")]
        [Authorize]
        public async Task<ActionResult<bool>> CanReviewRestaurant(int restaurantId)
        {
            try
            {
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(currentUserIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                var canReview = await _reviewService.CanUserReviewRestaurantAsync(userId, restaurantId);
                return Ok(new { Success = true, Data = canReview });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando si puede reseñar restaurante {RestaurantId}", restaurantId);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }

        [HttpGet("can-review/product/{productId}")]
        [Authorize]
        public async Task<ActionResult<bool>> CanReviewProduct(int productId)
        {
            try
            {
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(currentUserIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                var canReview = await _reviewService.CanUserReviewProductAsync(userId, productId);
                return Ok(new { Success = true, Data = canReview });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando si puede reseñar producto {ProductId}", productId);
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }
    }
}