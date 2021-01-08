using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface CancelPayment : CorrelatedBy<Guid>
    {
        string Reason { get; }
    }
}
