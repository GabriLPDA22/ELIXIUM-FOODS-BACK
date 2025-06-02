using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data.EntityConfigurations
{
  public static class OrderConfiguration
  {
    public static void ConfigureOrders(ModelBuilder modelBuilder)
    {
      // Configuración de Order
      modelBuilder.Entity<Order>(entity =>
      {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Subtotal)
                  .HasColumnType("decimal(10,2)")
                  .IsRequired();

        entity.Property(e => e.DeliveryFee)
                  .HasColumnType("decimal(10,2)")
                  .IsRequired();

        entity.Property(e => e.Total)
                  .HasColumnType("decimal(10,2)")
                  .IsRequired();

        entity.Property(e => e.Status)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasDefaultValue("Pending");

        // ✅ ARREGLO: Fechas con conversión UTC
        entity.Property(e => e.EstimatedDeliveryTime)
                  .IsRequired()
                  .HasConversion(
                      v => v.ToUniversalTime(),
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        entity.Property(e => e.CreatedAt)
                  .IsRequired()
                  .HasConversion(
                      v => v.ToUniversalTime(),
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        entity.Property(e => e.UpdatedAt)
                  .IsRequired()
                  .HasConversion(
                      v => v.ToUniversalTime(),
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Relaciones
        entity.HasOne(e => e.User)
                  .WithMany(u => u.CustomerOrders)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Restaurant)
                  .WithMany(r => r.Orders)
                  .HasForeignKey(e => e.RestaurantId)
                  .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.DeliveryAddress)
                  .WithMany()
                  .HasForeignKey(e => e.DeliveryAddressId)
                  .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.DeliveryPerson)
                  .WithMany(u => u.DeliveryOrders)
                  .HasForeignKey(e => e.DeliveryPersonId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);

        // ✅ ARREGLO: Nueva relación con Payment - Order tiene PaymentId
        entity.HasOne(e => e.Payment)
                  .WithOne(p => p.Order)
                  .HasForeignKey<Order>(e => e.PaymentId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

        // Índices
        entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("IX_Order_UserId");

        entity.HasIndex(e => e.RestaurantId)
                  .HasDatabaseName("IX_Order_RestaurantId");

        entity.HasIndex(e => e.Status)
                  .HasDatabaseName("IX_Order_Status");

        entity.HasIndex(e => e.CreatedAt)
                  .HasDatabaseName("IX_Order_CreatedAt");

        entity.HasIndex(e => new { e.UserId, e.Status })
                  .HasDatabaseName("IX_Order_User_Status");

        entity.HasIndex(e => new { e.RestaurantId, e.Status })
                  .HasDatabaseName("IX_Order_Restaurant_Status");

        // ✅ ARREGLO: Nuevo índice para PaymentId
        entity.HasIndex(e => e.PaymentId)
                  .IsUnique()
                  .HasDatabaseName("IX_Order_PaymentId");
      });

      // Configuración de OrderItem
      modelBuilder.Entity<OrderItem>(entity =>
      {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Quantity)
                  .IsRequired();

        entity.Property(e => e.UnitPrice)
                  .HasColumnType("decimal(10,2)")
                  .IsRequired();

        entity.Property(e => e.Subtotal)
                  .HasColumnType("decimal(10,2)")
                  .IsRequired();

        // Relaciones
        entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

        // Índices
        entity.HasIndex(e => e.OrderId)
                  .HasDatabaseName("IX_OrderItem_OrderId");

        entity.HasIndex(e => e.ProductId)
                  .HasDatabaseName("IX_OrderItem_ProductId");
      });

      // ✅ ARREGLO: Configuración de Payment SIN OrderId
      modelBuilder.Entity<Payment>(entity =>
      {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.PaymentMethod)
                  .IsRequired()
                  .HasMaxLength(100); // ✅ Aumentado para "Visa •••• 1234"

        entity.Property(e => e.Status)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasDefaultValue("Pending");

        entity.Property(e => e.TransactionId)
                  .HasMaxLength(200);

        entity.Property(e => e.Amount)
                  .HasColumnType("decimal(10,2)")
                  .IsRequired();

        entity.Property(e => e.PaymentReference)
                  .HasMaxLength(200);

        entity.Property(e => e.FailureReason)
                  .HasMaxLength(500);

        // ✅ ARREGLO: Fechas con conversión UTC
        entity.Property(e => e.PaymentDate)
                  .IsRequired()
                  .HasConversion(
                      v => v.ToUniversalTime(),
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        entity.Property(e => e.CreatedAt)
                  .IsRequired()
                  .HasConversion(
                      v => v.ToUniversalTime(),
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        entity.Property(e => e.UpdatedAt)
                  .IsRequired()
                  .HasConversion(
                      v => v.ToUniversalTime(),
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // ✅ ARREGLO: YA NO hay relación con Order aquí
        // La relación está definida en Order con PaymentId

        // ✅ ARREGLO: Índices actualizados (sin OrderId)
        entity.HasIndex(e => e.Status)
                  .HasDatabaseName("IX_Payment_Status");

        entity.HasIndex(e => e.TransactionId)
                  .IsUnique()
                  .HasDatabaseName("IX_Payment_TransactionId");

        entity.HasIndex(e => e.PaymentDate)
                  .HasDatabaseName("IX_Payment_PaymentDate");
      });
    }
  }
}
