// UberEatsBackend/DTOs/PaymentMethod/UpdatePaymentMethodDto.cs
using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.DTOs.PaymentMethod
{
    public class UpdatePaymentMethodDto
    {
        [Required]
        [StringLength(100)]
        public string Nickname { get; set; } = string.Empty;

        public string? CardholderName { get; set; }
        public string? PayPalEmail { get; set; }
        public bool IsDefault { get; set; } = false;

        // NOTA: No permitimos cambiar número de tarjeta o tipo por seguridad
        // Si el usuario quiere cambiar la tarjeta, debe crear una nueva y eliminar la anterior
    }
}
