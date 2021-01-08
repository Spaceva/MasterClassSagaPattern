using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface PaymentRefused : CorrelatedBy<Guid>
    {
        string Reason { get; }
    }
}
