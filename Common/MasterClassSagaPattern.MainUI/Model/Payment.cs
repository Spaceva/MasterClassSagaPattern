using System;

namespace MasterClassSagaPattern.MainUI
{
    public class Payment
    {
        public Guid Id { get; set; }

        public PaymentStatus Status { get; set; }

        public enum PaymentStatus
        {
            Pending,
            Accepted,
            Refused,
            Cancelled
        }
    }
}
