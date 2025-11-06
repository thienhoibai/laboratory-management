using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TestOrder.Infrastructure.Models;

public partial class CatalogBundle
{
    public int BundleId { get; set; }

    public int CatalogId { get; set; }

    public int? SortOrder { get; set; }

    public virtual TestBundle Bundle { get; set; } = null!;

    [JsonIgnore]
    public virtual TestCatalog Catalog { get; set; } = null!;
}
