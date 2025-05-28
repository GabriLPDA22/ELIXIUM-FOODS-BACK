using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;
using UberEatsBackend.Data;
using AutoMapper;
using System.Linq;

namespace UberEatsBackend.Services
{
  public class RestaurantService : IRestaurantService
  {
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RestaurantService(
        IRestaurantRepository restaurantRepository,
        ApplicationDbContext context,
        IMapper mapper)
    {
      _restaurantRepository = restaurantRepository;
      _context = context;
      _mapper = mapper;
    }

    public async Task<List<Restaurant>> GetAllRestaurantsAsync()
    {
      return await _context.Restaurants
          .OrderBy(r => r.Name)
          .ToListAsync();
    }

    public async Task<List<Restaurant>> GetPopularRestaurantsAsync(int limit = 10)
    {
      return await _restaurantRepository.GetPopularRestaurantsAsync(limit);
    }

    public async Task<List<Restaurant>> SearchRestaurantsAsync(string? query, string? cuisine)
    {
      return await _restaurantRepository.SearchRestaurantsAsync(query, cuisine);
    }

    public async Task<Restaurant?> GetRestaurantWithDetailsAsync(int id)
    {
      return await _restaurantRepository.GetWithDetailsAsync(id);
    }

    public async Task<bool> IsUserAuthorizedForRestaurant(int restaurantId, int userId, string userRole)
    {
      if (userRole == "Admin")
        return true;

      return false;
    }

    public async Task<List<Restaurant>> GetRestaurantsByTipoAsync(int tipo)
    {
      return await _restaurantRepository.GetByTipoAsync(tipo);
    }

    public async Task<List<Restaurant>> GetRestaurantsByBusinessIdAsync(int businessId)
    {
      return await _restaurantRepository.GetByBusinessIdAsync(businessId);
    }

    public async Task<bool> IsBusinessOwner(int businessId, int userId)
    {
      return false;
    }

    public async Task<List<Restaurant>> GetRestaurantsForAdminAsync(int userId)
    {
      return await _context.Restaurants
          .Include(r => r.Address)
          .Include(r => r.Business)
          .OrderBy(r => r.Name)
          .ToListAsync();
    }
  }
}
