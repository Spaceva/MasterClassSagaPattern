using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.BillingService;

public class PaymentRefusedConsumer : IConsumer<PaymentRefused>
{
    private readonly BillingDbContext dbContext;
    private readonly ILogger<PaymentRefusedConsumer> logger;

    public PaymentRefusedConsumer(BillingDbContext dbContext, ILogger<PaymentRefusedConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentRefused> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var reason = context.Message.Reason;
        var billing = await dbContext.Billings.FindAsync(id);

        if (billing is null)
        {
            return;
        }

        logger.LogInformation("'{id}' exists in this context. Deleting because of {event} message with Reason = '{reason}'.", id, nameof(PaymentRefused), reason);

        dbContext.Billings.Remove(billing);

        await dbContext.SaveChangesAsync();

        await context.Publish<BillingCancelled>(new { context.CorrelationId });
    }
}
