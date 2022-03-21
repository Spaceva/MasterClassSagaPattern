using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface OrderRegistered : CorrelatedBy<Guid>
{
    int Quantity { get; }

    string Address { get; }
}
