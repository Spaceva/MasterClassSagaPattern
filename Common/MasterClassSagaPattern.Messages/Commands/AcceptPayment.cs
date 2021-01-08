using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface AcceptPayment : CorrelatedBy<Guid>
    {
    }
}
