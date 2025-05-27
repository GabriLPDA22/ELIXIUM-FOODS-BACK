using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
  public class CategoryConfiguration : IEntityTypeConfiguration<Category>
  {
    public void Configure(EntityTypeBuilder<Category> builder)
    {
      builder.HasKey(c => c.Id);

      builder.Property(c => c.Name)
          .IsRequired()
          .HasMaxLength(100);

      builder.Property(c => c.Description)
          .HasMaxLength(500);

      // Relación con Business
      builder.HasOne(c => c.Business)
          .WithMany(b => b.Categories)
          .HasForeignKey(c => c.BusinessId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relación con Products
      builder.HasMany(c => c.Products)
          .WithOne(p => p.Category)
          .HasForeignKey(p => p.CategoryId)
          .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
