using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Business;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class BusinessController : ControllerBase
  {
    private readonly IBusinessService _businessService;
    private readonly IUserService _userService;

    public BusinessController(IBusinessService businessService, IUserService userService)
    {
      _businessService = businessService;
      _userService = userService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BusinessDto>>> GetAllBusinesses()
    {
      var businesses = await _businessService.GetAllBusinessesAsync();
      return Ok(businesses);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<BusinessDto>> GetBusiness(int id)
    {
      var business = await _businessService.GetBusinessByIdAsync(id);
      if (business == null)
      {
        return NotFound();
      }
      return Ok(business);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BusinessDto>> CreateBusiness(CreateBusinessDto createBusinessDto)
    {
      var createdBusiness = await _businessService.CreateBusinessAsync(createBusinessDto);
      return CreatedAtAction(
          nameof(GetBusiness),
          new { id = createdBusiness.Id },
          createdBusiness);
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<ActionResult<BusinessDto>> RegisterBusiness(CreateBusinessDto createBusinessDto)
    {
      try
      {
        // Obtener el ID del usuario actual
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
          return BadRequest("ID de usuario inválido");
        }

        // Verificar si el usuario ya tiene un negocio
        var existingBusiness = await _businessService.GetBusinessByAssignedUserIdAsync(userId);
        if (existingBusiness != null)
        {
          return BadRequest("El usuario ya tiene un negocio asociado");
        }

        // Asignar el usuario actual al negocio
        createBusinessDto.UserId = userId;

        // Crear el negocio
        var createdBusiness = await _businessService.CreateBusinessAsync(createBusinessDto);

        // Actualizar el rol del usuario a "Business"
        await _userService.UpdateUserRoleAsync(userId, "Business");

        return CreatedAtAction(
          nameof(GetBusiness),
          new { id = createdBusiness.Id },
          createdBusiness);
      }
      catch (Exception ex)
      {
        return StatusCode(500, ex.Message);
      }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<BusinessDto>> UpdateBusiness(int id, UpdateBusinessDto updateBusinessDto)
    {
      // Verificar si el usuario tiene autorización para este negocio
      if (!await IsAuthorizedForBusiness(id))
      {
        return Forbid();
      }

      var updatedBusiness = await _businessService.UpdateBusinessAsync(id, updateBusinessDto);
      if (updatedBusiness == null)
      {
        return NotFound();
      }
      return Ok(updatedBusiness);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBusiness(int id)
    {
      var result = await _businessService.DeleteBusinessAsync(id);
      if (!result)
      {
        return NotFound();
      }
      return NoContent();
    }

    [HttpGet("{id}/Stats")]
    [Authorize(Roles = "Admin,Business")]
    public async Task<ActionResult<BusinessStatsDto>> GetBusinessStats(int id)
    {
      // Verificar si el usuario tiene autorización para este negocio
      if (!await IsAuthorizedForBusiness(id))
      {
        return Forbid();
      }

      try
      {
        var stats = await _businessService.GetBusinessStatsAsync(id);
        return Ok(stats);
      }
      catch (KeyNotFoundException)
      {
        return NotFound();
      }
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<BusinessDto>> GetBusinessByUserId(int userId)
    {
        var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
        {
            return Unauthorized("User ID claim is missing or invalid.");
        }

        if (currentUserId != userId && currentUserRole != "Admin")
        {
            return Forbid("You are not authorized to access this resource.");
        }

        var businessDto = await _businessService.GetBusinessByAssignedUserIdAsync(userId);

        if (businessDto == null)
        {
            return NotFound($"No business found associated with User ID {userId}");
        }
        return Ok(businessDto);
    }

    private bool IsAdministrator()
    {
      var userRole = User.FindFirstValue(ClaimTypes.Role);
      return _businessService.IsAdministrator(userRole ?? string.Empty);
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
          var business = await _businessService.GetBusinessByIdAsync(businessId);
          return business?.UserId == userId;
      }

      return false;
    }
  }
}
