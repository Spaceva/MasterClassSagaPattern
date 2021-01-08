using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.DeliveryService
{
    public class OrderRegisteredConsumer : IConsumer<OrderRegistered>
    {
        private readonly DeliveryDbContext dbContext;
        private readonly ILogger<OrderRegisteredConsumer> logger;

        public OrderRegisteredConsumer(DeliveryDbContext dbContext, ILogger<OrderRegisteredConsumer> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderRegistered> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();
            var address = context.Message.Address;

            logger.LogInformation($"Received {nameof(OrderRegistered)} message with Id = '{id}' and Address = '{address}'.");

            dbContext.Add(new Delivery { Id = id, Address = address});

            await dbContext.SaveChangesAsync();

            logger.LogInformation($"Delivery '{id}' created. Waiting for Payment, Stock and Billing services.");

            await context.Publish<DeliveryPrepared>(new { context.CorrelationId });
        }
    }
}
