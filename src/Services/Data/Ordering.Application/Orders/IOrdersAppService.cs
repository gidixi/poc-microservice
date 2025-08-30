using System.Threading;
using System.Threading.Tasks;
using Poc.Micro.Ordering.Domain.V1;

namespace Poc.Micro.Ordering.Application.Orders;

public interface IOrdersAppService
{
    Task<Uuid> SavePricedOrderAsync(PricedOrder dto, CancellationToken ct);
}
