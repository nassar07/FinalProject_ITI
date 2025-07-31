using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Models;

public class ITIContext : IdentityDbContext<ApplicationUser>
{
    public ITIContext(DbContextOptions<ITIContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Subscribe> Subscribes { get; set; }
    public DbSet<Bazar> Bazars { get; set; }
    public DbSet<BazarBrand> BazarBrands  { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<OrderType> OrderTypes { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Subscribe>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(s => s.TotalAmount)
             .HasPrecision(18, 2);

        modelBuilder.Entity<OrderDetail>()
           .Property(s => s.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
          .Property(s => s.Price)
           .HasPrecision(18, 2);

        modelBuilder.Entity<Subscribe>()
          .Property(s => s.Price)
           .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
           .HasOne(o => o.DeliveryBoy)
           .WithMany(u => u.AssignedOrders)
           .HasForeignKey(o => o.DeliveryBoyID)
           .OnDelete(DeleteBehavior.SetNull);

        // BazarBrand (Many-to-Many between Bazar and Brand)
        modelBuilder.Entity<BazarBrand>()
            .HasKey(bb => bb.Id); // Explicit key, not composite

        modelBuilder.Entity<BazarBrand>()
            .HasIndex(bb => new { bb.BazarID, bb.BrandID })
            .IsUnique();

        modelBuilder.Entity<BazarBrand>()
            .HasOne(bb => bb.Bazar)
            .WithMany(b => b.BazarBrands)
            .HasForeignKey(bb => bb.BazarID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BazarBrand>()
            .HasOne(bb => bb.Brand)
            .WithMany(b => b.BazarBrands)
            .HasForeignKey(bb => bb.BrandID)
            .OnDelete(DeleteBehavior.Cascade);

        // Brand
        modelBuilder.Entity<Brand>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Brands)
            .HasForeignKey(b => b.CategoryID);

        modelBuilder.Entity<Brand>()
            .HasOne(b => b.Owner)
            .WithMany()
            .HasForeignKey(b => b.OwnerID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Brand>()
            .HasOne(b => b.Subscribe)
            .WithMany(s => s.Brands)
            .HasForeignKey(b => b.SubscribeID);

        // Order
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.OrderType)
            .WithMany(ot => ot.Orders)
            .HasForeignKey(o => o.OrderTypeID);

        // OrderDetail
        modelBuilder.Entity<OrderDetail>()
         .HasOne(od => od.Order)
         .WithMany(o => o.OrderDetails)
         .HasForeignKey(od => od.OrderID)
         .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDetail>()
        .HasOne(od => od.Product)
        .WithMany(p => p.OrderDetails)
        .HasForeignKey(od => od.ProductID)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderDetail>()
        .HasOne(od => od.Brand)
        .WithMany(b => b.OrderDetails)
        .HasForeignKey(od => od.BrandID)
        .OnDelete(DeleteBehavior.Restrict);

        // Payment (1-1 with Order)
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderID);

        modelBuilder.Entity<Product>()
           .HasOne(p => p.Brand)
           .WithMany(b => b.Products) // ✅ Correct reference
           .HasForeignKey(p => p.BrandID);

        // Review
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserID);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductID);
    }

}

