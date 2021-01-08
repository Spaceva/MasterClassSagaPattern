using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.PaymentService
{
    public class OrderRegisteredConsumer : IConsumer<OrderRegistered>
    {
        private readonly PaymentDbContext dbContext;
        private readonly ILogger<OrderRegisteredConsumer> logger;

        public OrderRegisteredConsumer(PaymentDbContext dbContext, ILogger<OrderRegisteredConsumer> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderRegistered> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();
            var quantity = context.Message.Quantity;

            logger.LogInformation($"Received {nameof(OrderRegistered)} message with Id = '{id}' and Quantity = {quantity}.");

            dbContext.Add(new Payment { Id = id, Amount = quantity * 2.5f });

            await dbContext.SaveChangesAsync();

            logger.LogInformation($"Payment '{id}' created. Waiting for stock and user input.");

            await context.Publish<PaymentCreated>(new { context.CorrelationId });
        }
    }
}
