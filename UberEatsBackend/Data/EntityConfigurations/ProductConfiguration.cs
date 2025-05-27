using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
  public class ProductConfiguration : IEntityTypeConfiguration<Product>
  {
    public void Configure(EntityTypeBuilder<Product> builder)
    {
      builder.HasKey(p => p.Id);

      builder.Property(p => p.Name)
          .IsRequired()
          .HasMaxLength(100);

      builder.Property(p => p.Description)
          .HasMaxLength(500);

      builder.Property(p => p.BasePrice)
          .IsRequired()
          .HasPrecision(10, 2);

      builder.Property(p => p.ImageUrl)
          .HasMaxLength(255);

      builder.Property(p => p.IsAvailable)
          .IsRequired()
          .HasDefaultValue(true);

      // Relación con Category
      builder.HasOne(p => p.Category)
          .WithMany(c => c.Products)
          .HasForeignKey(p => p.CategoryId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relación con RestaurantProducts
      builder.HasMany(p => p.RestaurantProducts)
          .WithOne(rp => rp.Product)
          .HasForeignKey(rp => rp.ProductId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relación con OrderItems
      builder.HasMany(p => p.OrderItems)
          .WithOne(oi => oi.Product)
          .HasForeignKey(oi => oi.ProductId)
          .OnDelete(DeleteBehavior.Restrict);
    }
  }
}
