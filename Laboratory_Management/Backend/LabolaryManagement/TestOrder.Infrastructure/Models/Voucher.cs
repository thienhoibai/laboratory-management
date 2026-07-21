using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }
    public string Code { get; set; } = null!;
    public byte DiscountType { get; set; } // 1 = Percentage, 2 = FixedAmount
    public double DiscountValue { get; set; }
    public double? MinOrderValue { get; set; }
    public double? MaxDiscountAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
