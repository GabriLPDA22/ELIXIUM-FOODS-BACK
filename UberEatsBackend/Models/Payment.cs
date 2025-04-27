using System;

namespace UberEatsBackend.Models
{
  public class Payment
  {
    public int Id { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }

    // Relaciones
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
  }
}
