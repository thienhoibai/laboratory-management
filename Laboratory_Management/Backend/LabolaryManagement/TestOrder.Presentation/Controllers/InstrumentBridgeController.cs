using Microsoft.AspNetCore.Mvc;
using TestOrder.Application.DTOs.InstrumentBridge;
using TestOrder.Application.Services.InstrumentBridge;

namespace TestOrder.Presentation.Controllers;

[ApiController]
[Route("api/bridge/bookings")]
[Tags("Instrument Bridge")]
public class InstrumentBridgeController : ControllerBase
{
    private readonly InstrumentBridgeService _svc;
    private readonly IConfiguration _cfg;

    public InstrumentBridgeController(InstrumentBridgeService svc, IConfiguration cfg)
    {
        _svc = svc;
        _cfg = cfg;
    }

    /// <summary>
    /// GET /api/bridge/bookings/{bookingId}/for-instrument
    /// Trả danh sách test con & tham số để máy sinh kết quả (kèm RefMin/RefMax)
    /// </summary>
    [HttpGet("{bookingId:guid}/for-instrument")]
    public async Task<IActionResult> ForInstrument(Guid bookingId)
    {
        var res = await _svc.GetForInstrumentAsync(bookingId);

        if (res == null)
            return Conflict(new { error = "Booking not ready (Status must be 4) or not found." });

        return Ok(res);
    }

    /// <summary>
    /// POST /api/bridge/bookings/{bookingId}/results
    /// Instrument gửi kết quả JSON, TestOrder validate + upsert + báo completed
    /// </summary>
    [HttpPost("{bookingId:guid}/results")]
    public async Task<IActionResult> Ingest(Guid bookingId, [FromBody] IngestRequest body)
    {
        // Auth: kiểm tra X-Bridge-Token (internal security)
        var expectedToken = _cfg["Bridge:Token"];
        var providedToken = Request.Headers["X-Bridge-Token"].FirstOrDefault();

        if (!string.IsNullOrEmpty(expectedToken) && !string.Equals(expectedToken, providedToken))
            return Unauthorized(new { error = "Invalid bridge token." });

        // Validate & ingest
        var res = await _svc.IngestResultsAsync(bookingId, body);

        if (res == null)
            return Conflict(new { error = "Booking not ready (Status must be 4) or not found." });

        return Ok(new
        {
            accepted = res.Accepted,
            rejected = res.Rejected,
            completed = res.Completed,
            missingPairs = res.MissingPairs.Select(x => new
            {
                testBookingNo = x.TestBookingNo,
                parameterId = x.ParameterId
            })
        });
    }

    /// <summary>
    /// GET /api/bridge/bookings/{bookingId}/expected
    /// Trả tổng số cặp cần có để booking xem đã đủ chưa (hỗ trợ FE/QA)
    /// </summary>
    [HttpGet("{bookingId:guid}/expected")]
    public async Task<IActionResult> Expected(Guid bookingId)
    {
        var res = await _svc.GetExpectedAsync(bookingId);

        if (res == null)
            return Conflict(new { error = "Booking not ready." });

        return Ok(res);
    }
}
