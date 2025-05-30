using System;
using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.DTOs.Offers
{
  public class OfferValidationDto
  {
    public int OfferId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal OrderSubtotal { get; set; }
    public bool CanApply { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
  }
}
