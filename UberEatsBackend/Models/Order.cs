using System;
using System.Collections.Generic;

namespace UberEatsBackend.Models
{
  public class Order
  {
    public int Id { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Preparing, ReadyForPickup, OnTheWay, Delivered, Cancelled
    public DateTime EstimatedDeliveryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;

    public int DeliveryAddressId { get; set; }
    public Address DeliveryAddress { get; set; } = null!;

    public int? DeliveryPersonId { get; set; }
    public User? DeliveryPerson { get; set; }

    // Navigation properties
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }

    public Order()
    {
      CreatedAt = DateTime.UtcNow;
      UpdatedAt = DateTime.UtcNow;
    }
  }
}
