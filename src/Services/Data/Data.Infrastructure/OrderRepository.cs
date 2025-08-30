using Microsoft.EntityFrameworkCore;
using Poc.Micro.Ordering.Domain.Orders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Poc.Micro.Data.Infrastructure;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(OrderDbContext db) : base(db) { }

    public Task<Order?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => _set.Include(x => x.Items).FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
}
