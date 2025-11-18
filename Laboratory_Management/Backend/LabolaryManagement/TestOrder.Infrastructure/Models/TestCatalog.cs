using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class TestCatalog
{
    public int CatalogId { get; set; }

    public string TestName { get; set; } = null!;

    public string? Description { get; set; }

    public double Price { get; set; }

    public virtual ICollection<BookingTest> BookingTests { get; set; } = new List<BookingTest>();

    public virtual ICollection<CatalogBundle> CatalogBundles { get; set; } = new List<CatalogBundle>();

    public virtual ICollection<TestParameter> Parameters { get; set; } = new List<TestParameter>();
}
