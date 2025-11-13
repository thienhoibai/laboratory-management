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
        [Route("info")]
        public async Task<IActionResult> GetAllBookings([FromQuery] int pageNumber)
        {
            var response = await _bookingService.GetAllBookingsAsync(pageNumber);
            return Ok(response);
        }

        [HttpGet]
        public IActionResult GetBookingInfo([FromQuery] Guid bookingId)
        {
            var response = _bookingService.GetBookingByIdAsync(bookingId).Result;
            return Ok(response);
        }
        [HttpGet("patient")]
        public async Task<IActionResult> GetBookingsByPatientId([FromQuery] Guid patientId, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var response = await _bookingService.GetBookingsByPatientIdAsync(patientId, pageNumber, pageSize);
            return Ok(response);
        }


        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDTO createBookingDto)
        {
            var response = await _bookingService.CreateNewBooking(createBookingDto);
            return Ok(response);
        }

        [HttpPut]
        [Route("check-in")]
        public async Task<IActionResult> CheckInBooking([FromQuery] Guid bookingId)
        {
            var response = await _bookingService.CheckInBooking(bookingId);
            switch (response)
            {
                case -1:
                    return NotFound("Booking not found");
                case -2:
                    return BadRequest("Booking is not in a state that allows check-in");
                default:
                    return Ok("Check-in successful");
            }
        }

        [HttpPut]
        [Route("check-out")]
        public async Task<IActionResult> CheckOutBooking([FromQuery] Guid bookingId)
        {
            var response = await _bookingService.CheckOutBooking(bookingId);
            switch (response)
            {
                case -1:
                    return NotFound("Booking not found");
                case -2:
                    return BadRequest("Booking is not in a state that allows check-out");
                default:
                    return Ok("Check-out successful");
            }
        }

    }
}
