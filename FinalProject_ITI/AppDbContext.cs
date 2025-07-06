using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
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
    public DbSet<DeliveryBoy> deliveryBoys { get; set; }
    public DbSet<OrderType> OrderTypes { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}

