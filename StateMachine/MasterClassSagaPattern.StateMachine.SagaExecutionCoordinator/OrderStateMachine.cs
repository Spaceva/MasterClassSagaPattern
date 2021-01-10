using Automatonymous;
using MassTransit;
using MasterClassSagaPattern.Common;
using MasterClassSagaPattern.Messages;
using System;

namespace MasterClassSagaPattern.StateMachine.SagaExecutionCoordinator
{
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
                            var address = context.Data.Address;
                            var quantity = context.Data.Quantity;
                            var amount = quantity * 2.5f;

                            context.Instance.Quantity = quantity;
                            context.Instance.Address = address;
                            context.Instance.Amount = amount;
                        })
                        .SendAsync(orderEndpoint, context => context.Init<RegisterOrder>(new { context.Instance.CorrelationId, context.Instance.Address, context.Instance.Quantity }))
                        .TransitionTo(AwaitingRegistration));

            During(AwaitingRegistration,
                When(OrderRegisteredEvent)
                    .SendAsync(deliveryEndpoint, context => context.Init<PrepareDelivery>(new { context.Instance.CorrelationId, context.Instance.Address }))
                    .SendAsync(billingEndpoint, context => context.Init<PrepareBilling>(new { context.Instance.CorrelationId, context.Instance.Address, context.Instance.Quantity, context.Instance.Amount }))
                    .SendAsync(stocksEndpoint, context => context.Init<BookStock>(new { context.Instance.CorrelationId, context.Instance.Quantity }))
                    .SendAsync(paymentEndpoint, context => context.Init<CreatePayment>(new { context.Instance.CorrelationId, context.Instance.Amount }))
                    .TransitionTo(AwaitingPayment));

            During(AwaitingPayment,
                When(StockBookedEvent)
                    .Then(context => context.Instance.IsStockBooked = true),
                When(DeliveryPreparedEvent)
                    .Then(context => context.Instance.IsDeliveryPrepared = true),
                When(BillingPreparedEvent)
                    .Then(context => context.Instance.IsBillingPrepared = true),
                When(PaymentCreatedEvent)
                    .Then(context => context.Instance.IsPaymentCreated = true),
                When(PaymentRefusedEvent)
                    .SendAsync(deliveryEndpoint, context => context.Init<CancelDelivery>(new { context.Instance.CorrelationId, context.Data.Reason }))
                    .SendAsync(billingEndpoint, context => context.Init<CancelBilling>(new { context.Instance.CorrelationId, context.Data.Reason }))
                    .SendAsync(stocksEndpoint, context => context.Init<UnbookStock>(new { context.Instance.CorrelationId, context.Instance.Quantity }))
                    .TransitionTo(Final),
                When(StockBookingFailedEvent)
                    .SendAsync(paymentEndpoint, context => context.Init<CancelPayment>(new { context.Instance.CorrelationId, context.Data.Reason }))
                    .SendAsync(deliveryEndpoint, context => context.Init<CancelDelivery>(new { context.Instance.CorrelationId, context.Data.Reason }))
                    .SendAsync(billingEndpoint, context => context.Init<CancelBilling>(new { context.Instance.CorrelationId, context.Data.Reason }))
                    .TransitionTo(Final),
                When(PaymentAcceptedEvent)
                    .IfElse(context => context.Instance.IsStockBooked,
                        a => a.IfElse(
                            innerContext => innerContext.Instance.IsBillingPrepared,
                            b => b.SendAsync(billingEndpoint, context => context.Init<CompleteBilling>(new { context.Instance.CorrelationId }))
                              .TransitionTo(AwaitingBilling),
                            b => b.TransitionTo(AwaitingBilling)),
                        a => a.TransitionTo(AwaitingStocks)));

            During(AwaitingStocks,
                When(DeliveryPreparedEvent)
                    .Then(context => context.Instance.IsDeliveryPrepared = true),
                When(BillingPreparedEvent)
                    .Then(context => context.Instance.IsBillingPrepared = true),
                When(StockBookingFailedEvent)
                    .SendAsync(paymentEndpoint, context => context.Init<CancelPayment>(new { context.Instance.CorrelationId, context.Data.Reason }))
                    .SendAsync(deliveryEndpoint, context => context.Init<CancelDelivery>(new { context.Instance.CorrelationId, context.Data.Reason }))
                    .SendAsync(billingEndpoint, context => context.Init<CancelBilling>(new { context.Instance.CorrelationId, context.Data.Reason }))
                    .TransitionTo(Final),
                When(StockBookedEvent)
                    .Then(context => context.Instance.IsStockBooked = true)
                    .SendAsync(billingEndpoint, context => context.Init<CompleteBilling>(new { context.Instance.CorrelationId }))
                    .TransitionTo(AwaitingBilling));

            During(AwaitingBilling,
                When(BillingPreparedEvent)
                    .Then(context => context.Instance.IsBillingPrepared = true)
                    .SendAsync(billingEndpoint, context => context.Init<CompleteBilling>(new { context.Instance.CorrelationId })),
                When(BillingCompletedEvent)
                .IfElse(
                        context => context.Instance.IsDeliveryPrepared,
                        a => a.SendAsync(deliveryEndpoint, context => context.Init<StartDelivery>(new { context.Instance.CorrelationId }))
                            .TransitionTo(AwaitingDelivery),
                        a => a.TransitionTo(AwaitingDelivery)));

            During(AwaitingDelivery,
                When(DeliveryPreparedEvent)
                    .Then(context => context.Instance.IsDeliveryPrepared = true)
                    .SendAsync(deliveryEndpoint, context => context.Init<StartDelivery>(new { context.Instance.CorrelationId })),
                When(DeliveryStartedEvent)
                    .TransitionTo(Final));

            During(Final,
                Ignore(PaymentCancelledEvent),
                Ignore(StockBookingFailedEvent));
        }

        public Event<OrderCreated> OrderCreatedEvent { get; private set; }
        public Event<OrderRegistered> OrderRegisteredEvent { get; private set; }
        public Event<PaymentAccepted> PaymentAcceptedEvent { get; private set; }
        public Event<PaymentCancelled> PaymentCancelledEvent { get; private set; }
        public Event<PaymentCreated> PaymentCreatedEvent { get; private set; }
        public Event<PaymentRefused> PaymentRefusedEvent { get; private set; }
        public Event<BillingPrepared> BillingPreparedEvent { get; private set; }
        public Event<BillingCompleted> BillingCompletedEvent { get; private set; }
        public Event<DeliveryPrepared> DeliveryPreparedEvent { get; private set; }
        public Event<DeliveryStarted> DeliveryStartedEvent { get; private set; }
        public Event<StockBooked> StockBookedEvent { get; private set; }
        public Event<StockBookingFailed> StockBookingFailedEvent { get; private set; }

        public State AwaitingRegistration { get; private set; }
        public State AwaitingPayment { get; private set; }
        public State AwaitingStocks { get; private set; }
        public State AwaitingBilling { get; private set; }
        public State AwaitingDelivery { get; private set; }

        private Uri GetSendEndpoint(string endpointName)
        {
            return new Uri($"queue:{endpointName}");
        }
    }
}
