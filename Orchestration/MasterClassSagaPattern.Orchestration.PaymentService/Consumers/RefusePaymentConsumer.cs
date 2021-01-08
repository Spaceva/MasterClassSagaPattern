using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Orchestration.PaymentService
{
    public class RefusePaymentConsumer : IConsumer<RefusePayment>
    {
        private readonly PaymentDbContext dbContext;
        private readonly ILogger<RefusePaymentConsumer> logger;

        public RefusePaymentConsumer(PaymentDbContext dbContext, ILogger<RefusePaymentConsumer> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<RefusePayment> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();
            var reason = context.Message.Reason;
            var payment = await dbContext.Payments.FindAsync(id);

            logger.LogInformation($"Received {nameof(RefusePayment)} message with Id = '{id}'");

            if (payment is null)
            {
                logger.LogInformation($"'{id}' does not exists in this context. Rejecting, will retry in a few.");

                throw new PaymentNotFoundException();
            }
            
            logger.LogInformation($"'{id}' exists in this context.");

            payment.IsPaymentAccepted = false;

            await dbContext.SaveChangesAsync();

            logger.LogInformation($"'{id}' refused.");

            await context.Publish<PaymentRefused>(new { context.CorrelationId, context.Message.Reason });
        }
    }
}
