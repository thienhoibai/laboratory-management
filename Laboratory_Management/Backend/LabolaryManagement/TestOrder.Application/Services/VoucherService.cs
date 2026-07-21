using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestOrder.Application.DTOs.Vouchers;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services
{
    public class VoucherService
    {
        private readonly VoucherRepository _voucherRepository;

        public VoucherService(VoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<IEnumerable<VoucherDTO>> GetAllVouchersPagedAsync(int pageNumber, int pageSize)
        {
            var vouchers = await _voucherRepository.GetAllPagedAsync(pageNumber, pageSize);
            return vouchers.Select(v => MapToDTO(v));
        }

        public async Task<VoucherDTO?> GetVoucherByIdAsync(int id)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            return voucher == null ? null : MapToDTO(voucher);
        }

        public async Task<VoucherDTO> CreateVoucherAsync(CreateVoucherDTO dto)
        {
            var voucher = new Voucher
            {
                Code = dto.Code.ToUpper().Trim(),
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinOrderValue = dto.MinOrderValue,
                MaxDiscountAmount = dto.MaxDiscountAmount,
                StartDate = dto.StartDate,
                ExpiryDate = dto.ExpiryDate,
                UsageLimit = dto.UsageLimit,
                UsageCount = 0,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _voucherRepository.AddAsync(voucher);
            return MapToDTO(voucher);
        }

        public async Task<VoucherDTO?> UpdateVoucherAsync(int id, UpdateVoucherDTO dto)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            if (voucher == null) return null;

            voucher.Code = dto.Code.ToUpper().Trim();
            voucher.DiscountType = dto.DiscountType;
            voucher.DiscountValue = dto.DiscountValue;
            voucher.MinOrderValue = dto.MinOrderValue;
            voucher.MaxDiscountAmount = dto.MaxDiscountAmount;
            voucher.StartDate = dto.StartDate;
            voucher.ExpiryDate = dto.ExpiryDate;
            voucher.UsageLimit = dto.UsageLimit;
            voucher.IsActive = dto.IsActive;

            await _voucherRepository.UpdateAsync(voucher);
            return MapToDTO(voucher);
        }

        public async Task<bool> DeleteVoucherAsync(int id)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            if (voucher == null) return false;

            await _voucherRepository.DeleteAsync(voucher);
            return true;
        }

        public async Task<ApplyVoucherResponseDTO> ValidateAndCalculateDiscountAsync(string code, double orderValue)
        {
            var response = new ApplyVoucherResponseDTO { IsValid = false };
            if (string.IsNullOrWhiteSpace(code))
            {
                response.Message = "Mã voucher không được để trống.";
                return response;
            }

            var voucher = await _voucherRepository.GetByCodeAsync(code);
            if (voucher == null)
            {
                response.Message = "Mã voucher không tồn tại.";
                return response;
            }

            if (!voucher.IsActive)
            {
                response.Message = "Voucher này hiện không hoạt động.";
                return response;
            }

            var now = DateTime.Now;
            if (voucher.StartDate.HasValue && voucher.StartDate.Value > now)
            {
                response.Message = "Voucher chưa đến thời gian bắt đầu áp dụng.";
                return response;
            }

            if (voucher.ExpiryDate.HasValue && voucher.ExpiryDate.Value < now)
            {
                response.Message = "Voucher đã hết hạn sử dụng.";
                return response;
            }

            if (voucher.UsageLimit.HasValue && voucher.UsageCount >= voucher.UsageLimit.Value)
            {
                response.Message = "Voucher đã vượt quá lượt sử dụng tối đa.";
                return response;
            }

            if (voucher.MinOrderValue.HasValue && orderValue < voucher.MinOrderValue.Value)
            {
                response.Message = $"Giá trị đơn hàng tối thiểu để sử dụng voucher này là {voucher.MinOrderValue.Value:N0} VNĐ.";
                return response;
            }

            // Calculate discount
            double discountAmount = 0;
            if (voucher.DiscountType == 1) // Percentage
            {
                discountAmount = orderValue * (voucher.DiscountValue / 100.0);
                if (voucher.MaxDiscountAmount.HasValue)
                {
                    discountAmount = Math.Min(discountAmount, voucher.MaxDiscountAmount.Value);
                }
            }
            else if (voucher.DiscountType == 2) // Fixed amount
            {
                discountAmount = voucher.DiscountValue;
            }

            discountAmount = Math.Min(discountAmount, orderValue); // Cannot discount more than order value
            
            response.IsValid = true;
            response.Message = "Áp dụng voucher thành công.";
            response.DiscountAmount = discountAmount;
            response.FinalAmount = orderValue - discountAmount;
            return response;
        }

        private VoucherDTO MapToDTO(Voucher v)
        {
            return new VoucherDTO
            {
                VoucherId = v.VoucherId,
                Code = v.Code,
                DiscountType = v.DiscountType,
                DiscountValue = v.DiscountValue,
                MinOrderValue = v.MinOrderValue,
                MaxDiscountAmount = v.MaxDiscountAmount,
                StartDate = v.StartDate,
                ExpiryDate = v.ExpiryDate,
                UsageLimit = v.UsageLimit,
                UsageCount = v.UsageCount,
                IsActive = v.IsActive
            };
        }
    }
}
