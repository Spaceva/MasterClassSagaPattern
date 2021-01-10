using System;

namespace MasterClassSagaPattern.Orchestration.StockService
{
    public class StockBooking
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
    }
}
