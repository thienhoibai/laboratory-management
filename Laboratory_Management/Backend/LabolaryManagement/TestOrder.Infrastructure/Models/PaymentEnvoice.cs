using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class PaymentEnvoice
{
    public int PaymentNo { get; set; }

    public Guid BookingId { get; set; }

    public string? Method { get; set; }

    public double? Amount { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? Token { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
