using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.PaymentService;

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

        logger.LogInformation("Received {event} message with Id = '{id}'", nameof(AcceptPayment), id);

        if (payment is null)
        {
            logger.LogInformation("'{id}' does not exists in this context. Rejecting, will retry in a few.", id);

            throw new PaymentNotFoundException();
        }

        logger.LogInformation("'{id}' exists in this context.", id);

        payment.IsPaymentAccepted = true;

        await dbContext.SaveChangesAsync();

        logger.LogInformation("'{id}' accepted.", id);

        await context.Publish<PaymentAccepted>(new { context.CorrelationId, payment.Amount });
    }
}
