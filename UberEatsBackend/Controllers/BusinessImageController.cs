// UberEatsBackend/Controllers/BusinessImageController.cs
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/businesses")]
  public class BusinessImageController : ControllerBase
  {
    private readonly IBusinessService _businessService;
    private readonly IStorageService _storageService;

    public BusinessImageController(
        IBusinessService businessService,
        IStorageService storageService)
    {
      _businessService = businessService;
      _storageService = storageService;
    }

    [HttpPost("{businessId}/logo")]
    [Authorize]
    public async Task<IActionResult> UploadLogo(int businessId, IFormFile file)
    {
      try
      {
        // Verificar autorización
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (!await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole!))
          return Forbid();

        // Comprobar que se ha subido un archivo
        if (file == null || file.Length == 0)
          return BadRequest("No se ha proporcionado una imagen válida");

        // Comprobar tipo de archivo
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!Array.Exists(allowedTypes, type => type.Equals(file.ContentType)))
          return BadRequest("Formato de imagen no válido. Use JPEG, PNG, GIF o WEBP.");

        // Convertir a base64
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var base64Image = Convert.ToBase64String(memoryStream.ToArray());

        // Guardar el archivo
        var logoUrl = await _storageService.SaveFileAsync(base64Image, file.FileName);

        // Actualizar la URL del logo en la base de datos
        await _businessService.UpdateLogoAsync(businessId, logoUrl);

        return Ok(new { logoUrl });
      }
      catch (Exception ex)
      {
        return StatusCode(500, ex.Message);
      }
    }

    [HttpPost("{businessId}/cover-image")]
    [Authorize]
    public async Task<IActionResult> UploadCoverImage(int businessId, IFormFile file)
    {
      try
      {
        // Verificar autorización
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (!await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole!))
          return Forbid();

        // Comprobar que se ha subido un archivo
        if (file == null || file.Length == 0)
          return BadRequest("No se ha proporcionado una imagen válida");

        // Comprobar tipo de archivo
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!Array.Exists(allowedTypes, type => type.Equals(file.ContentType)))
          return BadRequest("Formato de imagen no válido. Use JPEG, PNG, GIF o WEBP.");

        // Convertir a base64
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var base64Image = Convert.ToBase64String(memoryStream.ToArray());

        // Guardar el archivo
        var coverImageUrl = await _storageService.SaveFileAsync(base64Image, file.FileName);

        // Actualizar la URL de la imagen de portada en la base de datos
        await _businessService.UpdateCoverImageAsync(businessId, coverImageUrl);

        return Ok(new { coverImageUrl });
      }
      catch (Exception ex)
      {
        return StatusCode(500, ex.Message);
      }
    }

    [HttpDelete("{businessId}/logo")]
    [Authorize]
    public async Task<IActionResult> DeleteLogo(int businessId)
    {
      try
      {
        // Verificar autorización
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (!await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole!))
          return Forbid();

        // Obtener el negocio
        var business = await _businessService.GetBusinessByIdAsync(businessId);
        if (business == null)
          return NotFound($"Negocio con ID {businessId} no encontrado");

        // Eliminar el archivo si existe
        if (!string.IsNullOrEmpty(business.LogoUrl))
        {
          await _storageService.DeleteFileAsync(business.LogoUrl);
        }

        // Actualizar la URL del logo en la base de datos
        await _businessService.UpdateLogoAsync(businessId, null);

        return Ok(new { message = "Logo eliminado correctamente" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, ex.Message);
      }
    }

    [HttpDelete("{businessId}/cover-image")]
    [Authorize]
    public async Task<IActionResult> DeleteCoverImage(int businessId)
    {
      try
      {
        // Verificar autorización
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (!await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole!))
          return Forbid();

        // Obtener el negocio
        var business = await _businessService.GetBusinessByIdAsync(businessId);
        if (business == null)
          return NotFound($"Negocio con ID {businessId} no encontrado");

        // Eliminar el archivo si existe
        if (!string.IsNullOrEmpty(business.CoverImageUrl))
        {
          await _storageService.DeleteFileAsync(business.CoverImageUrl);
        }

        // Actualizar la URL de la imagen de portada en la base de datos
        await _businessService.UpdateCoverImageAsync(businessId, null);

        return Ok(new { message = "Imagen de portada eliminada correctamente" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, ex.Message);
      }
    }
  }
}
