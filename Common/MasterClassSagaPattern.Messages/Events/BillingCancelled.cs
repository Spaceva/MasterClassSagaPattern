using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface BillingCancelled : CorrelatedBy<Guid>
{
}
