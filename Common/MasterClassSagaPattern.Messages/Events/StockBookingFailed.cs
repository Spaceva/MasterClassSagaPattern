using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface StockBookingFailed : CorrelatedBy<Guid>
{
    string Reason { get; }
}
