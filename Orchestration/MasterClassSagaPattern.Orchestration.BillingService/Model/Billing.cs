using System;

namespace MasterClassSagaPattern.Orchestration.BillingService;

public class Billing
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public float Amount { get; set; }

    public string? Address { get; set; }

    public bool Paid { get; set; }
}
