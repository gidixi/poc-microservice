using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Poc.Micro.Persistence.Abstractions;
using Poc.Micro.Ordering.Domain.Orders;
using Poc.Micro.Ordering.Domain.V1;
using DomainOrder = Poc.Micro.Ordering.Domain.Orders.Order;
using DomainOrderItem = Poc.Micro.Ordering.Domain.Orders.OrderItem;

namespace Poc.Micro.Ordering.Application.Orders;

public sealed class OrdersAppService : IOrdersAppService
{
    private readonly IRepository<DomainOrder> _orders;
    private readonly IUnitOfWork _uow;

    public OrdersAppService(IRepository<DomainOrder> orders, IUnitOfWork uow)
    {
        _orders = orders;
        _uow = uow;
    }

    public async Task<Uuid> SavePricedOrderAsync(PricedOrder dto, CancellationToken ct)
    {
        var orderId = Guid.Parse(dto.Order.OrderId.Value);

        var exists = await _orders.AnyAsync(o => o.OrderId == orderId, ct);
        if (exists)
            return new Uuid { Value = orderId.ToString() };

        var order = DomainOrder.Create(
            orderId: orderId,
            customerId: dto.Order.CustomerId,
            currency: dto.Total.Currency,
            subtotal: (decimal)dto.Subtotal.Amount,
            tax: (decimal)dto.Tax.Amount,
            total: (decimal)dto.Total.Amount,
            items: dto.Order.Items.Select(i => new DomainOrderItem(
                sku: i.Sku,
                quantity: i.Qty.Value,
                unitPrice: (decimal)i.UnitPrice.Amount
            )).ToList()
        );

        await _uow.ExecuteInTransactionAsync(async ctInner =>
        {
            await _orders.AddAsync(order, ctInner);
            //await _uow.SaveChangesAsync(ctInner);
        }, ct);

        return new Uuid { Value = orderId.ToString() };
    }

    public async Task<List<PricedOrder>> ListOrdersAsync(CancellationToken ct)
    {
        var entities = await _orders.ListAsync(ct: ct);
        return entities.Select(o => new PricedOrder
        {
            Order = new Order
            {
                OrderId = new Uuid { Value = o.OrderId.ToString() },
                CustomerId = o.CustomerId,
                Items = { o.Items.Select(i => new OrderItem
                    {
                        Sku = i.Sku,
                        Qty = new Quantity { Value = i.Quantity },
                        UnitPrice = new Money { Currency = o.Currency, Amount = (double)i.UnitPrice }
                    }) }
            },
            Subtotal = new Money { Currency = o.Currency, Amount = (double)o.Subtotal },
            Tax = new Money { Currency = o.Currency, Amount = (double)o.Tax },
            Total = new Money { Currency = o.Currency, Amount = (double)o.Total }
        }).ToList();
    }
}
