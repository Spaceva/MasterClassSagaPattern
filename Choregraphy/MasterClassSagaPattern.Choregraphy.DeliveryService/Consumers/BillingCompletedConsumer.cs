using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.DeliveryService
{
    public class BillingCompletedConsumer : IConsumer<BillingCompleted>
    {
        private readonly DeliveryDbContext dbContext;
        private readonly ILogger<BillingCompletedConsumer> logger;

        public BillingCompletedConsumer(DeliveryDbContext dbContext, ILogger<BillingCompletedConsumer> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<BillingCompleted> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();

            logger.LogInformation($"Received {nameof(BillingCompleted)} message with Id = '{id}'");

            var delivery = await dbContext.Deliveries.FindAsync(id);

            if (delivery is null)
            {
                logger.LogInformation($"'{id}' does not exists in this context. Rejecting, will retry in a few.");

                throw new DeliveryNotFoundException();
            }

            logger.LogInformation($"'{id}' exists in this context.");

            delivery.IsBillingCompleted = true;
            var shouldSendIsShippedMessage = false;

            if (delivery.IsBillingCompleted
                && delivery.IsPaymentAccepted
                && delivery.IsStockBooked)
            {
                logger.LogInformation($"'{id}' can be shipped !");

                delivery.IsShipped = true;
                shouldSendIsShippedMessage = true;
            }

            await dbContext.SaveChangesAsync();

            logger.LogInformation($"'{id}' updated.");

            if (shouldSendIsShippedMessage)
            {
                await context.Publish<DeliveryStarted>(new { context.CorrelationId });
            }
        }
    }
}
