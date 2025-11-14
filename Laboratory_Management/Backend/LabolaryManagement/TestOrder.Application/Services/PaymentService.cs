using Azure.Core.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Application.DTOs.Payment;
using TestOrder.Application.Services.Payment;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Models.VnPayModels;
using TestOrder.Infrastructure.Repository;
using TestOrder.Infrastructure.Repository.VnPayLibs;

namespace TestOrder.Application.Services
{
    public class PaymentService : IVnPayService
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly TestOrder.Infrastructure.Repository.BookingRepository _bookingRepository;
        private readonly IConfiguration configuration;

        public PaymentService(IConfiguration configuration, PaymentRepository paymentRepository, TestOrder.Infrastructure.Repository.BookingRepository bookingRepository)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetAllPaymentEnvoicePaged(int pageNumber)
        {
            var payments = await _paymentRepository.GetAllPagedAsync(pageNumber);
            return payments.Select(p => new PaymentResponseDTO
            {
                BookingId = p.BookingId,
                Method = p.Method,
                Amount = p.Amount,
                Status = p.Status.HasValue ? ((PaymentStatusEnum)p.Status.Value).ToString() : "Unknown",
                CreatedAt = p.CreatedAt,
                PaidAt = p.PaidAt,
                Token = p.Token
            });
        }

        public async Task<PaymentResponseDTO?> GetPaymentEnvoiceByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
            {
                throw new Exception("Payment not found");
            }
                

            var entity = new PaymentResponseDTO
            {
                BookingId = payment.BookingId,
                Method = payment.Method,
                Amount = payment.Amount,
                Status = payment.Status.HasValue ? ((PaymentStatusEnum)payment.Status.Value).ToString() : "Unknown",
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt,
                Token = payment.Token
            };

            return entity;

        }

        public async Task<PaymentResponseDTO?> GetByBookingIdAsync (Guid bookingId)
        {
            var envoice = await _paymentRepository.GetByBookingIdAsync(bookingId);

            if (envoice == null)
            {
                throw new Exception("Payment not found");
            }
            var entity = new PaymentResponseDTO
            {
                BookingId = envoice.BookingId,
                Method = envoice.Method,
                Amount = envoice.Amount,
                Status = envoice.Status.HasValue ? ((PaymentStatusEnum)envoice.Status.Value).ToString() : "Unknown",
                CreatedAt = envoice.CreatedAt,
                PaidAt = envoice.PaidAt,
                Token = envoice.Token
            };
            return entity;

        }
        #region Processing Payment
        private async Task<PaymentEnvoice> CreatePaymentAsync(PaymentRequestDTO model)
        {
            var envoice = await _paymentRepository.GetByBookingIdAsync(model.BookingId);
            var tick = DateTime.Now.Ticks.ToString();
            if (envoice != null)
            {
                await _paymentRepository.DeleteAsync(envoice);
            }
            var payment = new PaymentEnvoice
            {
                BookingId = model.BookingId,
                Amount = model.Amount,
                Method = "",
                Status = (byte?)PaymentStatusEnum.Pending,
                CreatedAt = DateTime.Now,
                Token = tick,
            };
            await _paymentRepository.AddAsync(payment);
            return payment;
        }

        public string CreatePaymentUrl(PaymentRequestDTO model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            
            var vnPay = new VnPayRepository();
            var urlCallBack = configuration["VnPay:PaymentReturnUrl"];

            var payment = CreatePaymentAsync(model).Result;

            vnPay.AddRequestData("vnp_Version", configuration["VnPay:Version"]);
            vnPay.AddRequestData("vnp_Command", configuration["VnPay:Command"]);
            vnPay.AddRequestData("vnp_TmnCode", configuration["VnPay:TmnCode"]);
            vnPay.AddRequestData("vnp_Amount", ((int)payment.Amount * 100).ToString());
            vnPay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            vnPay.AddRequestData("vnp_CurrCode", configuration["VnPay:CurrCode"]);
            vnPay.AddRequestData("vnp_IpAddr", vnPay.GetIpAddress(context));
            vnPay.AddRequestData("vnp_Locale", configuration["VnPay:Locale"]);
            vnPay.AddRequestData("vnp_OrderInfo", $"Thanh toan qua VNPay ma lich dat {model.BookingId} Tong tien {model.Amount}");
            vnPay.AddRequestData("vnp_OrderType", "other");
            vnPay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            vnPay.AddRequestData("vnp_TxnRef", payment.Token);

            var paymentUrl = vnPay.CreateRequestUrl(configuration["VnPay:BaseUrl"], configuration["VnPay:HashSecret"]);
            return paymentUrl;

        }

        public VnPayResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnPay = new VnPayRepository();
            var hashSecret = configuration["VnPay:HashSecret"];
            var responseData = vnPay.GetFullResponseData(collections, hashSecret);

            return responseData;
        }

        public async Task<bool> UpdatePaymentAsync(string token, UpdatePaymentDTO dto)
        {
            var payment = await _paymentRepository.GetByTokenAsync(token);
            if (payment == null)
            {
                return false;
            }
            payment.Method = dto.Method ?? payment.Method;
            payment.Status = dto.Status ?? payment.Status;
            payment.PaidAt = dto.PaidAt ?? payment.PaidAt;
            await _paymentRepository.UpdateAsync(payment);

            // Nếu Paid thành công → đổi Booking.Status = ReadyForInstrument (5)
            if (payment.Status == (byte?)PaymentStatusEnum.Completed && payment.BookingId != Guid.Empty)
            {
                var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
                if (booking != null)
                {
                    booking.Status = (byte?)TestOrder.Application.DTOs.Bookings.BookingStatusEnum.ReadyForInstrument;
                    await _bookingRepository.UpdateAsync(booking);
                }
            }
            return true;
        }
    }
    #endregion
}
