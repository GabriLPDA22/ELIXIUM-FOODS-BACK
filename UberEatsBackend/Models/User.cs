using System;
using System.Collections.Generic;

namespace UberEatsBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer"; // Admin, Customer, Restaurant, DeliveryPerson
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navegación
        public List<Address> Addresses { get; set; } = new List<Address>();
        public Restaurant? Restaurant { get; set; }
        public List<Order> CustomerOrders { get; set; } = new List<Order>();
        public List<Order> DeliveryOrders { get; set; } = new List<Order>();

        public User()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public string FullName => $"{FirstName} {LastName}";
    }
}
