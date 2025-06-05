// UberEatsBackend/DTOs/PaymentMethod/UpdatePaymentMethodDto.cs - DEFINITIVO
namespace UberEatsBackend.DTOs.PaymentMethod
{
    public class UpdatePaymentMethodDto
    {
        public string Nickname { get; set; } = string.Empty;
        public string? CardholderName { get; set; }
        public string? PayPalEmail { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}
