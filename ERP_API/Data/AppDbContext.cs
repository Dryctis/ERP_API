using ERP_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();


    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        b.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>(e =>
        {
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
            e.HasMany(x => x.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId);
        });

        b.Entity<OrderItem>(e =>
        {
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        b.Entity<InventoryMovement>(e =>
        {
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        b.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });

        b.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        b.Entity<Role>()
            .HasIndex(x => x.Name)
            .IsUnique();

        b.Entity<RefreshToken>()
            .HasIndex(x => x.Token)
            .IsUnique();

        b.Entity<Customer>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
        });
    }
}
