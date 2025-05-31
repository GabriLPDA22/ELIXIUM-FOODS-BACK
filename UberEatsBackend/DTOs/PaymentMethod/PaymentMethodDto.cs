// UberEatsBackend/DTOs/PaymentMethod/PaymentMethodDto.cs - DEFINITIVO
namespace UberEatsBackend.DTOs.PaymentMethod
{
    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? LastFourDigits { get; set; }
        public int? ExpiryMonth { get; set; }
        public int? ExpiryYear { get; set; }
        public string? CardholderName { get; set; }
        public string? PayPalEmail { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
