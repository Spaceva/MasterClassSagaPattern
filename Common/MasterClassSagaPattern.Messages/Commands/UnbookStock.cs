using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface UnbookStock : CorrelatedBy<Guid>
    {
        int Quantity { get; }
    }
}
