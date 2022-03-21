using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface DeliveryCancelled : CorrelatedBy<Guid>
{
}
