using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class TestReport
{
    public long DocumentId { get; set; }

    public Guid? BookingId { get; set; }

    public string Filename { get; set; } = null!;

    public string? FileType { get; set; }

    public byte[]? ResultData { get; set; }

    public DateOnly? CreatedAt { get; set; }

    public virtual Booking? Booking { get; set; }
}
