namespace UberEatsBackend.DTOs.PaymentMethod
{
    public class CreatePaymentMethodDto
    {
        public string Nickname { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        // Campos para tarjetas (opcionales)
        public string? CardNumber { get; set; }
        public string? ExpiryDate { get; set; }
        public string? CVV { get; set; }
        public string? CardholderName { get; set; }

        // Para PayPal (opcional)
        public string? PayPalEmail { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}
