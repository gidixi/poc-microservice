using System.Collections.Concurrent;
using System.Threading.Tasks;
using Grpc.Core;
using Poc.Micro.Ordering.Api.V1;
using Poc.Micro.Ordering.Domain.V1;

namespace Poc.Micro.Inventory.Api;

using ApiInventory = Poc.Micro.Ordering.Api.V1.Inventory;

public class InventoryService : ApiInventory.InventoryBase
{
    private readonly ConcurrentDictionary<string, int> _stock = new();

    public override Task<ReservationResult> Reserve(Order request, ServerCallContext context)
    {
        foreach (var item in request.Items)
        {
            var available = _stock.GetOrAdd(item.Sku, 100);
            if (available < item.Qty.Value)
            {
                return Task.FromResult(new ReservationResult
                {
                    Order = request,
                    Reserved = false,
                    Reason = $"insufficient stock for {item.Sku}"
                });
            }
        }

        foreach (var item in request.Items)
        {
            _stock.AddOrUpdate(item.Sku, _ => 100 - item.Qty.Value, (_, current) => current - item.Qty.Value);
        }

        return Task.FromResult(new ReservationResult { Order = request, Reserved = true, Reason = string.Empty });
    }
}
