// UberEatsBackend/Data/EntityConfigurations/ReviewConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);

            // Propiedades básicas
            builder.Property(r => r.Rating)
                .IsRequired()
                .HasAnnotation("Range", new int[] { 1, 5 }); // 1-5 estrellas

            builder.Property(r => r.Comment)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.ImageUrl)
                .HasMaxLength(500);

            builder.Property(r => r.IsVerifiedPurchase)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(r => r.IsHelpful)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(r => r.HelpfulCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.UpdatedAt)
                .IsRequired();

            // Relaciones
            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Restaurant)
                .WithMany()
                .HasForeignKey(r => r.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para optimización
            builder.HasIndex(r => r.RestaurantId)
                .HasDatabaseName("IX_Review_RestaurantId");

            builder.HasIndex(r => r.ProductId)
                .HasDatabaseName("IX_Review_ProductId");

            builder.HasIndex(r => r.UserId)
                .HasDatabaseName("IX_Review_UserId");

            builder.HasIndex(r => r.Rating)
                .HasDatabaseName("IX_Review_Rating");

            builder.HasIndex(r => r.CreatedAt)
                .HasDatabaseName("IX_Review_CreatedAt");

            builder.HasIndex(r => r.IsActive)
                .HasDatabaseName("IX_Review_IsActive");

            // Índice único para evitar múltiples reseñas del mismo usuario
            builder.HasIndex(r => new { r.UserId, r.RestaurantId })
                .IsUnique()
                .HasDatabaseName("IX_Review_User_Restaurant")
                .HasFilter("\"ProductId\" IS NULL AND \"IsActive\" = true");

            builder.HasIndex(r => new { r.UserId, r.ProductId })
                .IsUnique()
                .HasDatabaseName("IX_Review_User_Product")
                .HasFilter("\"ProductId\" IS NOT NULL AND \"IsActive\" = true");

            // Índices compuestos para consultas comunes
            builder.HasIndex(r => new { r.RestaurantId, r.IsActive, r.Rating })
                .HasDatabaseName("IX_Review_Restaurant_Active_Rating");

            builder.HasIndex(r => new { r.ProductId, r.IsActive, r.Rating })
                .HasDatabaseName("IX_Review_Product_Active_Rating");

            builder.HasIndex(r => new { r.RestaurantId, r.IsActive, r.CreatedAt })
                .HasDatabaseName("IX_Review_Restaurant_Active_Date");

            builder.HasIndex(r => new { r.IsVerifiedPurchase, r.IsActive })
                .HasDatabaseName("IX_Review_Verified_Active");
        }
    }
}
