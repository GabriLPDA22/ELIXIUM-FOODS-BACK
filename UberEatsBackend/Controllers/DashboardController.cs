// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using UberEatsBackend.DTOs.Dashboard;
// using UberEatsBackend.Services;

// namespace UberEatsBackend.Controllers
// {
//   [ApiController]
//   [Route("api/[controller]")]
//   [Authorize(Roles = "Admin")]
//   public class DashboardController : ControllerBase
//   {
//     private readonly DashboardService _dashboardService;

//     public DashboardController(DashboardService dashboardService)
//     {
//       _dashboardService = dashboardService;
//     }

//     [HttpGet("stats")]
//     public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats([FromQuery] DashboardFilterDto filter)
//     {
//       var stats = await _dashboardService.GetDashboardStatsAsync(filter);
//       return Ok(stats);
//     }

//     [HttpGet("restaurants/top")]
//     public async Task<ActionResult<List<TopRestaurantDto>>> GetTopRestaurants([FromQuery] DashboardFilterDto filter)
//     {
//       var topRestaurants = await _dashboardService.GetTopRestaurantsAsync(filter);
//       return Ok(topRestaurants);
//     }

//     [HttpGet("products/top")]
//     public async Task<ActionResult<List<TopProductDto>>> GetTopProducts([FromQuery] DashboardFilterDto filter)
//     {
//       var topProducts = await _dashboardService.GetTopProductsAsync(filter);
//       return Ok(topProducts);
//     }

//     [HttpGet("revenue/bydate")]
//     public async Task<ActionResult<List<RevenueByDateDto>>> GetRevenueByDate([FromQuery] DashboardFilterDto filter)
//     {
//       var revenueByDate = await _dashboardService.GetRevenueByDateAsync(filter);
//       return Ok(revenueByDate);
//     }

//     [HttpGet("orders/bystatus")]
//     public async Task<ActionResult<List<OrdersByStatusDto>>> GetOrdersByStatus([FromQuery] DashboardFilterDto filter)
//     {
//       var ordersByStatus = await _dashboardService.GetOrdersByStatusAsync(filter);
//       return Ok(ordersByStatus);
//     }

//     [HttpGet("restaurant/{id}/stats")]
//     public async Task<ActionResult<RestaurantStatsDto>> GetRestaurantStats(int id, [FromQuery] DashboardFilterDto filter)
//     {
//       filter.RestaurantId = id;
//       var stats = await _dashboardService.GetRestaurantStatsAsync(filter);

//       if (stats == null)
//         return NotFound($"Restaurant with ID {id} not found");

//       return Ok(stats);
//     }
//   }
// }
