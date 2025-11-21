using ERP_API.Common.Entities;
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
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ProductSupplier> ProductSuppliers => Set<ProductSupplier>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();



    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        b.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {

        b.Entity<Order>(e =>
        {
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
            e.HasMany(x => x.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Restrict); 
        });

        b.Entity<OrderItem>(e =>
        {
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict); 
        });

        b.Entity<InventoryMovement>(e =>
        {
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => new { x.ProductId, x.CreatedAt });
        });

        b.Entity<Invoice>(e =>
        {
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
            e.HasMany(x => x.Items).WithOne(i => i.Invoice).HasForeignKey(i => i.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict); 
            e.HasMany(x => x.Payments).WithOne(p => p.Invoice).HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict); 
        });

        b.Entity<InvoiceItem>(e =>
        {
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.InvoiceId);
            e.HasIndex(x => x.ProductId);
        });

        b.Entity<InvoicePayment>(e =>
        {
            e.HasIndex(x => x.InvoiceId);
            e.HasIndex(x => x.PaymentDate);
        });

        b.Entity<PurchaseOrder>(e =>
        {
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId);
            e.HasMany(x => x.Items).WithOne(i => i.PurchaseOrder).HasForeignKey(i => i.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<PurchaseOrderItem>(e =>
        {
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict); 
            e.HasIndex(x => x.PurchaseOrderId);
            e.HasIndex(x => x.ProductId);
        });

        b.Entity<ProductSupplier>(e =>
        {
            e.HasOne(x => x.Product).WithMany(p => p.ProductSuppliers).HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade); 
            e.HasOne(x => x.Supplier).WithMany(s => s.ProductSuppliers).HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Cascade); 
            e.HasIndex(x => new { x.ProductId, x.SupplierId }).IsUnique();
            e.HasIndex(x => x.IsPreferred);
        });

       

        b.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });

        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        b.Entity<RefreshToken>().HasIndex(x => x.Token).IsUnique();

        b.Entity<Customer>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Name);
        });

        b.Entity<Product>(e =>
        {
            e.HasIndex(x => x.Sku);
            e.HasIndex(x => x.Name);
            e.HasIndex(x => x.Stock);
        });

        b.Entity<Supplier>(e =>
        {
            e.HasIndex(x => x.Email);
            e.HasIndex(x => x.Name);
            e.HasIndex(x => x.TaxId);
            e.HasIndex(x => x.IsActive);
        });

        b.Entity<Invoice>(e =>
        {
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.HasIndex(x => x.CustomerId);
            e.HasIndex(x => x.OrderId);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.DueDate);
            e.HasIndex(x => x.IssueDate);
            e.HasIndex(x => new { x.Status, x.DueDate });
        });

        b.Entity<PurchaseOrder>(e =>
        {
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.HasIndex(x => x.SupplierId);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.OrderDate);
            e.HasIndex(x => x.ExpectedDeliveryDate);
            e.HasIndex(x => new { x.Status, x.ExpectedDeliveryDate });
        });

    
        ConfigureSoftDeleteFilters(b);
    }


    private void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Not(property),
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override int SaveChanges()
    {
        HandleSoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void HandleSoftDelete()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted && e.Entity is ISoftDeletable);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;

            var entity = (ISoftDeletable)entry.Entity;
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
        }
    }
}