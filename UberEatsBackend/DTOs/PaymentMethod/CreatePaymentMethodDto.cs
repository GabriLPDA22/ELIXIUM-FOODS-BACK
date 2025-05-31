// UberEatsBackend/DTOs/PaymentMethod/CreatePaymentMethodDto.cs
using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.DTOs.PaymentMethod
{
    public class CreatePaymentMethodDto
    {
        [Required]
        [StringLength(100)]
        public string Nickname { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty; // "visa", "mastercard", "paypal", "other"

        // Campos para tarjetas (se procesan pero NO se almacenan completos)
        public string? CardNumber { get; set; } // Solo para procesamiento, extraemos últimos 4
        public string? ExpiryDate { get; set; } // MM/YY format
        public string? CVV { get; set; } // NUNCA se almacena
        public string? CardholderName { get; set; }

        // Para PayPal
        [EmailAddress]
        public string? PayPalEmail { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}
