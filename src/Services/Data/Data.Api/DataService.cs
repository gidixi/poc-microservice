using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
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

    public override async Task<ListOrdersResponse> ListOrders(Empty request, ServerCallContext context)
    {
        var orders = await _app.ListPricedOrdersAsync(context.CancellationToken);
        return new ListOrdersResponse { Orders = { orders } };
    }
}
