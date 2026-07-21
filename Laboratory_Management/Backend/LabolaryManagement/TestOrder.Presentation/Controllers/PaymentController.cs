using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using TestOrder.Application.DTOs.Payment;
using TestOrder.Application.Services;
using TestOrder.Application.Services.Payment;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/payments")]
    [ApiController]
    [Tags("Payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService vnPayService;
        private readonly PaymentService paymentService;
        private readonly string frontendBaseUrl;
        private string paymentSuccess => $"{frontendBaseUrl}/booking/successBooking?bookingId=";
        private string paymentFaile => $"{frontendBaseUrl}/booking/failBooking?bookingId=";

        public PaymentController(IVnPayService vnPayService, PaymentService paymentService, IConfiguration configuration)
        {
            this.vnPayService = vnPayService;
            this.paymentService = paymentService;
            this.frontendBaseUrl = (configuration["FrontendUrl"] ?? "https://laboratory-management-fe.vercel.app").TrimEnd('/');
        }

        private Guid GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
            return id != null && Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }

        [HttpGet]
        [Authorize(Policy = "perm:Payment.List")]
        public async Task<IActionResult> GetAllPayments([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var payments = await paymentService.GetAllPaymentEnvoicePaged(pageNumber, pageSize);
            return Ok(payments);
        }

        [HttpGet("/api/bookings/{bookingId:guid}/payments")]
        [Authorize(Policy = "perm:Payment.ByBooking.View")]
        public async Task<IActionResult> GetPaymentByBookingId([FromRoute] Guid bookingId)
        {
            try
            {
                var payment = await paymentService.GetByBookingIdAsync(bookingId);
                return Ok(payment);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving payment: {ex.Message}");
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "perm:Payment.View")]
        public async Task<IActionResult> GetPaymentById([FromRoute] int id)
        {
            var payment = await paymentService.GetPaymentEnvoiceByIdAsync(id);
            return Ok(payment);
        }

        [HttpPost("vnpay-url")]
        [Authorize(Policy = "perm:Payment.Create")]
        public IActionResult CreatePaymentUrl(PaymentRequestDTO model)
        {
            try
            {
                var paymentUrl = vnPayService.CreatePaymentUrl(model, HttpContext);
                return Ok(paymentUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating payment URL: {ex.Message}");
            }
        }

        [HttpGet("vnpay-return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var vnPayResponse = vnPayService.PaymentExecute(Request.Query);

                long amount = 0;
                if (Request.Query.ContainsKey("vnp_Amount"))
                {
                    amount = Convert.ToInt64(Request.Query["vnp_Amount"]) / 100;
                }

                if (vnPayResponse.IsSuccess && vnPayResponse.ResponseCode == "00")
                {
                    string token = vnPayResponse.OrderId;
                    var updatePaymentDto = new UpdatePaymentDTO
                    {
                        Method = vnPayResponse.PaymentMethod,
                        Status = (byte?)PaymentStatusEnum.Completed,
                        PaidAt = DateTime.Now,
                        Amount = amount
                    };
                    string id = await paymentService.UpdatePaymentAsync(token, updatePaymentDto, vnPayResponse.IsSuccess);
                    return Redirect($"{paymentSuccess}{id}&Amount={amount}");
                }
                else
                {
                    string token = vnPayResponse.OrderId;
                    var updatePaymentDto = new UpdatePaymentDTO
                    {
                        Status = (byte?)PaymentStatusEnum.Failed,
                    };
                    string id = await paymentService.UpdatePaymentAsync(token, updatePaymentDto, vnPayResponse.IsSuccess);
                    return Redirect($"{paymentFaile}{id}&Amount={amount}");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing payment return: {ex.Message}");
            }
        }
    }
}
