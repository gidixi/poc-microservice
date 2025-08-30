using Microsoft.EntityFrameworkCore;
using Poc.Micro.Ordering.Domain.Orders;

namespace Poc.Micro.Data.Infrastructure;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>(cfg =>
        {
            cfg.HasKey(x => x.OrderId);
            cfg.Property(x => x.CustomerId).IsRequired();
            cfg.Property(x => x.Currency).IsRequired();
            cfg.HasMany(x => x.Items).WithOne().OnDelete(DeleteBehavior.Cascade);
            cfg.HasIndex(x => x.OrderId).IsUnique();
        });

        b.Entity<OrderItem>(cfg =>
        {
            cfg.HasKey(x => x.Id);
            cfg.Property(x => x.Sku).IsRequired();
        });
    }
}
