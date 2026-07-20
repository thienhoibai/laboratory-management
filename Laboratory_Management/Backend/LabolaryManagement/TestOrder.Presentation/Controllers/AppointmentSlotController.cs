using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TestOrder.Application.Services;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/appointment-slots")]
    [ApiController]
    [Tags("Appointment Slots")]
    public class AppointmentSlotController : ControllerBase
    {
        private readonly AppointmentSlotService _appointmentSlotService;
        public AppointmentSlotController(AppointmentSlotService appointmentSlotService)
        {
            _appointmentSlotService = appointmentSlotService;
        }

        [HttpGet]
        [Authorize(Policy = "perm:AppointmentSlot.List")]
        public async Task<IActionResult> GetAppointmentSlots(
            [FromQuery] DateOnly? date,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (date.HasValue)
            {
                var response = await _appointmentSlotService.GetAppointmentSlotsByDateAsync(date.Value, pageNumber, pageSize);
                return Ok(response);
            }
            else
            {
                var response = await _appointmentSlotService.GetAllAppointmentSlot(pageNumber, pageSize);
                return Ok(response);
            }
        }

        [HttpGet("bookings-count")]
        [Authorize(Policy = "perm:AppointmentSlot.CountByDate.View")]
        public async Task<IActionResult> GetBookingsCountForSlot([FromQuery] List<Guid> appointmentSlotIds)
        {
            var response = await _appointmentSlotService.GetBookingsCountForMultipleSlotsAsync(appointmentSlotIds);
            return Ok(response);
        }

        [HttpGet("bookings-count-summary")]
        [Authorize(Policy = "perm:AppointmentSlot.CountAll.View")]
        public async Task<IActionResult> GetBookingsCountForAllSlots([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var response = await _appointmentSlotService.GetBookingCountForAllSlotAsync(pageNumber, pageSize);
            return Ok(response);
        }
    }
}
