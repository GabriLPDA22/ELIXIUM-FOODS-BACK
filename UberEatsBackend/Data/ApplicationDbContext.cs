// UberEatsBackend/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using UberEatsBackend.Data.EntityConfigurations;
using UberEatsBackend.Models;

namespace UberEatsBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Address> Addresses { get; set; } = null!;
        public DbSet<Restaurant> Restaurants { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<RestaurantProduct> RestaurantProducts { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Business> Businesses { get; set; } = null!;
        public DbSet<BusinessHour> BusinessHours { get; set; } = null!;
        public DbSet<ProductOffer> ProductOffers { get; set; } = null!;
        public DbSet<OrderItemOffer> OrderItemOffers { get; set; } = null!;

        // ===== NUEVA TABLA PARA MÉTODOS DE PAGO =====
        public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;

        // ===== NUEVA TABLA PARA HORARIOS DE RESTAURANTES =====
        public DbSet<RestaurantHour> RestaurantHours { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply existing configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new AddressConfiguration());
            modelBuilder.ApplyConfiguration(new RestaurantConfiguration());
            modelBuilder.ApplyConfiguration(new BusinessConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new RestaurantProductConfiguration());

            // Configure Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.DeliveryPerson)
                .WithMany(u => u.DeliveryOrders)
                .HasForeignKey(o => o.DeliveryPersonId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.CustomerOrders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Address relationships
            modelBuilder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== CONFIGURACIÓN PARA RESTAURANT HOURS =====
            modelBuilder.Entity<RestaurantHour>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DayOfWeek)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.IsOpen)
                    .HasDefaultValue(true);

                entity.Property(e => e.OpenTime)
                    .IsRequired();

                entity.Property(e => e.CloseTime)
                    .IsRequired();

                // Relación con Restaurant
                entity.HasOne(e => e.Restaurant)
                    .WithMany()
                    .HasForeignKey(e => e.RestaurantId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices para optimización
                entity.HasIndex(e => e.RestaurantId)
                    .HasDatabaseName("IX_RestaurantHour_RestaurantId");

                entity.HasIndex(e => new { e.RestaurantId, e.DayOfWeek })
                    .IsUnique()
                    .HasDatabaseName("IX_RestaurantHour_Restaurant_Day");

                entity.HasIndex(e => e.DayOfWeek)
                    .HasDatabaseName("IX_RestaurantHour_DayOfWeek");
            });

            // ===== CONFIGURACIÓN PARA PAYMENT METHODS =====
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nickname)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasConversion<string>(); // Almacenar enum como string

                entity.Property(e => e.LastFourDigits)
                    .HasMaxLength(4);

                entity.Property(e => e.CardholderName)
                    .HasMaxLength(100);

                entity.Property(e => e.PayPalEmail)
                    .HasMaxLength(255);

                entity.Property(e => e.PaymentToken)
                    .HasMaxLength(500);

                entity.Property(e => e.IsDefault)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .IsRequired();

                // Relación con User
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices para optimización
                entity.HasIndex(e => new { e.UserId, e.IsActive })
                    .HasDatabaseName("IX_PaymentMethod_User_Active");

                entity.HasIndex(e => new { e.UserId, e.IsDefault })
                    .HasDatabaseName("IX_PaymentMethod_User_Default");

                entity.HasIndex(e => e.Type)
                    .HasDatabaseName("IX_PaymentMethod_Type");
            });

            // ===== CONFIGURACIONES EXISTENTES PARA PRODUCT OFFERS =====
            modelBuilder.Entity<ProductOffer>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.DiscountType)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("percentage");

                entity.Property(e => e.DiscountValue)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.MinimumOrderAmount)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.MinimumQuantity)
                    .HasDefaultValue(1);

                entity.Property(e => e.StartDate)
                    .IsRequired();

                entity.Property(e => e.EndDate)
                    .IsRequired();

                entity.Property(e => e.UsageLimit)
                    .HasDefaultValue(0);

                entity.Property(e => e.UsageCount)
                    .HasDefaultValue(0);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("active");

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .IsRequired();

                // Relaciones
                entity.HasOne(e => e.Restaurant)
                    .WithMany()
                    .HasForeignKey(e => e.RestaurantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(e => e.RestaurantId)
                    .HasDatabaseName("IX_ProductOffer_RestaurantId");

                entity.HasIndex(e => e.ProductId)
                    .HasDatabaseName("IX_ProductOffer_ProductId");

                entity.HasIndex(e => new { e.RestaurantId, e.ProductId })
                    .HasDatabaseName("IX_ProductOffer_Restaurant_Product");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_ProductOffer_Status");

                entity.HasIndex(e => new { e.StartDate, e.EndDate })
                    .HasDatabaseName("IX_ProductOffer_DateRange");

                entity.HasIndex(e => new { e.Status, e.StartDate, e.EndDate, e.RestaurantId })
                    .HasDatabaseName("IX_ProductOffer_ActiveOffers")
                    .HasFilter("\"Status\" = 'active'");
            });

            // Configuración para OrderItemOffer
            modelBuilder.Entity<OrderItemOffer>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.OfferName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.DiscountType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.DiscountValue)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.DiscountAmount)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.OriginalPrice)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.FinalPrice)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(e => e.AppliedAt)
                    .IsRequired()
                    .HasDefaultValueSql("NOW()");

                // Relación con OrderItem
                entity.HasOne(e => e.OrderItem)
                    .WithMany()
                    .HasForeignKey(e => e.OrderItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(e => e.OrderItemId)
                    .HasDatabaseName("IX_OrderItemOffer_OrderItemId");

                entity.HasIndex(e => e.OfferId)
                    .HasDatabaseName("IX_OrderItemOffer_OfferId");

                entity.HasIndex(e => e.AppliedAt)
                    .HasDatabaseName("IX_OrderItemOffer_AppliedAt");
            });
        }
    }
}
