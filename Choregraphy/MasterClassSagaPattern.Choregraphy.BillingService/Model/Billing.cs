﻿using System;

namespace MasterClassSagaPattern.Choregraphy.BillingService
{
    public class Billing
    {
        public Guid Id { get; set; }

        public int Quantity { get; set; }

        public float Amount { get; set; }

        public string Address { get; set; }

        public bool Paid { get; set; }
    }
}
