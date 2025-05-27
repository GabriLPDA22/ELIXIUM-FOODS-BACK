using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.DTOs.Category;
using UberEatsBackend.Models;
using UberEatsBackend.Services;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IBusinessService _businessService;

        public CategoriesController(ApplicationDbContext context, IMapper mapper, IBusinessService businessService)
        {
            _context = context;
            _mapper = mapper;
            _businessService = businessService;
        }

        // GET: api/categories
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Business)
                .Include(c => c.Products)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<CategoryDto>>(categories));
        }

        // GET: api/categories/business/5
        [HttpGet("business/{businessId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesByBusiness(int businessId)
        {
            var categories = await _context.Categories
                .Where(c => c.BusinessId == businessId)
                .Include(c => c.Business)
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<CategoryDto>>(categories));
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Business)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            return Ok(_mapper.Map<CategoryDto>(category));
        }

        // POST: api/categories
        [HttpPost]
        [Authorize(Roles = "Admin,Business")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            // Verify authorization for the business
            if (!await IsAuthorizedForBusiness(createCategoryDto.BusinessId))
                return Forbid();

            var category = _mapper.Map<Category>(createCategoryDto);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var createdCategory = await _context.Categories
                .Include(c => c.Business)
                .Include(c => c.Products)
                .FirstAsync(c => c.Id == category.Id);

            return CreatedAtAction(nameof(GetCategory),
                new { id = category.Id },
                _mapper.Map<CategoryDto>(createdCategory));
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Business")]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories
                .Include(c => c.Business)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            // Verify authorization
            if (!await IsAuthorizedForBusiness(category.BusinessId))
                return Forbid();

            _mapper.Map(updateCategoryDto, category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Business")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Business)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            // Verify authorization
            if (!await IsAuthorizedForBusiness(category.BusinessId))
                return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> IsAuthorizedForBusiness(int businessId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return false;

            return await _businessService.IsUserAuthorizedForBusiness(businessId, userId, userRole ?? "");
        }
    }
}
