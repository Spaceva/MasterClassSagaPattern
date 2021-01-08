using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface CompleteBilling : CorrelatedBy<Guid>
    {
    }
}
