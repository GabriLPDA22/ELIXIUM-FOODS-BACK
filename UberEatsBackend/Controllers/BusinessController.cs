// UberEatsBackend/Controllers/BusinessController.cs
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
  [Authorize]
  public class BusinessController : ControllerBase
  {
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
      _businessService = businessService;
    }

    // GET: api/Business
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<BusinessDto>>> GetAllBusinesses()
    {
      var businesses = await _businessService.GetAllBusinessesAsync();
      return Ok(businesses);
    }

    // GET: api/Business/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BusinessDto>> GetBusiness(int id)
    {
      // Verificar que el usuario tiene acceso al negocio
      if (!await CanAccessBusiness(id))
      {
        return Forbid();
      }

      var business = await _businessService.GetBusinessByIdAsync(id);
      if (business == null)
      {
        return NotFound();
      }

      return Ok(business);
    }

    // GET: api/Business/MyBusinesses
    [HttpGet("MyBusinesses")]
    public async Task<ActionResult<IEnumerable<BusinessDto>>> GetMyBusinesses()
    {
      int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      var businesses = await _businessService.GetBusinessesByOwnerIdAsync(userId);
      return Ok(businesses);
    }

    // POST: api/Business
    [HttpPost]
    [Authorize(Roles = "Restaurant,Admin")]
    public async Task<ActionResult<BusinessDto>> CreateBusiness(CreateBusinessDto createBusinessDto)
    {
      int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      var createdBusiness = await _businessService.CreateBusinessAsync(userId, createBusinessDto);

      return CreatedAtAction(
          nameof(GetBusiness),
          new { id = createdBusiness.Id },
          createdBusiness);
    }

    // PUT: api/Business/5
    [HttpPut("{id}")]
    public async Task<ActionResult<BusinessDto>> UpdateBusiness(int id, UpdateBusinessDto updateBusinessDto)
    {
      // Verificar que el usuario tiene acceso al negocio
      if (!await CanAccessBusiness(id))
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

    // DELETE: api/Business/5
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

    // GET: api/Business/5/Stats
    [HttpGet("{id}/Stats")]
    public async Task<ActionResult<BusinessStatsDto>> GetBusinessStats(int id)
    {
      // Verificar que el usuario tiene acceso al negocio
      if (!await CanAccessBusiness(id))
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

    // Método auxiliar para verificar si el usuario tiene acceso al negocio
    private async Task<bool> CanAccessBusiness(int businessId)
    {
      var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
      var userRole = User.FindFirstValue(ClaimTypes.Role);

      return await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole);
    }
  }
}
