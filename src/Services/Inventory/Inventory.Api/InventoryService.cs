using System.Threading.Tasks;
using Grpc.Core;
using Poc.Micro.Ordering.Api.V1;
using Poc.Micro.Ordering.Domain.V1;

namespace Poc.Micro.Inventory.Api;

using ApiInventory = Poc.Micro.Ordering.Api.V1.Inventory;

public class InventoryService : ApiInventory.InventoryBase
{
    public override Task<ReservationResult> Reserve(Order request, ServerCallContext context)
    {
        return Task.FromResult(new ReservationResult
        {
            Order = request,
            Reserved = true,
            Reason = string.Empty
        });
    }
}
