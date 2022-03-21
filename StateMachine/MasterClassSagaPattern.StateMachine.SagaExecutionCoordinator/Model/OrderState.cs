using MassTransit;
using System;

namespace MasterClassSagaPattern.StateMachine.SagaExecutionCoordinator;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string State { get; set; } = default!;
    public int Quantity { get; set; }
    public string? Address { get; set; }
    public float Amount { get; set; }
    public bool IsBillingPrepared { get; set; }
    public bool IsStockBooked { get; set; }
    public bool IsPaymentCreated { get; set; }
    public bool IsDeliveryPrepared { get; set; }
}
