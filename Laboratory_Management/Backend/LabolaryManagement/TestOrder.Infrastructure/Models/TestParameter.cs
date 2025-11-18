using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class TestParameter
{
    public int ParameterId { get; set; }

    public string ParameterName { get; set; } = null!;

    public string? Unit { get; set; }

    public string? ReferenceRange { get; set; }

    public double? MinRange { get; set; }

    public double? MaxRange { get; set; }

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();

    public virtual ICollection<TestCatalog> Catalogs { get; set; } = new List<TestCatalog>();
}
