using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Testing;
using Poc.Micro.Inventory.Api;
using Poc.Micro.Ordering.Domain.V1;
using Xunit;

namespace Inventory.Tests;

public class InventoryServiceTests
{
    [Fact]
    public async Task Reserve_ReturnsTrue_WhenStockAvailable()
    {
        var svc = new InventoryService();
        var order = new Order
        {
            Items = { new OrderItem { Sku = "A", Qty = new Quantity { Value = 5 }, UnitPrice = new Money { Amount = 1, Currency = "EUR" } } }
        };

        var result = await svc.Reserve(order, CreateContext());

        Assert.True(result.Reserved);
        Assert.Equal(string.Empty, result.Reason);
    }

    [Fact]
    public async Task Reserve_ReturnsFalse_WhenStockInsufficient()
    {
        var svc = new InventoryService();
        var order = new Order
        {
            Items = { new OrderItem { Sku = "B", Qty = new Quantity { Value = 200 }, UnitPrice = new Money { Amount = 1, Currency = "EUR" } } }
        };

        var result = await svc.Reserve(order, CreateContext());

        Assert.False(result.Reserved);
        Assert.Contains("insufficient stock", result.Reason);
    }

    private static ServerCallContext CreateContext() =>
        TestServerCallContext.Create("test", null, DateTime.UtcNow.AddMinutes(1), new Metadata(), CancellationToken.None,
            "127.0.0.1", null, null, _ => Task.CompletedTask, () => new WriteOptions(), _ => { });
}
