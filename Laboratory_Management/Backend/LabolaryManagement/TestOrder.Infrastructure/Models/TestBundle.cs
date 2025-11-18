using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class TestBundle
{
    public int BundleId { get; set; }

    public string? BundleName { get; set; }

    public string? Description { get; set; }

    public double? Price { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<CatalogBundle> CatalogBundles { get; set; } = new List<CatalogBundle>();
}
