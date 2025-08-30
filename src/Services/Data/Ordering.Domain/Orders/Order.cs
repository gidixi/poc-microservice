using System;
using System.Collections.Generic;
using Poc.Micro.Ordering.Domain.Abstractions;

namespace Poc.Micro.Ordering.Domain.Orders;

public class Order : IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public string CustomerId { get; private set; } = default!;
    public string Currency { get; private set; } = default!;
    public decimal Subtotal { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Total { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();

    private Order() { }

    public static Order Create(Guid orderId, string customerId, string currency, decimal subtotal, decimal tax, decimal total, List<OrderItem> items)
    {
        return new Order
        {
            OrderId = orderId,
            CustomerId = customerId,
            Currency = currency,
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
            Items = items
        };
    }
}

public class OrderItem
{
    public int Id { get; private set; }
    public string Sku { get; private set; } = default!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private OrderItem() { }

    public OrderItem(string sku, int quantity, decimal unitPrice)
    {
        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
