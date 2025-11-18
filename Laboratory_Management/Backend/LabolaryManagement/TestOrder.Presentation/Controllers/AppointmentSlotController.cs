using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestOrder.Application.Services;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Khung giờ hẹn")]
    public class AppointmentSlotController : ControllerBase
    {
        private readonly AppointmentSlotService _appointmentSlotService;
        public AppointmentSlotController(AppointmentSlotService appointmentSlotService)
        {
            _appointmentSlotService = appointmentSlotService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAppointmentSlots([FromQuery] int pageNumber)
        {
            var response = await _appointmentSlotService.GetAllAppointmentSlot(pageNumber);
            return Ok(response);
        }

        [HttpGet("by-date")]
        public async Task<IActionResult> GetAppointmentSlotsByDate([FromQuery] DateOnly appointmentDate, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var response = await _appointmentSlotService.GetAppointmentSlotsByDateAsync(appointmentDate, pageNumber, pageSize);
            return Ok(response);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetBookingsCountForSlot([FromQuery] List<Guid> appointmentSlotIds)
        {
            var response = await _appointmentSlotService.GetBookingsCountForMultipleSlotsAsync(appointmentSlotIds);
            return Ok(response);
        }

        [HttpGet("count-all")]
        public async Task<IActionResult> GetBookingsCountForAllSlots([FromQuery] int pageNumber)
        {
            var response = await _appointmentSlotService.GetBookingCountForAllSlotAsync(pageNumber);
            return Ok(response);
        }

    }
}
