using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
  public class UserConfiguration : IEntityTypeConfiguration<User>
  {
    public void Configure(EntityTypeBuilder<User> builder)
    {
      builder.HasKey(u => u.Id);

      builder.Property(u => u.Email)
          .IsRequired()
          .HasMaxLength(100);

      builder.HasIndex(u => u.Email)
          .IsUnique();

      builder.Property(u => u.PasswordHash)
          .IsRequired();

      builder.Property(u => u.FirstName)
          .IsRequired()
          .HasMaxLength(50);

      builder.Property(u => u.LastName)
          .IsRequired()
          .HasMaxLength(50);

      builder.Property(u => u.PhoneNumber)
          .HasMaxLength(20);

      builder.Property(u => u.Role)
          .IsRequired()
          .HasMaxLength(20)
          .HasDefaultValue("Customer");

      builder.Property(u => u.CreatedAt)
          .IsRequired();

      builder.Property(u => u.UpdatedAt)
          .IsRequired();

      // Nuevos campos para el perfil
      builder.Property(u => u.Birthdate)
          .IsRequired(false);

      builder.Property(u => u.Bio)
          .IsRequired(false);

      builder.Property(u => u.DietaryPreferencesJson)
          .IsRequired(false);

      builder.Property(u => u.PhotoURL)
          .IsRequired(false)
          .HasMaxLength(255);

      // Relaciones
      builder.HasMany(u => u.Addresses)
          .WithOne(a => a.User)
          .HasForeignKey(a => a.UserId)
          .OnDelete(DeleteBehavior.Cascade);

      builder.HasOne(u => u.Restaurant)
          .WithOne(r => r.Owner)
          .HasForeignKey<Restaurant>(r => r.UserId)
          .OnDelete(DeleteBehavior.Restrict);
    }
  }
}
