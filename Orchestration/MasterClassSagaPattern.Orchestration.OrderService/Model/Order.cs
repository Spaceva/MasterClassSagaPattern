using System;

namespace MasterClassSagaPattern.Orchestration.OrderService;

public class Order
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public string? Address { get; set; }
}
