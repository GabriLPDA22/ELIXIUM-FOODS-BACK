using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public BusinessController(IBusinessService businessService)
    {
      _businessService = businessService;
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

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BusinessDto>> UpdateBusiness(int id, UpdateBusinessDto updateBusinessDto)
    {
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BusinessStatsDto>> GetBusinessStats(int id)
    {
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
  }
}
