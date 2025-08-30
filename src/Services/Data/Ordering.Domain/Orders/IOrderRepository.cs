using System;
using System.Threading;
using System.Threading.Tasks;
using Poc.Micro.Ordering.Domain.Abstractions;

namespace Poc.Micro.Ordering.Domain.Orders;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}
