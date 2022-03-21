using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface OrderCreated : CorrelatedBy<Guid>
{
    int Quantity { get; }

    string Address { get; }
}
