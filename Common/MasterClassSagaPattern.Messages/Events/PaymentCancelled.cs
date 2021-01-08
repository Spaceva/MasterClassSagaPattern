using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface PaymentCancelled : CorrelatedBy<Guid>
    {
    }
}
