using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestOrder.Application.Services.Booking;
using TestOrder.Application.DTOs.Bookings;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public BookingController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public IActionResult CreateBooking([FromBody] BookingRequestDTO createBookingDto)
        {
            var bookingId = _bookingService.CreateNewBooking(createBookingDto);
            return Ok(bookingId);
        }
    }
}
