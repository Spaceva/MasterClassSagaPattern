using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface PaymentAccepted : CorrelatedBy<Guid>
{
}
