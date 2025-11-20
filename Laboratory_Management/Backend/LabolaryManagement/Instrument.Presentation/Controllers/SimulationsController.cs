using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Instrument.Infrastructure;
using Instrument.Domain.Entities;
using Instrument.Application.Results;
using System.Net.Http.Json;

namespace Instrument.Presentation.Controllers;

public record ForInstrumentRes(Guid BookingId, string? PatientName, string? PatientSex, List<ForInstrumentItemDto> Items);
public record ResultItem(long TestBookingNo, int ParameterId, decimal Value);
public record PostResultsReq(Guid BookingId, string InstrumentCode, DateTimeOffset MeasuredAt, List<ResultItem> Results);
public record RunReq(Guid BookingId, string? InstrumentCode);

[ApiController]
[Route("api/instrument/simulations")]
public class SimulationsController : ControllerBase
{
    private readonly HttpClient _http;
    private readonly InstrumentDbContext _db;
    private readonly IResultGenerator _gen;

    public SimulationsController(IHttpClientFactory f, InstrumentDbContext db, IResultGenerator gen)
    {
        _http = f.CreateClient("testorder");
        _db = db; _gen = gen;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunReq req)
    {
        var info = await _http.GetFromJsonAsync<ForInstrumentRes>($"/api/bridge/bookings/{req.BookingId}/for-instrument");
        if (info == null || info.Items.Count == 0) return NotFound("No parameters");

        var measuredAt = DateTimeOffset.UtcNow;
        var valueByParam = _gen.GenerateValues(info.Items, info.PatientSex);

        var code = req.InstrumentCode ?? "INSTR001";
        var results = new List<ResultItem>();

        foreach (var item in info.Items)
        {
            var v = valueByParam[item.ParameterId];

            _db.InstrumentResults.Add(new InstrumentResult {
                InstrumentCode = code,
                BookingId      = info.BookingId,
                TestBookingNo  = item.TestBookingNo,
                ParameterId    = item.ParameterId,
                ParameterName  = item.ParameterName,
                Value          = v,
                Unit           = item.Unit,
                ReferenceRange = item.ReferenceRange,
                MeasuredAt     = measuredAt.UtcDateTime,
                CreatedAt      = DateTime.UtcNow
            });

            results.Add(new ResultItem(item.TestBookingNo, item.ParameterId, v));
        }
        await _db.SaveChangesAsync();

        var payload = new PostResultsReq(info.BookingId, code, measuredAt, results);
        var res = await _http.PostAsJsonAsync($"/api/bridge/bookings/{info.BookingId}/results", payload);
        res.EnsureSuccessStatusCode();

        return Ok(new { status = "OK", bookingId = info.BookingId });
    }
}
