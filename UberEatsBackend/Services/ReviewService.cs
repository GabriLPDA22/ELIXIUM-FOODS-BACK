// UberEatsBackend/Services/ReviewService.cs
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UberEatsBackend.DTOs.Review;
using UberEatsBackend.Models;
using UberEatsBackend.Repositories;

namespace UberEatsBackend.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IReviewRepository reviewRepository,
            IUserRepository userRepository,
            IRestaurantRepository restaurantRepository,
            IProductRepository productRepository,
            IMapper mapper,
            ILogger<ReviewService> logger)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _restaurantRepository = restaurantRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ReviewDto>> GetRestaurantReviewsAsync(int restaurantId, ReviewFilterDto? filter = null)
        {
            try
            {
                var reviews = await _reviewRepository.GetReviewsByRestaurantIdAsync(restaurantId, filter);
                var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);
                
                // Calcular tiempo relativo
                foreach (var dto in reviewDtos)
                {
                    dto.TimeAgo = CalculateTimeAgo(dto.CreatedAt);
                }
                
                return reviewDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reseñas del restaurante {RestaurantId}", restaurantId);
                throw;
            }
        }

        public async Task<List<ReviewDto>> GetProductReviewsAsync(int productId, ReviewFilterDto? filter = null)
        {
            try
            {
                var reviews = await _reviewRepository.GetReviewsByProductIdAsync(productId, filter);
                var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);
                
                foreach (var dto in reviewDtos)
                {
                    dto.TimeAgo = CalculateTimeAgo(dto.CreatedAt);
                }
                
                return reviewDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reseñas del producto {ProductId}", productId);
                throw;
            }
        }

        public async Task<List<ReviewDto>> GetUserReviewsAsync(int userId, ReviewFilterDto? filter = null)
        {
            try
            {
                var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId, filter);
                var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);
                
                foreach (var dto in reviewDtos)
                {
                    dto.TimeAgo = CalculateTimeAgo(dto.CreatedAt);
                }
                
                return reviewDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reseñas del usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null) return null;
                
                var reviewDto = _mapper.Map<ReviewDto>(review);
                reviewDto.TimeAgo = CalculateTimeAgo(reviewDto.CreatedAt);
                
                return reviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reseña {ReviewId}", id);
                throw;
            }
        }

        public async Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewDto createDto)
        {
            try
            {
                // Validar que el usuario existe
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado");

                // Validar que el restaurante existe
                var restaurant = await _restaurantRepository.GetByIdAsync(createDto.RestaurantId);
                if (restaurant == null)
                    throw new ArgumentException("Restaurante no encontrado");

                // Validar producto si se especifica
                if (createDto.ProductId.HasValue)
                {
                    var product = await _productRepository.GetByIdAsync(createDto.ProductId.Value);
                    if (product == null)
                        throw new ArgumentException("Producto no encontrado");
                }

                // Verificar si ya existe una reseña del usuario
                Review? existingReview = null;
                if (createDto.ProductId.HasValue)
                {
                    existingReview = await _reviewRepository.GetUserReviewForProductAsync(userId, createDto.ProductId.Value);
                }
                else
                {
                    existingReview = await _reviewRepository.GetUserReviewForRestaurantAsync(userId, createDto.RestaurantId);
                }

                if (existingReview != null)
                    throw new InvalidOperationException("Ya has reseñado este " + (createDto.ProductId.HasValue ? "producto" : "restaurante"));

                // Verificar si es compra verificada
                bool isVerifiedPurchase = false;
                if (createDto.ProductId.HasValue)
                {
                    isVerifiedPurchase = await _reviewRepository.HasUserPurchasedProductAsync(userId, createDto.ProductId.Value);
                }
                else
                {
                    isVerifiedPurchase = await _reviewRepository.HasUserOrderedFromRestaurantAsync(userId, createDto.RestaurantId);
                }

                // Crear la reseña
                var review = _mapper.Map<Review>(createDto);
                review.UserId = userId;
                review.IsVerifiedPurchase = isVerifiedPurchase;
                review.CreatedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;

                var createdReview = await _reviewRepository.CreateAsync(review);

                // Actualizar rating promedio del restaurante
                await _reviewRepository.UpdateRestaurantAverageRatingAsync(createDto.RestaurantId);

                _logger.LogInformation("Reseña creada exitosamente: {ReviewId} por usuario {UserId}", createdReview.Id, userId);

                var reviewDto = _mapper.Map<ReviewDto>(createdReview);
                reviewDto.TimeAgo = CalculateTimeAgo(reviewDto.CreatedAt);
                
                return reviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando reseña para usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<ReviewDto?> UpdateReviewAsync(int id, int userId, UpdateReviewDto updateDto)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null) return null;

                // Verificar que el usuario es el propietario de la reseña
                if (review.UserId != userId)
                    throw new UnauthorizedAccessException("No puedes editar esta reseña");

                _mapper.Map(updateDto, review);
                review.UpdatedAt = DateTime.UtcNow;

                await _reviewRepository.UpdateAsync(review);

                // Actualizar rating promedio del restaurante
                await _reviewRepository.UpdateRestaurantAverageRatingAsync(review.RestaurantId);

                _logger.LogInformation("Reseña actualizada exitosamente: {ReviewId}", id);

                var reviewDto = _mapper.Map<ReviewDto>(review);
                reviewDto.TimeAgo = CalculateTimeAgo(reviewDto.CreatedAt);
                
                return reviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando reseña {ReviewId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteReviewAsync(int id, int userId, string userRole)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null) return false;

                // Solo el propietario o un admin pueden eliminar la reseña
                if (review.UserId != userId && userRole != "Admin")
                    throw new UnauthorizedAccessException("No puedes eliminar esta reseña");

                // Soft delete
                review.IsActive = false;
                review.UpdatedAt = DateTime.UtcNow;
                await _reviewRepository.UpdateAsync(review);

                // Actualizar rating promedio del restaurante
                await _reviewRepository.UpdateRestaurantAverageRatingAsync(review.RestaurantId);

                _logger.LogInformation("Reseña eliminada exitosamente: {ReviewId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando reseña {ReviewId}", id);
                throw;
            }
        }

        public async Task<RestaurantReviewSummaryDto?> GetRestaurantReviewSummaryAsync(int restaurantId)
        {
            try
            {
                var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
                if (restaurant == null) return null;

                var stats = await _reviewRepository.GetRestaurantStatsAsync(restaurantId);
                var recentReviews = await _reviewRepository.GetTopReviewsByRestaurantAsync(restaurantId, 5);

                var summary = new RestaurantReviewSummaryDto
                {
                    RestaurantId = restaurantId,
                    RestaurantName = restaurant.Name,
                    Stats = stats,
                    RecentReviews = _mapper.Map<List<ReviewDto>>(recentReviews)
                };

                foreach (var dto in summary.RecentReviews)
                {
                    dto.TimeAgo = CalculateTimeAgo(dto.CreatedAt);
                }

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo resumen de reseñas del restaurante {RestaurantId}", restaurantId);
                throw;
            }
        }

        public async Task<ProductReviewSummaryDto?> GetProductReviewSummaryAsync(int productId)
        {
            try
            {
                var product = await _productRepository.GetProductWithDetailsAsync(productId);
                if (product == null) return null;

                var stats = await _reviewRepository.GetProductStatsAsync(productId);
                var recentReviews = await _reviewRepository.GetTopReviewsByProductAsync(productId, 5);

                var summary = new ProductReviewSummaryDto
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    RestaurantId = 0, // Necesitarías obtener esto del contexto o parámetro
                    RestaurantName = "", // Necesitarías obtener esto del contexto
                    Stats = stats,
                    RecentReviews = _mapper.Map<List<ReviewDto>>(recentReviews)
                };

                foreach (var dto in summary.RecentReviews)
                {
                    dto.TimeAgo = CalculateTimeAgo(dto.CreatedAt);
                }

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo resumen de reseñas del producto {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> CanUserReviewRestaurantAsync(int userId, int restaurantId)
        {
            try
            {
                // Verificar si ya tiene una reseña
                var existingReview = await _reviewRepository.GetUserReviewForRestaurantAsync(userId, restaurantId);
                if (existingReview != null) return false;

                // Verificar si ha ordenado del restaurante
                return await _reviewRepository.HasUserOrderedFromRestaurantAsync(userId, restaurantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando si usuario {UserId} puede reseñar restaurante {RestaurantId}", userId, restaurantId);
                return false;
            }
        }

        public async Task<bool> CanUserReviewProductAsync(int userId, int productId)
        {
            try
            {
                // Verificar si ya tiene una reseña
                var existingReview = await _reviewRepository.GetUserReviewForProductAsync(userId, productId);
                if (existingReview != null) return false;

                // Verificar si ha comprado el producto
                return await _reviewRepository.HasUserPurchasedProductAsync(userId, productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando si usuario {UserId} puede reseñar producto {ProductId}", userId, productId);
                return false;
            }
        }

        public async Task<bool> MarkReviewAsHelpfulAsync(int reviewId, int userId)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null) return false;

                // Verificar que no sea su propia reseña
                if (review.UserId == userId) return false;

                // Por simplicidad, incrementamos el contador
                // En una implementación más robusta, guardarías qué usuarios marcaron como útil
                review.HelpfulCount++;
                review.UpdatedAt = DateTime.UtcNow;
                await _reviewRepository.UpdateAsync(review);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marcando reseña {ReviewId} como útil", reviewId);
                return false;
            }
        }

        public async Task<ReviewDto?> GetUserReviewForRestaurantAsync(int userId, int restaurantId)
        {
            try
            {
                var review = await _reviewRepository.GetUserReviewForRestaurantAsync(userId, restaurantId);
                if (review == null) return null;

                var reviewDto = _mapper.Map<ReviewDto>(review);
                reviewDto.TimeAgo = CalculateTimeAgo(reviewDto.CreatedAt);
                
                return reviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reseña de usuario {UserId} para restaurante {RestaurantId}", userId, restaurantId);
                throw;
            }
        }

        public async Task<ReviewDto?> GetUserReviewForProductAsync(int userId, int productId)
        {
            try
            {
                var review = await _reviewRepository.GetUserReviewForProductAsync(userId, productId);
                if (review == null) return null;

                var reviewDto = _mapper.Map<ReviewDto>(review);
                reviewDto.TimeAgo = CalculateTimeAgo(reviewDto.CreatedAt);
                
                return reviewDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reseña de usuario {UserId} para producto {ProductId}", userId, productId);
                throw;
            }
        }

        private string CalculateTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;

            if (timeSpan.TotalDays >= 365)
                return $"Hace {(int)(timeSpan.TotalDays / 365)} año{((int)(timeSpan.TotalDays / 365) > 1 ? "s" : "")}";
            if (timeSpan.TotalDays >= 30)
                return $"Hace {(int)(timeSpan.TotalDays / 30)} mes{((int)(timeSpan.TotalDays / 30) > 1 ? "es" : "")}";
            if (timeSpan.TotalDays >= 1)
                return $"Hace {(int)timeSpan.TotalDays} día{((int)timeSpan.TotalDays > 1 ? "s" : "")}";
            if (timeSpan.TotalHours >= 1)
                return $"Hace {(int)timeSpan.TotalHours} hora{((int)timeSpan.TotalHours > 1 ? "s" : "")}";
            if (timeSpan.TotalMinutes >= 1)
                return $"Hace {(int)timeSpan.TotalMinutes} minuto{((int)timeSpan.TotalMinutes > 1 ? "s" : "")}";
            
            return "Hace un momento";
        }
    }
}
