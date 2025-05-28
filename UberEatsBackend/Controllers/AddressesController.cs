using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.DTOs.Address;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;

namespace UberEatsBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class AddressesController : ControllerBase
  {
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Address> _addressRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AddressesController(
        ApplicationDbContext context,
        IRepository<Address> addressRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
      _context = context;
      _addressRepository = addressRepository;
      _userRepository = userRepository;
      _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExtendedAddressDto>>> GetUserAddresses()
    {
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var addresses = await _context.Addresses
          .Where(a => a.UserId == userId)
          .ToListAsync();

      return Ok(_mapper.Map<IEnumerable<ExtendedAddressDto>>(addresses));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExtendedAddressDto>> GetAddressById(int id)
    {
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var address = await _addressRepository.GetByIdAsync(id);

      if (address == null)
        return NotFound();

      // Verificar que la dirección pertenece al usuario
      if (address.UserId != userId && User.FindFirst(ClaimTypes.Role)?.Value != "Admin")
        return Forbid();

      return Ok(_mapper.Map<ExtendedAddressDto>(address));
    }

    [HttpPost]
public async Task<ActionResult<ExtendedAddressDto>> CreateAddress(CreateExtendedAddressDto createAddressDto)
{
  var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
  var user = await _userRepository.GetByIdAsync(userId);

  if (user == null)
    return NotFound();

  var address = _mapper.Map<Address>(createAddressDto);
  address.UserId = userId; // Asegurar que se asigne el UserId para direcciones de usuarios

  // Si esta dirección se marca como predeterminada, desmarcar las demás
  if (address.IsDefault)
  {
    await UnsetDefaultAddresses(userId);
  }
  // Si es la primera dirección, marcarla como predeterminada automáticamente
  else if (!await _context.Addresses.AnyAsync(a => a.UserId == userId))
  {
    address.IsDefault = true;
  }

  var createdAddress = await _addressRepository.CreateAsync(address);

  return CreatedAtAction(
      nameof(GetAddressById),
      new { id = createdAddress.Id },
      _mapper.Map<ExtendedAddressDto>(createdAddress));
}

    [HttpPut("{id}")]
    public async Task<ActionResult<ExtendedAddressDto>> UpdateAddress(int id, CreateExtendedAddressDto updateAddressDto)
    {
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var address = await _addressRepository.GetByIdAsync(id);

      if (address == null)
        return NotFound();

      // Verificar que la dirección pertenece al usuario
      if (address.UserId != userId && User.FindFirst(ClaimTypes.Role)?.Value != "Admin")
        return Forbid();

      // Si esta dirección se marca como predeterminada, desmarcar las demás
      if (updateAddressDto.IsDefault && !address.IsDefault)
      {
        await UnsetDefaultAddresses(userId);
      }

      // Actualizar propiedades
      _mapper.Map(updateAddressDto, address);

      await _addressRepository.UpdateAsync(address);

      return Ok(_mapper.Map<ExtendedAddressDto>(address));
    }

    [HttpPut("{id}/default")]
    public async Task<IActionResult> SetAsDefault(int id)
    {
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var address = await _addressRepository.GetByIdAsync(id);

      if (address == null)
        return NotFound();

      // Verificar que la dirección pertenece al usuario
      if (address.UserId != userId && User.FindFirst(ClaimTypes.Role)?.Value != "Admin")
        return Forbid();

      // Desmarcar las direcciones actuales como predeterminadas
      await UnsetDefaultAddresses(userId);

      // Marcar esta dirección como predeterminada
      address.IsDefault = true;
      await _addressRepository.UpdateAsync(address);

      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
      var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
      var address = await _addressRepository.GetByIdAsync(id);

      if (address == null)
        return NotFound();

      // Verificar que la dirección pertenece al usuario
      if (address.UserId != userId && User.FindFirst(ClaimTypes.Role)?.Value != "Admin")
        return Forbid();

      // Guardar si era la dirección predeterminada
      bool wasDefault = address.IsDefault;

      await _addressRepository.DeleteAsync(address.Id);

      // Si era la predeterminada, buscar otra dirección para marcar como predeterminada
      if (wasDefault)
      {
        var firstAddress = await _context.Addresses
            .Where(a => a.UserId == userId)
            .FirstOrDefaultAsync();

        if (firstAddress != null)
        {
          firstAddress.IsDefault = true;
          await _addressRepository.UpdateAsync(firstAddress);
        }
      }

      return NoContent();
    }

    // Método privado para desmarcar todas las direcciones predeterminadas de un usuario
    private async Task UnsetDefaultAddresses(int userId)
    {
      var defaultAddresses = await _context.Addresses
          .Where(a => a.UserId == userId && a.IsDefault)
          .ToListAsync();

      foreach (var addr in defaultAddresses)
      {
        addr.IsDefault = false;
        await _addressRepository.UpdateAsync(addr);
      }
    }
  }
}
