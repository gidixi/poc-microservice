using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Poc.Micro.Ordering.Api.V1;
using Poc.Micro.Ordering.Domain.V1;

namespace Poc.Micro.Pricing.Api;

using ApiPricing = Poc.Micro.Ordering.Api.V1.Pricing;

public class PricingService : ApiPricing.PricingBase
{
    public override Task<PricedOrder> Calculate(Order request, ServerCallContext context)
    {
        double subtotal = 0;
        foreach (var item in request.Items)
        {
            if (item.UnitPrice == null || item.UnitPrice.Amount <= 0)
            {
                item.UnitPrice = new Money { Amount = MockPrice(item.Sku), Currency = "EUR" };
            }
            subtotal += item.UnitPrice.Amount * item.Qty.Value;
        }

        var tax = Math.Round(subtotal * 0.22, 2);
        var total = subtotal + tax;

        return Task.FromResult(new PricedOrder
        {
            Order = request,
            Subtotal = new Money { Amount = subtotal, Currency = "EUR" },
            Tax = new Money { Amount = tax, Currency = "EUR" },
            Total = new Money { Amount = total, Currency = "EUR" }
        });
    }

    private static double MockPrice(string sku) => sku.GetHashCode() % 10 + 10;
}
