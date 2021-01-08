using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface BookStock : CorrelatedBy<Guid>
    {
        int Quantity { get; }
    }
}
