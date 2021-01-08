using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Orchestration.PaymentService
{
    public class AcceptPaymentConsumer : IConsumer<AcceptPayment>
    {
        private readonly PaymentDbContext dbContext;
        private readonly ILogger<AcceptPaymentConsumer> logger;

        public AcceptPaymentConsumer(PaymentDbContext dbContext, ILogger<AcceptPaymentConsumer> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<AcceptPayment> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();
            var payment = await dbContext.Payments.FindAsync(id);

            logger.LogInformation($"Received {nameof(AcceptPayment)} message with Id = '{id}'");

            if (payment is null)
            {
                logger.LogInformation($"'{id}' does not exists in this context. Rejecting, will retry in a few.");

                throw new PaymentNotFoundException();
            }
            
            logger.LogInformation($"'{id}' exists in this context.");

            payment.IsPaymentAccepted = true;

            await dbContext.SaveChangesAsync();

            logger.LogInformation($"'{id}' accepted.");

            await context.Publish<PaymentAccepted>(new { context.CorrelationId, payment.Amount });
        }
    }
}
