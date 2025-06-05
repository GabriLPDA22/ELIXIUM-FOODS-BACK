// Data/EntityConfigurations/ProductOfferConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
    public class ProductOfferConfiguration : IEntityTypeConfiguration<ProductOffer>
    {
        public void Configure(EntityTypeBuilder<ProductOffer> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .HasMaxLength(1000);

            builder.Property(e => e.DiscountType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("percentage");

            builder.Property(e => e.DiscountValue)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(e => e.MinimumOrderAmount)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);

            builder.Property(e => e.MinimumQuantity)
                .HasDefaultValue(1);

            builder.Property(e => e.StartDate)
                .IsRequired();

            builder.Property(e => e.EndDate)
                .IsRequired();

            builder.Property(e => e.UsageLimit)
                .HasDefaultValue(0);

            builder.Property(e => e.UsageCount)
                .HasDefaultValue(0);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("active");

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .IsRequired();

            // Relaciones
            builder.HasOne(e => e.Restaurant)
                .WithMany()
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(e => e.RestaurantId)
                .HasDatabaseName("IX_ProductOffer_RestaurantId");

            builder.HasIndex(e => e.ProductId)
                .HasDatabaseName("IX_ProductOffer_ProductId");

            builder.HasIndex(e => new { e.RestaurantId, e.ProductId })
                .HasDatabaseName("IX_ProductOffer_Restaurant_Product");

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("IX_ProductOffer_Status");

            builder.HasIndex(e => new { e.StartDate, e.EndDate })
                .HasDatabaseName("IX_ProductOffer_DateRange");

            // Índice compuesto para consultas de ofertas activas
            builder.HasIndex(e => new { e.Status, e.StartDate, e.EndDate, e.RestaurantId })
                .HasDatabaseName("IX_ProductOffer_ActiveOffers")
                .HasFilter("\"Status\" = 'active'");
        }
    }
}
