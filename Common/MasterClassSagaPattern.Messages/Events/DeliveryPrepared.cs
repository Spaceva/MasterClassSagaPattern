using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface DeliveryPrepared : CorrelatedBy<Guid>
{
}
