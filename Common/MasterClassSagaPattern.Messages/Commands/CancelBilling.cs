using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface CancelBilling : CorrelatedBy<Guid>
    {
        string Reason { get; }
    }
}
