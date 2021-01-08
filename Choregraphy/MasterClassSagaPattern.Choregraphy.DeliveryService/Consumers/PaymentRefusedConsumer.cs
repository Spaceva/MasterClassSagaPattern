using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.DeliveryService
{
    public class PaymentRefusedConsumer : IConsumer<PaymentRefused>
    {
        private readonly DeliveryDbContext dbContext;
        private readonly ILogger<PaymentRefusedConsumer> logger;

        public PaymentRefusedConsumer(DeliveryDbContext dbContext, ILogger<PaymentRefusedConsumer> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentRefused> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();
            var delivery = await dbContext.Deliveries.FindAsync(id);

            if (delivery is null)
            {
                return;
            }

            logger.LogInformation($"'{id}' exists in this context. Deleting because of {nameof(PaymentRefused)} message.");

            dbContext.Deliveries.Remove(delivery);

            await dbContext.SaveChangesAsync();

            await context.Publish<DeliveryCancelled>(new { context.CorrelationId });
        }
    }
}
