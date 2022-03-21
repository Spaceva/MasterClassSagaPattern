using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface CancelDelivery : CorrelatedBy<Guid>
{
    string Reason { get; }
}
