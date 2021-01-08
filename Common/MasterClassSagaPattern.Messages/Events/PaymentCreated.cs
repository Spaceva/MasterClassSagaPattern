using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface PaymentCreated : CorrelatedBy<Guid>
    {
        int Quantity { get; }

        float Amount { get; }
    }
}
