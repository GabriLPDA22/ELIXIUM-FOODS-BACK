using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.Models
{
  public class PaymentMethod
  {
    public int Id { get; set; }

    [Required]
    public string Nickname { get; set; } = string.Empty; // "Tarjeta personal", "PayPal trabajo"

    [Required]
    public PaymentType Type { get; set; }

    // Para tarjetas - SOLO almacenar últimos 4 dígitos por seguridad
    public string? LastFourDigits { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    public string? CardholderName { get; set; }

    // Para PayPal
    public string? PayPalEmail { get; set; }

    // Token de Stripe/PayPal para procesamiento real (en producción)
    public string? PaymentToken { get; set; }

    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Relaciones
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<Order> Orders { get; set; } = new List<Order>();
  }

  public enum PaymentType
  {
    Visa = 1,
    Mastercard = 2,
    PayPal = 3,
    Other = 4
  }
}
