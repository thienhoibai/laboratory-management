using System;
using System.Collections.Generic;

namespace TestOrder.Presentation.Models;

public partial class BookingTest
{
    public long TestBookingNo { get; set; }

    public Guid? BookingId { get; set; }

    public int? CatalogId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual TestCatalog? Catalog { get; set; }

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
