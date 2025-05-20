// BusinessConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
  public class BusinessConfiguration : IEntityTypeConfiguration<Business>
  {
    public void Configure(EntityTypeBuilder<Business> builder)
    {
      builder.HasKey(b => b.Id);

      builder.Property(b => b.Name)
          .IsRequired()
          .HasMaxLength(100);

      builder.Property(b => b.Description)
          .IsRequired()
          .HasMaxLength(500);

      builder.Property(b => b.LogoUrl)
          .HasMaxLength(255);

      builder.Property(b => b.ContactEmail)
          .IsRequired()
          .HasMaxLength(100);

      builder.Property(b => b.ContactPhone)
          .HasMaxLength(20);

      builder.Property(b => b.TaxId)
          .HasMaxLength(50);

      builder.Property(b => b.BusinessType)
          .IsRequired()
          .HasMaxLength(50)
          .HasDefaultValue("Restaurant");

      builder.Property(b => b.IsActive)
          .IsRequired()
          .HasDefaultValue(true);

      builder.Property(b => b.CreatedAt)
          .IsRequired();

      builder.Property(b => b.UpdatedAt)
          .IsRequired();

      // Relationship with Restaurants is configured in RestaurantConfiguration
    }
  }
}
