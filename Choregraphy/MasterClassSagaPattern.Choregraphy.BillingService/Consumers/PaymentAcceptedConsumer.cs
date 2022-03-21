using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.BillingService;

public class PaymentAcceptedConsumer : IConsumer<PaymentAccepted>
{
    private readonly BillingDbContext dbContext;
    private readonly ILogger<PaymentAcceptedConsumer> logger;

    public PaymentAcceptedConsumer(BillingDbContext dbContext, ILogger<PaymentAcceptedConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentAccepted> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var billing = await dbContext.Billings.FindAsync(id);

        logger.LogInformation("Received payment with Id '{id}'. Processing...", id);

        if (billing is null)
        {
            logger.LogInformation("'{id}' does not exists in this context. Rejecting, will retry in a few.", id);

            throw new BillingNotFoundException();
        }

        logger.LogInformation("'{id}' exists in this context.", id);

        billing.Paid = true;

        await dbContext.SaveChangesAsync();

        logger.LogInformation($"Processed.");

        await context.Publish<BillingCompleted>(new { context.CorrelationId });
    }
}
