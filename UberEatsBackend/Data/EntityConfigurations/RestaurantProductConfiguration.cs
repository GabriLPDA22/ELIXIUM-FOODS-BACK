using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
  public class RestaurantProductConfiguration : IEntityTypeConfiguration<RestaurantProduct>
  {
    public void Configure(EntityTypeBuilder<RestaurantProduct> builder)
    {
      builder.HasKey(rp => rp.Id);

      // Índice único para evitar duplicados Restaurant-Product
      builder.HasIndex(rp => new { rp.RestaurantId, rp.ProductId })
          .IsUnique()
          .HasDatabaseName("IX_RestaurantProduct_Restaurant_Product");

      builder.Property(rp => rp.Price)
          .IsRequired()
          .HasPrecision(10, 2);

      builder.Property(rp => rp.IsAvailable)
          .IsRequired()
          .HasDefaultValue(true);

      builder.Property(rp => rp.StockQuantity)
          .HasDefaultValue(0);

      builder.Property(rp => rp.Notes)
          .HasMaxLength(500);

      builder.Property(rp => rp.CreatedAt)
          .IsRequired();

      builder.Property(rp => rp.UpdatedAt)
          .IsRequired();

      // Relación con Restaurant
      builder.HasOne(rp => rp.Restaurant)
          .WithMany(r => r.RestaurantProducts)
          .HasForeignKey(rp => rp.RestaurantId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relación con Product
      builder.HasOne(rp => rp.Product)
          .WithMany(p => p.RestaurantProducts)
          .HasForeignKey(rp => rp.ProductId)
          .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
