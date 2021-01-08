using System;

namespace MasterClassSagaPattern.Choregraphy.DeliveryService
{
    public class Delivery
    {
        public Guid Id { get; set; }

        public bool IsBillingCompleted { get; set; }

        public bool IsStockBooked { get; set; }

        public bool IsPaymentAccepted { get; set; }

        public bool IsShipped { get; set; }

        public string Address { get; set; }
    }
}
