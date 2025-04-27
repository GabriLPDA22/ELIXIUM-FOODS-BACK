using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
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
        private readonly IRepository<Address> _addressRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AddressesController(
            IRepository<Address> addressRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _addressRepository = addressRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetUserAddresses()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userRepository.GetWithAddressesAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(_mapper.Map<IEnumerable<AddressDto>>(user.Addresses));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AddressDto>> GetAddressById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var address = await _addressRepository.GetByIdAsync(id);

            if (address == null)
                return NotFound();

            // Verificar que la dirección pertenece al usuario
            if (address.UserId != userId && User.FindFirst(ClaimTypes.Role)?.Value != "Admin")
                return Forbid();

            return Ok(_mapper.Map<AddressDto>(address));
        }

        [HttpPost]
        public async Task<ActionResult<AddressDto>> CreateAddress(CreateAddressDto createAddressDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return NotFound();

            var address = _mapper.Map<Address>(createAddressDto);
            address.UserId = userId;

            var createdAddress = await _addressRepository.AddAsync(address);

            return CreatedAtAction(
                nameof(GetAddressById),
                new { id = createdAddress.Id },
                _mapper.Map<AddressDto>(createdAddress));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, UpdateAddressDto updateAddressDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var address = await _addressRepository.GetByIdAsync(id);

            if (address == null)
                return NotFound();

            // Verificar que la dirección pertenece al usuario
            if (address.UserId != userId && User.FindFirst(ClaimTypes.Role)?.Value != "Admin")
                return Forbid();

            // Actualizar propiedades
            address.Street = updateAddressDto.Street;
            address.City = updateAddressDto.City;
            address.State = updateAddressDto.State;
            address.ZipCode = updateAddressDto.ZipCode;
            address.Latitude = updateAddressDto.Latitude;
            address.Longitude = updateAddressDto.Longitude;

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

            await _addressRepository.DeleteAsync(address);

            return NoContent();
        }
    }
}
