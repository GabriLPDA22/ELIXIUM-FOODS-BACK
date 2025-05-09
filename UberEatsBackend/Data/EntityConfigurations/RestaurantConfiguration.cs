using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
  public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
  {
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
      builder.HasKey(r => r.Id);

      builder.Property(r => r.Name)
          .IsRequired()
          .HasMaxLength(100);

      builder.Property(r => r.Description)
          .IsRequired()
          .HasMaxLength(500);

      builder.Property(r => r.LogoUrl)
          .HasMaxLength(255);

      builder.Property(r => r.CoverImageUrl)
          .HasMaxLength(255);

      builder.Property(r => r.AverageRating)
          .HasDefaultValue(0);

      builder.Property(r => r.IsOpen)
          .IsRequired();

      builder.Property(r => r.DeliveryFee)
          .IsRequired()
          .HasPrecision(10, 2);

      builder.Property(r => r.EstimatedDeliveryTime)
          .IsRequired();

      builder.Property(r => r.CreatedAt)
          .IsRequired();

      builder.Property(r => r.UpdatedAt)
          .IsRequired();

      builder.Property(r => r.Tipo)
          .IsRequired()
          .HasDefaultValue(1);

      // Relaciones
      builder.HasOne(r => r.Owner)
          .WithOne(u => u.Restaurant)
          .HasForeignKey<Restaurant>(r => r.UserId)
          .OnDelete(DeleteBehavior.Restrict);

      builder.HasOne(r => r.Address)
          .WithOne(a => a.Restaurant)
          .HasForeignKey<Restaurant>(r => r.AddressId)
          .OnDelete(DeleteBehavior.Cascade);

      builder.HasMany(r => r.Menus)
          .WithOne(m => m.Restaurant)
          .HasForeignKey(m => m.RestaurantId)
          .OnDelete(DeleteBehavior.Cascade);

      builder.HasMany(r => r.Orders)
          .WithOne(o => o.Restaurant)
          .HasForeignKey(o => o.RestaurantId)
          .OnDelete(DeleteBehavior.Restrict);
    }
  }
}
