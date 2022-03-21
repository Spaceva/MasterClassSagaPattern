using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface BillingPrepared : CorrelatedBy<Guid>
{
}
