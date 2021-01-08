using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages
{
    public interface PrepareBilling : CorrelatedBy<Guid>
    {
        int Quantity { get; }

        string Address { get; }

        float Amount { get; }
    }
}
