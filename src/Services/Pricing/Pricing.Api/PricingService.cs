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
            if (item.UnitPrice == null)
            {
                item.UnitPrice = new Money { Amount = 10, Currency = "USD" };
            }
            subtotal += item.UnitPrice.Amount * item.Qty.Value;
        }

        var subtotalMoney = new Money { Amount = subtotal, Currency = "USD" };
        var taxMoney = new Money { Amount = subtotal * 0.2, Currency = "USD" };
        var totalMoney = new Money { Amount = subtotalMoney.Amount + taxMoney.Amount, Currency = "USD" };

        return Task.FromResult(new PricedOrder
        {
            Order = request,
            Subtotal = subtotalMoney,
            Tax = taxMoney,
            Total = totalMoney
        });
    }
}
