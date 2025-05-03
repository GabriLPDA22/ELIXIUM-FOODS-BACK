using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Data;
using UberEatsBackend.DTOs.Category;
using UberEatsBackend.Models;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoriesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/categories
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<CategoryDto>>(categories));
        }

        // GET: api/categories/restaurant-types
        [HttpGet("restaurant-types")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetRestaurantTypes()
        {
            var restaurantTypes = await _context.Categories
                .Where(c => c.MenuId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<CategoryDto>>(restaurantTypes));
        }

        // GET: api/categories/menu/5
        [HttpGet("menu/{menuId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesByMenu(int menuId)
        {
            var categories = await _context.Categories
                .Where(c => c.MenuId == menuId)
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
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CategoryDto>(category));
        }
    }
}
