using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Orchestration.BillingService;

public class CancelBillingConsumer : IConsumer<CancelBilling>
{
    private readonly BillingDbContext dbContext;
    private readonly ILogger<CancelBillingConsumer> logger;

    public CancelBillingConsumer(BillingDbContext dbContext, ILogger<CancelBillingConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<CancelBilling> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var reason = context.Message.Reason;
        var billing = await dbContext.Billings.FindAsync(id);

        if (billing is null)
        {
            return;
        }

        logger.LogInformation("'{id}' exists in this context. Deleting because of {command} message with Reason = '{reason}'.", id, nameof(CancelBilling), reason);

        dbContext.Billings.Remove(billing);

        await dbContext.SaveChangesAsync();
    }
}
