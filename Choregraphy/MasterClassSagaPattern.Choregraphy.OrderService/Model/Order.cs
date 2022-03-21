using System;

namespace MasterClassSagaPattern.Choregraphy.OrderService;

public class Order
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public string? Address { get; set; }
}
