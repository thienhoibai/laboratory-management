using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Policy = "perm:AppointmentSlot.List")]
        public async Task<IActionResult> GetAllAppointmentSlots([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var response = await _appointmentSlotService.GetAllAppointmentSlot(pageNumber, pageSize);
            return Ok(response);
        }

        [HttpGet("by-date")]
        [Authorize(Policy = "perm:AppointmentSlot.ByDate.View")]
        public async Task<IActionResult> GetAppointmentSlotsByDate([FromQuery] DateOnly appointmentDate, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var response = await _appointmentSlotService.GetAppointmentSlotsByDateAsync(appointmentDate, pageNumber, pageSize);
            return Ok(response);
        }

        [HttpGet("count")]
        [Authorize(Policy = "perm:AppointmentSlot.CountByDate.View")]
        public async Task<IActionResult> GetBookingsCountForSlot([FromQuery] List<Guid> appointmentSlotIds)
        {
            var response = await _appointmentSlotService.GetBookingsCountForMultipleSlotsAsync(appointmentSlotIds);
            return Ok(response);
        }

        [HttpGet("count-all")]
        [Authorize(Policy = "perm:AppointmentSlot.CountAll.View")]
        public async Task<IActionResult> GetBookingsCountForAllSlots([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var response = await _appointmentSlotService.GetBookingCountForAllSlotAsync(pageNumber,pageSize);
            return Ok(response);
        }
    }
}
