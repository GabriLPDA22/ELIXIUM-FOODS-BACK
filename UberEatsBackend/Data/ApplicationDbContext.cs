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
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<RestaurantProduct> RestaurantProducts { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Business> Businesses { get; set; } = null!;
    public DbSet<Promotion> Promotions { get; set; } = null!;
    public DbSet<BusinessHour> BusinessHours { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Apply configurations
      modelBuilder.ApplyConfiguration(new UserConfiguration());
      modelBuilder.ApplyConfiguration(new AddressConfiguration());
      modelBuilder.ApplyConfiguration(new RestaurantConfiguration());
      modelBuilder.ApplyConfiguration(new BusinessConfiguration());
      modelBuilder.ApplyConfiguration(new CategoryConfiguration());
      modelBuilder.ApplyConfiguration(new ProductConfiguration());
      modelBuilder.ApplyConfiguration(new RestaurantProductConfiguration());

      // Configure Order and User (relationship with DeliveryPerson)
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

      // Configure Address relationships
      modelBuilder.Entity<Address>()
          .HasOne(a => a.User)
          .WithMany(u => u.Addresses)
          .HasForeignKey(a => a.UserId)
          .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
