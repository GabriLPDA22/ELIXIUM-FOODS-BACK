// UberEatsBackend/Models/Review.cs
using System;

namespace UberEatsBackend.Models
{
    public class Review
    {
        public int Id { get; set; }
        
        // Relaciones
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;
        
        // Producto específico (opcional - para reseñas de productos)
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        
        // Contenido de la reseña
        public int Rating { get; set; } // 1-5 estrellas
        public string Comment { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } // Imagen opcional de la reseña
        
        // Metadata
        public bool IsVerifiedPurchase { get; set; } = false; // Si compró el producto/visitó restaurante
        public bool IsHelpful { get; set; } = false;
        public int HelpfulCount { get; set; } = 0;
        public bool IsActive { get; set; } = true; // Para soft delete
        
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => _createdAt = value.Kind == DateTimeKind.Unspecified ?
                DateTime.SpecifyKind(value, DateTimeKind.Utc) :
                value.ToUniversalTime();
        }

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => _updatedAt = value.Kind == DateTimeKind.Unspecified ?
                DateTime.SpecifyKind(value, DateTimeKind.Utc) :
                value.ToUniversalTime();
        }

        public Review()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}