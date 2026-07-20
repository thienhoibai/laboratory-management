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
    [Route("api/bookings")]
    [ApiController]
    [Tags("Bookings")]
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
        [Authorize(Policy = "perm:Booking.List")]
        public async Task<IActionResult> GetAllBookingsInfoAsync
            ([FromQuery] DateOnly? date,
             [FromQuery] string? keyword,
             [FromQuery] string? sortBy,
             [FromQuery] string? sortDirection,
             [FromQuery] int pageSize,
             [FromQuery] int pageNumber)
        {
            try { 
                var response = await _bookingService.GetAllBookingsAsync
                (date, keyword, sortBy, sortDirection, pageSize, pageNumber);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(400, new ResponseMessage
                {
                    ResponseCode = ResponseCode.NotFound,
                    Message = "An error occurred while processing your request: " + ex.Message
                });
            }
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = "perm:Booking.View")]
        public IActionResult GetBookingInfo([FromRoute] Guid id)
        {
            var response = _bookingService.GetBookingByIdAsync(id).Result;
            return Ok(response);
        }

        [HttpGet("/api/patients/{patientId:guid}/bookings")]
        [Authorize(Policy = "perm:Booking.View.Own")]
        public async Task<IActionResult> GetBookingsByPatientId
            ([FromRoute] Guid patientId, [FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] byte? filterStatus)
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
                    return StatusCode(201, response);
                    
                case ResponseCode.NotFound:
                    return NotFound(response);

                case ResponseCode.BadInstanceState:
                    return BadRequest(response);
            }
        }

        [HttpPost("{id:guid}/check-in")]
        [Authorize(Policy = "perm:Booking.Update.CheckIn")]
        public async Task<IActionResult> CheckInBooking([FromRoute] Guid id)
        {
            var response = await _bookingService.CheckInBooking(id);
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

        [HttpPost("{id:guid}/check-out")]
        [Authorize(Policy = "perm:Booking.Update.CheckOut")]
        public async Task<IActionResult> CheckOutBooking([FromRoute] Guid id)
        {
            var response = await _bookingService.CheckOutBooking(id);
            
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
