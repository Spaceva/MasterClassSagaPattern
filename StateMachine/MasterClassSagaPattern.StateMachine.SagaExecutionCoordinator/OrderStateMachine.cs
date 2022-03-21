using MassTransit;
using MasterClassSagaPattern.Common;
using MasterClassSagaPattern.Messages;
using System;

namespace MasterClassSagaPattern.StateMachine.SagaExecutionCoordinator;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        var deliveryEndpoint = GetSendEndpoint(Constants.Queues.DELIVERY);
        var billingEndpoint = GetSendEndpoint(Constants.Queues.BILLING);
        var orderEndpoint = GetSendEndpoint(Constants.Queues.ORDER);
        var paymentEndpoint = GetSendEndpoint(Constants.Queues.PAYMENT);
        var stocksEndpoint = GetSendEndpoint(Constants.Queues.STOCKS);

        Event(() => OrderCreatedEvent, x => x.CorrelateById(context => context.Message.CorrelationId).SelectId(context => context.Message.CorrelationId));
        Event(() => OrderRegisteredEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => PaymentAcceptedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => PaymentCancelledEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => PaymentCreatedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => PaymentRefusedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => BillingPreparedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => DeliveryPreparedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => DeliveryStartedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => StockBookedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => StockBookingFailedEvent, x => x.CorrelateById(context => context.Message.CorrelationId));

        InstanceState(x => x.State);

        Initially(When(OrderCreatedEvent)
                    .Then(context =>
                    {
                        var address = context.Message.Address;
                        var quantity = context.Message.Quantity;
                        var amount = quantity * 2.5f;

                        context.Saga.Quantity = quantity;
                        context.Saga.Address = address;
                        context.Saga.Amount = amount;
                    })
                    .SendAsync(orderEndpoint, context => context.Init<RegisterOrder>(new { context.Saga.CorrelationId, context.Saga.Address, context.Saga.Quantity }))
                    .TransitionTo(AwaitingRegistration));

        During(AwaitingRegistration,
            When(OrderRegisteredEvent)
                .SendAsync(deliveryEndpoint, context => context.Init<PrepareDelivery>(new { context.Saga.CorrelationId, context.Saga.Address }))
                .SendAsync(billingEndpoint, context => context.Init<PrepareBilling>(new { context.Saga.CorrelationId, context.Saga.Address, context.Saga.Quantity, context.Saga.Amount }))
                .SendAsync(stocksEndpoint, context => context.Init<BookStock>(new { context.Saga.CorrelationId, context.Saga.Quantity }))
                .SendAsync(paymentEndpoint, context => context.Init<CreatePayment>(new { context.Saga.CorrelationId, context.Saga.Amount }))
                .TransitionTo(AwaitingPayment));

        During(AwaitingPayment,
            When(StockBookedEvent)
                .Then(context => context.Saga.IsStockBooked = true),
            When(DeliveryPreparedEvent)
                .Then(context => context.Saga.IsDeliveryPrepared = true),
            When(BillingPreparedEvent)
                .Then(context => context.Saga.IsBillingPrepared = true),
            When(PaymentCreatedEvent)
                .Then(context => context.Saga.IsPaymentCreated = true),
            When(PaymentRefusedEvent)
                .SendAsync(deliveryEndpoint, context => context.Init<CancelDelivery>(new { context.Saga.CorrelationId, context.Message.Reason }))
                .SendAsync(billingEndpoint, context => context.Init<CancelBilling>(new { context.Saga.CorrelationId, context.Message.Reason }))
                .SendAsync(stocksEndpoint, context => context.Init<UnbookStock>(new { context.Saga.CorrelationId, context.Saga.Quantity }))
                .TransitionTo(Final),
            When(StockBookingFailedEvent)
                .SendAsync(paymentEndpoint, context => context.Init<CancelPayment>(new { context.Saga.CorrelationId, context.Message.Reason }))
                .SendAsync(deliveryEndpoint, context => context.Init<CancelDelivery>(new { context.Saga.CorrelationId, context.Message.Reason }))
                .SendAsync(billingEndpoint, context => context.Init<CancelBilling>(new { context.Saga.CorrelationId, context.Message.Reason }))
                .TransitionTo(Final),
            When(PaymentAcceptedEvent)
                .IfElse(context => context.Saga.IsStockBooked,
                    a => a.IfElse(
                        innerContext => innerContext.Saga.IsBillingPrepared,
                        b => b.SendAsync(billingEndpoint, context => context.Init<CompleteBilling>(new { context.Saga.CorrelationId }))
                          .TransitionTo(AwaitingBilling),
                        b => b.TransitionTo(AwaitingBilling)),
                    a => a.TransitionTo(AwaitingStocks)));

        During(AwaitingStocks,
            When(DeliveryPreparedEvent)
                .Then(context => context.Saga.IsDeliveryPrepared = true),
            When(BillingPreparedEvent)
                .Then(context => context.Saga.IsBillingPrepared = true),
            When(StockBookingFailedEvent)
                .SendAsync(paymentEndpoint, context => context.Init<CancelPayment>(new { context.Saga.CorrelationId, context.Message.Reason }))
                .SendAsync(deliveryEndpoint, context => context.Init<CancelDelivery>(new { context.Saga.CorrelationId, context.Message.Reason }))
                .SendAsync(billingEndpoint, context => context.Init<CancelBilling>(new { context.Saga.CorrelationId, context.Message.Reason }))
                .TransitionTo(Final),
            When(StockBookedEvent)
                .Then(context => context.Saga.IsStockBooked = true)
                .SendAsync(billingEndpoint, context => context.Init<CompleteBilling>(new { context.Saga.CorrelationId }))
                .TransitionTo(AwaitingBilling));

        During(AwaitingBilling,
            When(BillingPreparedEvent)
                .Then(context => context.Saga.IsBillingPrepared = true)
                .SendAsync(billingEndpoint, context => context.Init<CompleteBilling>(new { context.Saga.CorrelationId })),
            When(BillingCompletedEvent)
            .IfElse(
                    context => context.Saga.IsDeliveryPrepared,
                    a => a.SendAsync(deliveryEndpoint, context => context.Init<StartDelivery>(new { context.Saga.CorrelationId }))
                        .TransitionTo(AwaitingDelivery),
                    a => a.TransitionTo(AwaitingDelivery)));

        During(AwaitingDelivery,
            When(DeliveryPreparedEvent)
                .Then(context => context.Saga.IsDeliveryPrepared = true)
                .SendAsync(deliveryEndpoint, context => context.Init<StartDelivery>(new { context.Saga.CorrelationId })),
            When(DeliveryStartedEvent)
                .TransitionTo(Final));

        During(Final,
            Ignore(PaymentCancelledEvent),
            Ignore(StockBookingFailedEvent));
    }

    public Event<OrderCreated> OrderCreatedEvent { get; private set; } = default!;
    public Event<OrderRegistered> OrderRegisteredEvent { get; private set; } = default!;
    public Event<PaymentAccepted> PaymentAcceptedEvent { get; private set; } = default!;
    public Event<PaymentCancelled> PaymentCancelledEvent { get; private set; } = default!;
    public Event<PaymentCreated> PaymentCreatedEvent { get; private set; } = default!;
    public Event<PaymentRefused> PaymentRefusedEvent { get; private set; } = default!;
    public Event<BillingPrepared> BillingPreparedEvent { get; private set; } = default!;
    public Event<BillingCompleted> BillingCompletedEvent { get; private set; } = default!;
    public Event<DeliveryPrepared> DeliveryPreparedEvent { get; private set; } = default!;
    public Event<DeliveryStarted> DeliveryStartedEvent { get; private set; } = default!;
    public Event<StockBooked> StockBookedEvent { get; private set; } = default!;
    public Event<StockBookingFailed> StockBookingFailedEvent { get; private set; } = default!;

    public State AwaitingRegistration { get; private set; } = default!;
    public State AwaitingPayment { get; private set; } = default!;
    public State AwaitingStocks { get; private set; } = default!;
    public State AwaitingBilling { get; private set; } = default!;
    public State AwaitingDelivery { get; private set; } = default!;

    private static Uri GetSendEndpoint(string endpointName)
     => new($"queue:{endpointName}");
}
