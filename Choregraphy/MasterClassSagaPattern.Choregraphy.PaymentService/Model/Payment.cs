using System;

namespace MasterClassSagaPattern.Choregraphy.PaymentService;

public class Payment
{
    public Guid Id { get; set; }

    public float Amount { get; set; }

    public bool IsStockBooked { get; set; }

    public bool? IsPaymentAccepted { get; set; }
}
