using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface DeliveryStarted : CorrelatedBy<Guid>
{
}
