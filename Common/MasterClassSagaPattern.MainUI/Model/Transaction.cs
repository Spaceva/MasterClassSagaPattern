using System;

namespace MasterClassSagaPattern.MainUI
{
    public class Transaction
    {
        public Guid Id { get; set; }

        public bool? PaymentStatus { get; set; }
    }
}
