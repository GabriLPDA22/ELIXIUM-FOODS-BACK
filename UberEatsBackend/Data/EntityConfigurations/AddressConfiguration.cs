using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
  public class AddressConfiguration : IEntityTypeConfiguration<Address>
  {
    public void Configure(EntityTypeBuilder<Address> builder)
    {
      builder.HasKey(a => a.Id);

      builder.Property(a => a.Street)
          .IsRequired()
          .HasMaxLength(255);

      builder.Property(a => a.City)
          .IsRequired()
          .HasMaxLength(100);

      builder.Property(a => a.State)
          .IsRequired()
          .HasMaxLength(100);

      builder.Property(a => a.ZipCode)
          .IsRequired()
          .HasMaxLength(20);

      // Nuevos campos
      builder.Property(a => a.Name)
          .HasMaxLength(100);

      builder.Property(a => a.Number)
          .HasMaxLength(20);

      builder.Property(a => a.Interior)
          .HasMaxLength(20);

      builder.Property(a => a.Neighborhood)
          .HasMaxLength(100);

      builder.Property(a => a.Phone)
          .HasMaxLength(20);

      builder.Property(a => a.IsDefault)
          .IsRequired()
          .HasDefaultValue(false);

      // Relaciones
      builder.HasOne(a => a.User)
          .WithMany(u => u.Addresses)
          .HasForeignKey(a => a.UserId)
          .OnDelete(DeleteBehavior.Cascade);

      builder.HasMany(a => a.Orders)
          .WithOne(o => o.DeliveryAddress)
          .HasForeignKey(o => o.DeliveryAddressId);
    }
  }
}
