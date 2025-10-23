using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class PaymentEnvoice
{
    public long PaymentNo { get; set; }

    public long? BookingId { get; set; }

    public string? Method { get; set; }

    public double? Amount { get; set; }

    public int? Status { get; set; }

    public DateOnly? CreatedAt { get; set; }

    public DateOnly? PaidAt { get; set; }

    public string? Token { get; set; }

    public virtual Booking? Booking { get; set; }
}
