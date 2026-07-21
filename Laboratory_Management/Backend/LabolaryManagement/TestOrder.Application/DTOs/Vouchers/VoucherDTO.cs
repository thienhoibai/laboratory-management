using System;

namespace TestOrder.Application.DTOs.Vouchers
{
    public class VoucherDTO
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
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateVoucherDTO
    {
        public string Code { get; set; } = null!;
        public byte DiscountType { get; set; } // 1 = Percentage, 2 = FixedAmount
        public double DiscountValue { get; set; }
        public double? MinOrderValue { get; set; }
        public double? MaxDiscountAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? UsageLimit { get; set; }
    }

    public class UpdateVoucherDTO
    {
        public string Code { get; set; } = null!;
        public byte DiscountType { get; set; }
        public double DiscountValue { get; set; }
        public double? MinOrderValue { get; set; }
        public double? MaxDiscountAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? UsageLimit { get; set; }
        public bool IsActive { get; set; }
    }

    public class ApplyVoucherDTO
    {
        public string Code { get; set; } = null!;
        public double OrderValue { get; set; }
    }

    public class ApplyVoucherResponseDTO
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public double DiscountAmount { get; set; }
        public double FinalAmount { get; set; }
    }
}
