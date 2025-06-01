using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.Services;
using UberEatsBackend.DTOs.Product;
using UberEatsBackend.DTOs.Restaurant;
using UberEatsBackend.Models;

namespace UberEatsBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public SearchController(
            IRestaurantService restaurantService,
            IProductService productService,
            IMapper mapper)
        {
            _restaurantService = restaurantService;
            _productService = productService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> SearchAll(
            [FromQuery] string? query,
            [FromQuery] int? category,
            [FromQuery] string? sortBy,
            [FromQuery] string? location,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            try
            {
                var searchedRestaurantEntities = await _restaurantService.SearchRestaurantsAsync(query ?? string.Empty, category);
                var restaurantDtos = _mapper.Map<List<RestaurantCardDto>>(searchedRestaurantEntities);

                var searchedProductDtos = await _productService.SearchProductsAsync(query ?? string.Empty, category);

                var results = new
                {
                    Restaurants = restaurantDtos,
                    Products = searchedProductDtos,
                    TotalResults = restaurantDtos.Count + searchedProductDtos.Count,
                    Query = query ?? string.Empty
                };

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Consider logging the exception ex
                return StatusCode(500, "An error occurred while processing your search request.");
            }
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(new List<string>());
            }

            // Placeholder: Implement actual suggestion logic based on your data
            var placeholderSuggestions = new List<string>
            {
                query + " suggestion A",
                query + " suggestion B",
                "Best " + query
            }.Take(5).ToList();
            await Task.CompletedTask;

            return Ok(placeholderSuggestions);
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularSearches()
        {
            // Placeholder: Implement actual logic to retrieve popular searches
            var popularSearches = new List<string> {
                "Pizza",
                "Burgers",
                "Sushi",
                "Salads",
                "Dessert"
            };
            await Task.CompletedTask;

            return Ok(popularSearches);
        }
    }
}
