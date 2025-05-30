using System;
using System.ComponentModel.DataAnnotations;

namespace UberEatsBackend.DTOs.Offers
{
  public class UpdateProductOfferDto
  {
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? MinimumQuantity { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public string? Status { get; set; }
  }
}
