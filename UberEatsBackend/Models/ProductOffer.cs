using System;

namespace UberEatsBackend.Models
{
    public class ProductOffer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Tipo de descuento: "percentage" o "fixed"
        public string DiscountType { get; set; } = "percentage";
        public decimal DiscountValue { get; set; }

        // Condiciones para aplicar la oferta
        public decimal MinimumOrderAmount { get; set; } = 0;
        public int MinimumQuantity { get; set; } = 1;

        // Fechas de validez
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Límites de uso
        public int UsageLimit { get; set; } = 0; // 0 = sin límite
        public int UsageCount { get; set; } = 0;

        // Estado: "active", "inactive", "expired"
        public string Status { get; set; } = "active";

        // Relaciones
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Campos de auditoría
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

        public ProductOffer()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Métodos de validación
        public bool IsActive()
        {
            var now = DateTime.UtcNow;
            return Status == "active" &&
                   StartDate <= now &&
                   EndDate >= now &&
                   (UsageLimit == 0 || UsageCount < UsageLimit);
        }

        public decimal CalculateDiscount(decimal originalPrice, int quantity)
        {
            if (!IsActive()) return 0;

            if (quantity < MinimumQuantity) return 0;

            if (DiscountType == "percentage")
            {
                return originalPrice * (DiscountValue / 100);
            }
            else // fixed
            {
                return Math.Min(DiscountValue, originalPrice);
            }
        }
    }

    // Modelo para registrar ofertas aplicadas en pedidos
    public class OrderItemOffer
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = null!;
        public int OfferId { get; set; }
        public string OfferName { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }
}
