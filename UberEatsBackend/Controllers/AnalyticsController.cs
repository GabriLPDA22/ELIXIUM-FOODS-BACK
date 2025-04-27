// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using UberEatsBackend.DTOs.Dashboard;
// using UberEatsBackend.Services;

// namespace UberEatsBackend.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize(Roles = "Admin,Restaurant")]
//     public class AnalyticsController : ControllerBase
//     {
//         private readonly AnalyticsService _analyticsService;

//         public AnalyticsController(AnalyticsService analyticsService)
//         {
//             _analyticsService = analyticsService;
//         }

//         [HttpGet("users/growth")]
//         [Authorize(Roles = "Admin")]
//         public async Task<ActionResult<List<UserGrowthDto>>> GetUserGrowth([FromQuery] DashboardFilterDto filter)
//         {
//             var data = await _analyticsService.GetUserGrowthAsync(filter);
//             return Ok(data);
//         }

//         [HttpGet("users/retention")]
//         [Authorize(Roles = "Admin")]
//         public async Task<ActionResult<UserRetentionDto>> GetUserRetention([FromQuery] DashboardFilterDto filter)
//         {
//             var data = await _analyticsService.GetUserRetentionAsync(filter);
//             return Ok(data);
//         }

//         [HttpGet("orders/heatmap")]
//         public async Task<ActionResult<List<OrderHeatmapDto>>> GetOrdersHeatmap([FromQuery] DashboardFilterDto filter)
//         {
//             var data = await _analyticsService.GetOrdersHeatmapAsync(filter);
//             return Ok(data);
//         }

//         [HttpGet("restaurant/{id}/customers")]
//         [Authorize(Roles = "Admin,Restaurant")]
//         public async Task<ActionResult<List<CustomerInsightDto>>> GetRestaurantCustomers(int id, [FromQuery] DashboardFilterDto filter)
//         {
//             filter.RestaurantId = id;
//             var data = await _analyticsService.GetRestaurantCustomersAsync(filter);

//             if (data == null)
//                 return NotFound($"Restaurant with ID {id} not found");

//             return Ok(data);
//         }

//         [HttpGet("restaurant/{id}/performance")]
//         [Authorize(Roles = "Admin,Restaurant")]
//         public async Task<ActionResult<RestaurantPerformanceDto>> GetRestaurantPerformance(int id, [FromQuery] DashboardFilterDto filter)
//         {
//             filter.RestaurantId = id;
//             var data = await _analyticsService.GetRestaurantPerformanceAsync(filter);

//             if (data == null)
//                 return NotFound($"Restaurant with ID {id} not found");

//             return Ok(data);
//         }

//         [HttpGet("delivery/metrics")]
//         [Authorize(Roles = "Admin")]
//         public async Task<ActionResult<DeliveryMetricsDto>> GetDeliveryMetrics([FromQuery] DashboardFilterDto filter)
//         {
//             var data = await _analyticsService.GetDeliveryMetricsAsync(filter);
//             return Ok(data);
//         }
//     }
// }
