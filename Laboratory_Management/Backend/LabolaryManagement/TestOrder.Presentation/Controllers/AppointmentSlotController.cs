using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestOrder.Application.Services;
using TestOrder.Application.DTOs.AppointmentSlots;


namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentSlotController : ControllerBase
    {
        private readonly AppointmentSlotService _appointmentSlotService;
        public AppointmentSlotController(AppointmentSlotService appointmentSlotService)
        {
            _appointmentSlotService = appointmentSlotService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAppointmentSlots(int pageNumber)
        {
            var slots = await _appointmentSlotService.GetAllAppointmentSlot(pageNumber);
            return Ok(slots);
        }

        [HttpGet]
        [Route("date/{appointmentDate}")]
        public async Task<IActionResult> GetAppointmentSlotByDate(DateOnly appointmentDate)
        {
            var slot = await _appointmentSlotService.GetAppointmentSlotsByDateAsync(appointmentDate);
            if (slot == null)
            {
                return NotFound();
            }
            return Ok(slot);
        }

        [HttpPost]
        public async Task<IActionResult> AddAppointmentSlot([FromBody] AppointmentSlotDTO appointmentSlot)
        {
            await _appointmentSlotService.AddAppointmentSlotAsync(appointmentSlot);
            return CreatedAtAction(nameof(GetAppointmentSlotByDate), new { appointmentDate = appointmentSlot.AppointmentDate }, appointmentSlot);
        }

    }
}
