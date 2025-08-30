using System;
using System.Collections.Generic;

namespace Poc.Micro.Data.Infrastructure;

public class OrderEntity
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public double Subtotal { get; set; }
    public double Tax { get; set; }
    public double Total { get; set; }
    public List<OrderItemEntity> Items { get; set; } = new();
}
