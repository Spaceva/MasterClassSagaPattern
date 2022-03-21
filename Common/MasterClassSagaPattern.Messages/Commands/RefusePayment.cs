using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface RefusePayment : CorrelatedBy<Guid>
{
    string Reason { get; }
}
