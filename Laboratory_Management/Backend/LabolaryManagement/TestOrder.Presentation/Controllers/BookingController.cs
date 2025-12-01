using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TestOrder.Application.Services.Booking;
using TestOrder.Application.DTOs.Bookings;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using System.Security.Claims;

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

        private Guid GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                     ?? User.FindFirst("sub")?.Value;
            return id != null && Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }

        [HttpGet]
        [Route("info")]
        [Authorize(Policy = "perm:Booking.List")]
        public async Task<IActionResult> GetAllBookingsInfoByDateAsync
            ([FromQuery] DateOnly date,
             [FromQuery] string? keyword,
             [FromQuery] string? sortBy,
             [FromQuery] string? sortDirection,
             [FromQuery] int pageSize,
             [FromQuery] int pageNumber)
        {
            try { 
                var response = await _bookingService.GetAllBookingsByDateAsync
                (date, keyword, sortBy, sortDirection, pageSize, pageNumber);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(400, new ResponseMessage
                {
                    ResponseCode = ResponseCode.BadInstanceState,
                    Message = "An error occurred while processing your request: " + ex.Message
                });
            }
        }

        [HttpGet]
        [Authorize(Policy = "perm:Booking.View")]
        public IActionResult GetBookingInfo([FromQuery] Guid bookingId)
        {
            var response = _bookingService.GetBookingByIdAsync(bookingId).Result;
            return Ok(response);
        }


        [HttpGet("patient")]
        [Authorize(Policy = "perm:Booking.View.Own")]
        public async Task<IActionResult> GetBookingsByPatientId
            ([FromQuery] Guid patientId, [FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] byte? filterStatus)
        {
            try
            {
                var response = await _bookingService.GetBookingsByPatientIdAsync(patientId, pageNumber, pageSize, filterStatus);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ResponseMessage
                {
                    ResponseCode = ResponseCode.NotFound,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Policy = "perm:Booking.Create")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDTO createBookingDto)
        {
            var response = await _bookingService.CreateBookingAsync(createBookingDto);
            switch (response.ResponseCode)
            {
                default:
                    return Ok(response);
                    
                case ResponseCode.NotFound:
                    return NotFound(response);

                case ResponseCode.BadInstanceState:
                    return BadRequest(response);
            }
        }

        [HttpPut]
        [Route("check-in")]
        [Authorize(Policy = "perm:Booking.Update.CheckIn")]
        public async Task<IActionResult> CheckInBooking([FromQuery] Guid bookingId)
        {
            var response = await _bookingService.CheckInBooking(bookingId);
            switch (response.ResponseCode)
            {
                case ResponseCode.NotFound:
                    return NotFound(response);
                case ResponseCode.BadInstanceState:
                    return BadRequest(response);
                default:
                    return Ok(response);
            }
        }

        [HttpPut]
        [Route("check-out")]
        [Authorize(Policy = "perm:Booking.Update.CheckOut")]
        public async Task<IActionResult> CheckOutBooking([FromQuery] Guid bookingId)
        {
            var response = await _bookingService.CheckOutBooking(bookingId);
            
            switch (response.ResponseCode)
            {
                case ResponseCode.NotFound:
                    return NotFound(response);
                case ResponseCode.BadInstanceState:
                    return BadRequest(response);
                default:
                    return Ok(response);

            }
        }
    }
}
