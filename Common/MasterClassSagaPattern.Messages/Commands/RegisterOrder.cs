using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface RegisterOrder : CorrelatedBy<Guid>
{
    int Quantity { get; }

    string Address { get; }
}
