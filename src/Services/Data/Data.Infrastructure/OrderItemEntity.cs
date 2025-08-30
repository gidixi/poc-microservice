using System;

namespace Poc.Micro.Data.Infrastructure;

public class OrderItemEntity
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
}
