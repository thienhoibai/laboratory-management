using Microsoft.AspNetCore.Mvc;
using TestOrder.Application.DTOs.InstrumentBridge;
using TestOrder.Application.InstrumentBridge;

namespace TestOrder.Presentation.Controllers
{
    [ApiController]
    [Route("api/bridge/bookings")]
    public class InstrumentBridgeController : ControllerBase
    {
        private readonly InstrumentBridgeService _svc;
        public InstrumentBridgeController(InstrumentBridgeService svc) => _svc = svc;

        [HttpGet("{bookingId:guid}/for-instrument")]
        public async Task<IActionResult> GetForInstrument(Guid bookingId)
            => Ok(await _svc.GetForInstrumentAsync(bookingId));

        [HttpPost("{bookingId:guid}/results")]
        public async Task<IActionResult> PostResults(Guid bookingId, [FromBody] PostResultsRequest body)
        {
            if (bookingId != body.BookingId) return BadRequest("bookingId mismatch");
            await _svc.SaveResultsFromInstrumentAsync(body);
            return Ok(new { status = "OK" });
        }
    }
}
