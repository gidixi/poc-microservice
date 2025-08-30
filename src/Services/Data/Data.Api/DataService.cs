using System.Threading.Tasks;
using Grpc.Core;
using Poc.Micro.Ordering.Api.V1;
using Poc.Micro.Ordering.Application.Orders;
using Poc.Micro.Ordering.Domain.V1;

namespace Poc.Micro.Data.Api;

public class DataService : Poc.Micro.Ordering.Api.V1.Data.DataBase
{
    private readonly IOrdersAppService _app;

    public DataService(IOrdersAppService app)
    {
        _app = app;
    }

    public override Task<Uuid> SaveOrder(PricedOrder request, ServerCallContext context)
        => _app.SavePricedOrderAsync(request, context.CancellationToken);
}
