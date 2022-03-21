using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.BillingService;

public class PaymentCancelledConsumer : IConsumer<PaymentCancelled>
{
    private readonly BillingDbContext dbContext;
    private readonly ILogger<PaymentCancelledConsumer> logger;

    public PaymentCancelledConsumer(BillingDbContext dbContext, ILogger<PaymentCancelledConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCancelled> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var billing = await dbContext.Billings.FindAsync(id);

        if (billing is null)
        {
            return;
        }

        logger.LogInformation("'{id}' exists in this context. Deleting because of {event} message.", id, nameof(PaymentCancelled));

        dbContext.Billings.Remove(billing);

        await dbContext.SaveChangesAsync();

        await context.Publish<BillingCancelled>(new { context.CorrelationId });
    }
}
