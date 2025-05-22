using System.Collections.Generic;

namespace UberEatsBackend.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Ahora pertenece directamente al Business
        public int BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        // Navigation properties
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
