  using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestOrder.Application.DTOs.Payment;
using TestOrder.Application.Services;
using TestOrder.Application.Services.Payment;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Thanh Toán và hóa đơn")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService vnPayService;
        private readonly PaymentService paymentService;
        private const string paymentSuccess = "http://localhost:5174/booking/successBooking?bookingId=";

        public PaymentController(IVnPayService vnPayService, PaymentService paymentService)
        {
            this.vnPayService = vnPayService;
            this.paymentService = paymentService;
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllPayments([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var payments = await paymentService.GetAllPaymentEnvoicePaged(pageNumber, pageSize);
            return Ok(payments);
        }
        [HttpGet]
        [Route("by-booking")]
        public async Task<IActionResult> GetPaymentByBookingId([FromQuery] Guid bookingId)
        {
            try
            {
                var payment = await paymentService.GetByBookingIdAsync(bookingId);
                return Ok(payment);

            }
            catch (Exception ex)
            {
               
                return BadRequest($"{ex.Message}");
            }
        }
        [HttpGet]
        [Route("id={id:int}")]
        public async Task<IActionResult> GetPaymentById([FromRoute] int id)
        {
            var payment = await paymentService.GetPaymentEnvoiceByIdAsync(id);
            return Ok(payment);
        }

        [HttpPost]
        [Route("vnpay-url")]
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

        [HttpGet]
        [Route("vnpay-return")]
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

                if (vnPayResponse.IsSuccess)
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
                    return BadRequest("Payment failed");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing payment return: {ex.Message}");
            }
        }
    }
}
