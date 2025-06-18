using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Models
{
    public class ITIContext : IdentityDbContext<ApplicationUser>
    {
        public ITIContext(DbContextOptions<ITIContext> options) : base(options) { }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<DeliveryBoy> DeliveryBoys { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Subscribe> Subscribes { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<Bazar> Bazars { get; set; }
        public DbSet<BazarBrand> BazarBrands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Custom Fluent configurations can go here
        }
    }
}
