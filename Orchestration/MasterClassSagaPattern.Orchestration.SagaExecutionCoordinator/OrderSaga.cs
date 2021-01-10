using MassTransit;
using MassTransit.Saga;
using MasterClassSagaPattern.Common;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Orchestration.SagaExecutionCoordinator
{
    public class OrderSaga :
        ISaga,
        InitiatedBy<OrderCreated>,
        Orchestrates<OrderRegistered>,
        Orchestrates<PaymentAccepted>,
        Orchestrates<PaymentCancelled>,
        Orchestrates<PaymentCreated>,
        Orchestrates<PaymentRefused>,
        Orchestrates<BillingPrepared>,
        Orchestrates<BillingCompleted>,
        Orchestrates<DeliveryPrepared>,
        Orchestrates<DeliveryStarted>,
        Orchestrates<StockBooked>,
        Orchestrates<StockBookingFailed>
    {
        private readonly ILogger<OrderSaga> logger;

        public Guid CorrelationId { get; set; }
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public string Address { get; set; }
        public float Amount { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsBillingPrepared { get; set; }
        public bool IsBillingCompleted { get; set; }
        public bool IsStockBooked { get; set; }
        public bool IsPaymentCreated { get; set; }
        public bool? IsPaymentAccepted { get; set; }
        public bool IsDeliveryPrepared { get; set; }
        public bool IsShipped { get; set; }
        public bool IsCancelled { get; set; }
        public string CancellationReason { get; set; }

        public OrderSaga()
        {
            this.logger = new LoggerFactory().CreateLogger<OrderSaga>();
        }

        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();
            var address = context.Message.Address;
            var quantity = context.Message.Quantity;
            var amount = quantity * 2.5f;

            logger.LogInformation($"Received {nameof(OrderCreated)} message with {{ Id = '{id}', Address = '{address}', Quantity = {quantity} }}.");

            this.Quantity = quantity;
            this.Address = address;
            this.Amount = amount;

            logger.LogInformation($"Delivery '{id}' created. Waiting for Payment and Stock services.");

            var endpoint = await GetSendEndpoint(context, Constants.Queues.ORDER);
            await endpoint.Send<RegisterOrder>(new { CorrelationId, Address, Quantity });
        }

        public async Task Consume(ConsumeContext<OrderRegistered> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(OrderRegistered)} message with {{ Id = '{id}' }}.");

            this.IsRegistered = true;

            logger.LogInformation($"Order service did register.");

            var endpoint = await GetSendEndpoint(context, Constants.Queues.DELIVERY);
            await endpoint.Send<PrepareDelivery>(new { CorrelationId, Address });
            endpoint = await GetSendEndpoint(context, Constants.Queues.BILLING);
            await endpoint.Send<PrepareBilling>(new { CorrelationId, Address, Quantity, Amount });
            endpoint = await GetSendEndpoint(context, Constants.Queues.STOCKS);
            await endpoint.Send<BookStock>(new { CorrelationId, Quantity });
            endpoint = await GetSendEndpoint(context, Constants.Queues.PAYMENT);
            await endpoint.Send<CreatePayment>(new { CorrelationId, Amount });
        }

        public async Task Consume(ConsumeContext<StockBooked> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(StockBooked)} with Id '{id}'. Processing...");

            this.IsStockBooked = true;

            logger.LogInformation($"Marked IsStockBooked to true .");

            await TryStartDelivery(context);
        }

        public async Task Consume(ConsumeContext<BillingCompleted> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(BillingCompleted)} with Id '{id}'. Processing...");

            this.IsBillingCompleted = true;

            logger.LogInformation($"Marked IsBillingCompleted to true .");

            await TryStartDelivery(context);
        }

        public async Task Consume(ConsumeContext<PaymentRefused> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();
            var reason = context.Message.Reason;

            logger.LogInformation($"Received {nameof(PaymentRefused)} with Id '{id}' and Reason = '{reason}'. Processing...");

            this.IsPaymentAccepted = false;

            logger.LogInformation($"Marked IsPaymentAccepted = false .");

            var endpoint = await GetSendEndpoint(context, Constants.Queues.DELIVERY);
            await endpoint.Send<CancelDelivery>(new { CorrelationId, context.Message.Reason });
            endpoint = await GetSendEndpoint(context, Constants.Queues.BILLING);
            await endpoint.Send<CancelBilling>(new { CorrelationId, context.Message.Reason });
            endpoint = await GetSendEndpoint(context, Constants.Queues.STOCKS);
            await endpoint.Send<UnbookStock>(new { CorrelationId, Quantity });
        }

        public Task Consume(ConsumeContext<DeliveryPrepared> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(DeliveryPrepared)} with Id '{id}'. Processing...");

            this.IsDeliveryPrepared = true;

            logger.LogInformation($"Marked IsDeliveryPrepared to true .");

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<DeliveryStarted> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(DeliveryStarted)} with Id '{id}'. Processing...");

            this.IsShipped = true;

            logger.LogInformation($"Marked DeliveryStarted to true .");

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<BillingPrepared> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(BillingPrepared)} with Id '{id}'. Processing...");

            this.IsBillingPrepared = true;

            logger.LogInformation($"Marked IsBillingPrepared to true .");

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<PaymentCreated> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(PaymentCreated)} with Id '{id}'. Processing...");

            this.IsPaymentCreated = true;

            logger.LogInformation($"Marked IsPaymentCreated to true .");

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<PaymentCancelled> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(PaymentCancelled)} with Id '{id}'. Processing...");

            this.IsCancelled = true;

            logger.LogInformation($"Marked IsCancelled to true .");

            return Task.CompletedTask;
        }

        public async Task Consume(ConsumeContext<PaymentAccepted> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(PaymentAccepted)} with Id '{id}'. Processing...");

            this.IsPaymentAccepted = true;

            logger.LogInformation($"Marked IsPaymentAccepted = true .");

            var endpoint = await GetSendEndpoint(context, Constants.Queues.BILLING);
            await endpoint.Send<CompleteBilling>(new { CorrelationId });
        }

        public async Task Consume(ConsumeContext<StockBookingFailed> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(StockBookingFailed)} with Id '{id}' and Reason '{context.Message.Reason}'. Processing...");

            this.IsCancelled = true;

            logger.LogInformation($"Marked IsCancelled to true .");

            var endpoint = await GetSendEndpoint(context, Constants.Queues.DELIVERY);
            await endpoint.Send<CancelDelivery>(new { CorrelationId, context.Message.Reason });
            endpoint = await GetSendEndpoint(context, Constants.Queues.BILLING);
            await endpoint.Send<CancelBilling>(new { CorrelationId, context.Message.Reason });
            endpoint = await GetSendEndpoint(context, Constants.Queues.PAYMENT);
            await endpoint.Send<CancelPayment>(new { CorrelationId, context.Message.Reason });
        }

        private async Task TryStartDelivery(ISendEndpointProvider sendEndpointProvider)
        {
            var canBeShipped = this.IsBillingCompleted && this.IsPaymentAccepted.GetValueOrDefault() && this.IsStockBooked && !this.IsCancelled;
            if (canBeShipped)
            {
                var endpoint = await GetSendEndpoint(sendEndpointProvider, Constants.Queues.DELIVERY);
                await endpoint.Send<StartDelivery>(new { CorrelationId });
            }
        }

        private async Task<ISendEndpoint> GetSendEndpoint(ISendEndpointProvider sendEndpointProvider, string endpointName)
        {
            return await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{endpointName}"));
        }
    }
}
