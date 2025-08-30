using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Poc.Micro.Data.Infrastructure;
using Poc.Micro.Ordering.Api.V1;
using Poc.Micro.Ordering.Domain.V1;

namespace Poc.Micro.Data.Api;

public class DataService : Poc.Micro.Ordering.Api.V1.Data.DataBase
{
    private readonly OrderDbContext _db;

    public DataService(OrderDbContext db)
    {
        _db = db;
    }

    public override async Task<Uuid> SaveOrder(PricedOrder request, ServerCallContext context)
    {
        var orderId = Guid.Parse(request.Order.OrderId.Value);
        var entity = new OrderEntity
        {
            OrderId = orderId,
            CustomerId = request.Order.CustomerId,
            Currency = request.Total.Currency,
            Subtotal = request.Subtotal.Amount,
            Tax = request.Tax.Amount,
            Total = request.Total.Amount,
            Items = request.Order.Items.Select(i => new OrderItemEntity
            {
                Sku = i.Sku,
                Quantity = i.Qty.Value,
                UnitPrice = i.UnitPrice.Amount
            }).ToList()
        };
        _db.Orders.Add(entity);
        await _db.SaveChangesAsync();
        return new Uuid { Value = orderId.ToString() };
    }
}
