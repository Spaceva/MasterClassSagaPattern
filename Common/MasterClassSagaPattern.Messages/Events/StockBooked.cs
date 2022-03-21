using MassTransit;
using System;

namespace MasterClassSagaPattern.Messages;

public interface StockBooked : CorrelatedBy<Guid>
{
}
