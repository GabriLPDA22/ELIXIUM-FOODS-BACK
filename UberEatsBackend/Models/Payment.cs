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

    private DateTime _paymentDate;

    public DateTime PaymentDate
    {
        get => _paymentDate;
        set => _paymentDate = value.Kind == DateTimeKind.Unspecified ?
            DateTime.SpecifyKind(value, DateTimeKind.Utc) :
            value.ToUniversalTime();
    }

    // Relaciones
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
  }
}
