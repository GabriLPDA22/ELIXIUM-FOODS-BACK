using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Restaurant;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;
using AutoMapper;

namespace UberEatsBackend.Services
{
  public class RestaurantService : IRestaurantService
  {
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IMapper _mapper;

    public RestaurantService(IRestaurantRepository restaurantRepository, IMapper mapper)
    {
      _restaurantRepository = restaurantRepository;
      _mapper = mapper;
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

      return await _restaurantRepository.IsOwner(restaurantId, userId);
    }

    public async Task<List<Restaurant>> GetRestaurantsByOwnerAsync(int userId)
    {
      return await _restaurantRepository.GetByOwnerAsync(userId);
    }
  }

  public interface IRestaurantService
  {
    Task<List<Restaurant>> GetPopularRestaurantsAsync(int limit = 10);
    Task<List<Restaurant>> SearchRestaurantsAsync(string? query, string? cuisine);
    Task<Restaurant?> GetRestaurantWithDetailsAsync(int id);
    Task<bool> IsUserAuthorizedForRestaurant(int restaurantId, int userId, string userRole);
    Task<List<Restaurant>> GetRestaurantsByOwnerAsync(int userId);
  }
}
