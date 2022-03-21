using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.StateMachine.PaymentService;

public class CreatePaymentConsumer : IConsumer<CreatePayment>
{
    private readonly PaymentDbContext dbContext;
    private readonly ILogger<CreatePaymentConsumer> logger;

    public CreatePaymentConsumer(PaymentDbContext dbContext, ILogger<CreatePaymentConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<CreatePayment> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var amount = context.Message.Amount;

        logger.LogInformation("Received {command} message with Id = '{id}' and Amount = {amount}.", nameof(CreatePayment), id, amount);

        dbContext.Add(new Payment { Id = id, Amount = amount });

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Payment '{id}' created.", id);

        await context.Publish<PaymentCreated>(new { context.CorrelationId, context.Message.Amount });
    }
}
