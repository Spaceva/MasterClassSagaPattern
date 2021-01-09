using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.MainUI
{
    public class PaymentCancelledConsumer : IConsumer<PaymentCancelled>
    {
        private readonly MainDbContext dbContext;
        private readonly ILogger<PaymentCancelledConsumer> logger;

        public PaymentCancelledConsumer(MainDbContext dbContext, ILogger<PaymentCancelledConsumer> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCancelled> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();
            var billing = await dbContext.Payments.FindAsync(id);

            if (billing is null)
            {
                return;
            }

            logger.LogInformation($"'{id}' exists in this context. Updating because of {nameof(PaymentCancelled)} message.");

            billing.Status = Payment.PaymentStatus.Cancelled;

            await dbContext.SaveChangesAsync();
        }
    }
}
