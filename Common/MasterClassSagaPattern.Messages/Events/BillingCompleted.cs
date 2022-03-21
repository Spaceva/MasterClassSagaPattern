using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface BillingCompleted : CorrelatedBy<Guid>
{
}
