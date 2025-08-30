using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Testing;
using Poc.Micro.Ordering.Domain.V1;
using Poc.Micro.Pricing.Api;
using Xunit;

namespace Pricing.Tests;

public class PricingServiceTests
{
    [Fact]
    public async Task Calculate_FillsMissingPrices_AndComputesTotals()
    {
        var svc = new PricingService();
        var order = new Order
        {
            OrderId = new Uuid { Value = Guid.NewGuid().ToString() },
            CustomerId = "c1",
            Items =
            {
                new OrderItem { Sku = "A", Qty = new Quantity { Value = 2 }, UnitPrice = new Money { Amount = 10, Currency = "EUR" } },
                new OrderItem { Sku = "B", Qty = new Quantity { Value = 1 }, UnitPrice = new Money() }
            }
        };

        var result = await svc.Calculate(order, CreateContext());

        Assert.All(result.Order.Items, i => Assert.True(i.UnitPrice.Amount > 0));
        var expectedSubtotal = result.Order.Items.Sum(i => i.UnitPrice.Amount * i.Qty.Value);
        Assert.Equal(expectedSubtotal, result.Subtotal.Amount);
        var expectedTax = Math.Round(expectedSubtotal * 0.22, 2);
        Assert.Equal(expectedTax, result.Tax.Amount);
        Assert.Equal(expectedSubtotal + expectedTax, result.Total.Amount);
        Assert.All(result.Order.Items, i => Assert.Equal("EUR", i.UnitPrice.Currency));
        Assert.Equal("EUR", result.Subtotal.Currency);
        Assert.Equal("EUR", result.Tax.Currency);
        Assert.Equal("EUR", result.Total.Currency);
    }

    private static ServerCallContext CreateContext() =>
        TestServerCallContext.Create("test", null, DateTime.UtcNow.AddMinutes(1), new Metadata(), CancellationToken.None,
            "127.0.0.1", null, null, _ => Task.CompletedTask, () => new WriteOptions(), _ => { });
}
