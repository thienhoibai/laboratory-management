using System;
using System.Collections.Generic;

namespace TestOrder.Presentation.Models;

public partial class CatalogBundle
{
    public int BundleId { get; set; }

    public int CatalogId { get; set; }

    public int? SortOrder { get; set; }

    public virtual TestBundle Bundle { get; set; } = null!;

    public virtual TestCatalog Catalog { get; set; } = null!;
}
