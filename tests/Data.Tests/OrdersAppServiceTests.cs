using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Poc.Micro.Data.Infrastructure;
using Poc.Micro.Ordering.Application.Orders;
using Poc.Micro.Ordering.Domain.V1;
using Poc.Micro.Persistence.EntityFramework;
using DomainOrder = Poc.Micro.Ordering.Domain.Orders.Order;
using Xunit;

namespace Data.Tests;

public class OrdersAppServiceTests
{
    [Fact]
    public async Task SavePricedOrderAsync_PersistsOrder()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new OrderDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var repo = new Repository<DomainOrder>(db);
        var uow = new EfUnitOfWork(db);
        var svc = new OrdersAppService(repo, uow);

        var orderId = Guid.NewGuid();
        var dto = new PricedOrder
        {
            Order = new Order
            {
                OrderId = new Uuid { Value = orderId.ToString() },
                CustomerId = "c1",
                Items = { new OrderItem { Sku = "A", Qty = new Quantity { Value = 1 }, UnitPrice = new Money { Amount = 5, Currency = "EUR" } } }
            },
            Subtotal = new Money { Amount = 5, Currency = "EUR" },
            Tax = new Money { Amount = 1, Currency = "EUR" },
            Total = new Money { Amount = 6, Currency = "EUR" }
        };

        await svc.SavePricedOrderAsync(dto, CancellationToken.None);

        var saved = await db.Orders.Include(o => o.Items).SingleOrDefaultAsync(o => o.OrderId == orderId);
        Assert.NotNull(saved);
        Assert.Single(saved!.Items);
    }

    [Fact]
    public async Task ListPricedOrdersAsync_ReturnsSavedOrders()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new OrderDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var repo = new Repository<DomainOrder>(db);
        var uow = new EfUnitOfWork(db);
        var svc = new OrdersAppService(repo, uow);

        var orderId = Guid.NewGuid();
        var dto = new PricedOrder
        {
            Order = new Order
            {
                OrderId = new Uuid { Value = orderId.ToString() },
                CustomerId = "c1",
                Items = { new OrderItem { Sku = "A", Qty = new Quantity { Value = 1 }, UnitPrice = new Money { Amount = 5, Currency = "EUR" } } }
            },
            Subtotal = new Money { Amount = 5, Currency = "EUR" },
            Tax = new Money { Amount = 1, Currency = "EUR" },
            Total = new Money { Amount = 6, Currency = "EUR" }
        };

        await svc.SavePricedOrderAsync(dto, CancellationToken.None);

        var list = await svc.ListPricedOrdersAsync(CancellationToken.None);

        Assert.Single(list);
        Assert.Equal(orderId.ToString(), list[0].Order.OrderId.Value);
    }
}
