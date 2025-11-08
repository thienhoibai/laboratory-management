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

        public PaymentController(IVnPayService vnPayService, PaymentService paymentService)
        {
            this.vnPayService = vnPayService;
            this.paymentService = paymentService;
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllPayments([FromQuery] int pageNumber)
        {
            var payments = await paymentService.GetAllPaymentEnvoicePaged(pageNumber);
            return Ok(payments);
        }
        [HttpGet]
        [Route("by-booking")]
        public async Task<IActionResult> GetPaymentByBookingId([FromQuery] Guid bookingId)
        {
            var payment = await paymentService.GetByBookingIdAsync(bookingId);
            return Ok(payment);
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
                if (vnPayResponse.IsSuccess)
                {
                    int paymentNo = int.Parse(vnPayResponse.OrderId);
                    var updatePaymentDto = new UpdatePaymentDTO
                    {
                        Method = vnPayResponse.PaymentMethod,
                        Status = (byte?)PaymentStatusEnum.Completed,
                        PaidAt = DateTime.Now,
                        Token = vnPayResponse.PaymentMethod + "_" + "TETSTSTTSTS"
                    };
                    await paymentService.UpdatePaymentAsync(paymentNo, updatePaymentDto);

                    return Ok("Payment successful");
                }
                else
                {
                    int paymentNo = int.Parse(vnPayResponse.OrderId);
                    var updatePaymentDto = new UpdatePaymentDTO
                    {
                        Status = (byte?)PaymentStatusEnum.Failed,            
                    };
                    await paymentService.UpdatePaymentAsync(paymentNo, updatePaymentDto);
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
