using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class TestResult
{
    public long ResultId { get; set; }

    public long? TestBookingNo { get; set; }

    public int? ParameterId { get; set; }

    public string? ResultValue { get; set; }

    public bool? IsNormal { get; set; }

    /// <summary>
    /// Nhận xét từ AI (Gemini) về chỉ số này
    /// </summary>
    public string? Comment { get; set; }

    public virtual TestParameter? Parameter { get; set; }

    public virtual BookingTest? TestBookingNoNavigation { get; set; }
}
