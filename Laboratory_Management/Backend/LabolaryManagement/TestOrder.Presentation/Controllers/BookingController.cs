using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestOrder.Application.Services.Booking;
using TestOrder.Application.DTOs.Bookings;
using System.Threading.Tasks;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Đặt lịch xét nghiệm")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public BookingController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        [Tags("Lấy thông tin Lịch hẹn xét nghiệm theo mã tham chiếu")]
        public IActionResult GetBookingInfo([FromQuery] Guid bookingId)
        {
            var response = _bookingService.GetBookingByIdAsync(bookingId).Result;
            return Ok(response);
        }
        [HttpGet("patient")]
        [Tags("Lấy thông tin lịch hẹn xét nghiệm theo mã bệnh nhân")]
        public IActionResult GetBookingsByPatientId([FromQuery] long patientId)
        {
            var response = _bookingService.GetBookingsByPatientIdAsync(patientId);
            return Ok(response);
        }


        [HttpPost]
        [Tags("Tạo mới đặt lịch xét nghiệm")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDTO createBookingDto)
        {
            var response = await _bookingService.CreateNewBooking(createBookingDto);
            return Ok(response);
        }

    }
}
