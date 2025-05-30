// Data/EntityConfigurations/OrderItemOfferConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
    public class OrderItemOfferConfiguration : IEntityTypeConfiguration<OrderItemOffer>
    {
        public void Configure(EntityTypeBuilder<OrderItemOffer> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.OfferName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.DiscountType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.DiscountValue)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(e => e.OriginalPrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(e => e.FinalPrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(e => e.AppliedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Relación con OrderItem
            builder.HasOne(e => e.OrderItem)
                .WithMany()
                .HasForeignKey(e => e.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(e => e.OrderItemId)
                .HasDatabaseName("IX_OrderItemOffer_OrderItemId");

            builder.HasIndex(e => e.OfferId)
                .HasDatabaseName("IX_OrderItemOffer_OfferId");

            builder.HasIndex(e => e.AppliedAt)
                .HasDatabaseName("IX_OrderItemOffer_AppliedAt");
        }
    }
}
