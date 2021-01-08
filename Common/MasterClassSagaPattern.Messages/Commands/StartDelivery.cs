using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface StartDelivery : CorrelatedBy<Guid>
    {
    }
}
