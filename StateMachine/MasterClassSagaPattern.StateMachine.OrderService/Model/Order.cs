using System;

namespace MasterClassSagaPattern.StateMachine.OrderService;

public class Order
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public string? Address { get; set; }
}
