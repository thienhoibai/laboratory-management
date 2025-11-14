using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestOrder.Application.DTOs.Bookings;
using TestOrder.Infrastructure.Data;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Presentation.Controllers
{
    public record UpdateStatusRequest(byte Status);

    [ApiController]
    [Route("api/bookings")] 
    public class BookingStatusController : ControllerBase
    {
        private readonly TestOrderDBContext _db;
        public BookingStatusController(TestOrderDBContext db) { _db = db; }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusRequest body)
        {
            var booking = await _db.Set<Booking>().SingleOrDefaultAsync(b => b.BookingId == id);
            if (booking == null) return NotFound();

            var current = booking.Status;

            // Rule: chỉ cho đặt InProgress (Start run) khi hiện trạng là ReadyForInstrument (5)
            if ((BookingStatusEnum)body.Status == BookingStatusEnum.InProgress
                && current != (byte)BookingStatusEnum.ReadyForInstrument)
            {
                return Conflict("Booking must be ReadyForInstrument (5) to start running.");
            }

            booking.Status = body.Status;
            await _db.SaveChangesAsync();
            return Ok(new { bookingId = id, status = booking.Status });
        }
    }
}
