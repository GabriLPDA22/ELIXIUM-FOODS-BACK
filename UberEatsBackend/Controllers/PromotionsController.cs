// UberEatsBackend/Controllers/PromotionsController.cs
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UberEatsBackend.DTOs.Business;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/promotions")]
    public class PromotionsController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public PromotionsController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PromotionDto>>> GetBusinessPromotions(int businessId)
        {
            try
            {
                var promotions = await _businessService.GetBusinessPromotionsAsync(businessId);
                return Ok(promotions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PromotionDto>> CreatePromotion(int businessId, CreatePromotionDto createPromotionDto)
        {
            try
            {
                // Verificar autorización
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (!await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole!))
                    return Forbid();

                var createdPromotion = await _businessService.CreatePromotionAsync(businessId, createPromotionDto);
                return CreatedAtAction(nameof(GetPromotion), new { businessId, promotionId = createdPromotion.Id }, createdPromotion);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{promotionId}")]
        public async Task<ActionResult<PromotionDto>> GetPromotion(int businessId, int promotionId)
        {
            try
            {
                var promotions = await _businessService.GetBusinessPromotionsAsync(businessId);
                var promotion = promotions.Find(p => p.Id == promotionId);

                if (promotion == null)
                    return NotFound($"Promoción con ID {promotionId} no encontrada en el negocio con ID {businessId}");

                return Ok(promotion);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{promotionId}")]
        [Authorize]
        public async Task<ActionResult<PromotionDto>> UpdatePromotion(int businessId, int promotionId, UpdatePromotionDto updatePromotionDto)
        {
            try
            {
                // Verificar autorización
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (!await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole!))
                    return Forbid();

                var updatedPromotion = await _businessService.UpdatePromotionAsync(businessId, promotionId, updatePromotionDto);

                if (updatedPromotion == null)
                    return NotFound($"Promoción con ID {promotionId} no encontrada en el negocio con ID {businessId}");

                return Ok(updatedPromotion);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{promotionId}")]
        [Authorize]
        public async Task<ActionResult> DeletePromotion(int businessId, int promotionId)
        {
            try
            {
                // Verificar autorización
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (!await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole!))
                    return Forbid();

                var result = await _businessService.DeletePromotionAsync(businessId, promotionId);

                if (!result)
                    return NotFound($"Promoción con ID {promotionId} no encontrada en el negocio con ID {businessId}");

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("{promotionId}/activate")]
        [Authorize]
        public async Task<ActionResult> ActivatePromotion(int businessId, int promotionId)
        {
            try
            {
                // Verificar autorización
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (!await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole!))
                    return Forbid();

                var result = await _businessService.ActivatePromotionAsync(businessId, promotionId);

                if (!result)
                    return NotFound($"Promoción con ID {promotionId} no encontrada en el negocio con ID {businessId}");

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch("{promotionId}/deactivate")]
        [Authorize]
        public async Task<ActionResult> DeactivatePromotion(int businessId, int promotionId)
        {
            try
            {
                // Verificar autorización
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userRole = User.FindFirstValue(ClaimTypes.Role);

                if (!await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole!))
                    return Forbid();

                var result = await _businessService.DeactivatePromotionAsync(businessId, promotionId);

                if (!result)
                    return NotFound($"Promoción con ID {promotionId} no encontrada en el negocio con ID {businessId}");

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
