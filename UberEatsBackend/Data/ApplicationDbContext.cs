using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Data.EntityConfigurations;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<Restaurant> Restaurants { get; set; } = null!;
    public DbSet<Menu> Menus { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Aplicar configuraciones
      modelBuilder.ApplyConfiguration(new UserConfiguration());

      // Configuración para Order y User (relación con DeliveryPerson)
      modelBuilder.Entity<Order>()
          .HasOne(o => o.DeliveryPerson)
          .WithMany(u => u.DeliveryOrders)
          .HasForeignKey(o => o.DeliveryPersonId)
          .IsRequired(false)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Order>()
          .HasOne(o => o.User)
          .WithMany(u => u.CustomerOrders)
          .HasForeignKey(o => o.UserId)
          .OnDelete(DeleteBehavior.Restrict);

      // Otras configuraciones globales si son necesarias
    }
  }
}
