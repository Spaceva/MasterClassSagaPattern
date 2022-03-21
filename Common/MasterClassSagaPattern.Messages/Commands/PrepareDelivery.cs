using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface PrepareDelivery : CorrelatedBy<Guid>
{
    string Address { get; }
}
