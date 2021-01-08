using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface CreatePayment : CorrelatedBy<Guid>
    {
        float Amount { get; }
    }
}
