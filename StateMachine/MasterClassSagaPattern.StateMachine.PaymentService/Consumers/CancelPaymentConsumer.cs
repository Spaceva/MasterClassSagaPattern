using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.StateMachine.PaymentService;

public class CancelPaymentConsumer : IConsumer<CancelPayment>
{
    private readonly PaymentDbContext dbContext;
    private readonly ILogger<CancelPaymentConsumer> logger;

    public CancelPaymentConsumer(PaymentDbContext dbContext, ILogger<CancelPaymentConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<CancelPayment> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var reason = context.Message.Reason;
        var payment = await dbContext.Payments.FindAsync(id);

        logger.LogInformation("Received {command} message with Id = '{id}'", nameof(CancelPayment), id);

        if (payment is null)
        {
            logger.LogInformation("'{id}' does not exists in this context. Rejecting, will retry in a few.", id);

            throw new PaymentNotFoundException();
        }

        logger.LogInformation("'{id}' exists in this context. Deleting because of {command} message with reason = '{reason}'.", id, nameof(CancelPayment), reason);

        dbContext.Payments.Remove(payment);

        await dbContext.SaveChangesAsync();

        await context.Publish<PaymentCancelled>(new { context.CorrelationId });
    }
}
