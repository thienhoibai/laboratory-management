using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Instrument.Presentation.Controllers;

public record ForInstrumentItemBridge(int ParameterId, string ParameterName, string? Unit, decimal? RefMin, decimal? RefMax, List<long> TestBookingNos, List<int> CatalogIds);
public record ForInstrumentRes(Guid BookingId, string? PatientName, string? PatientSex, List<ForInstrumentItemBridge> Items);
public record ResultItem(long TestBookingNo, int ParameterId, decimal Value);
public record PostResultsReq(Guid BookingId, string InstrumentCode, DateTimeOffset MeasuredAt, List<ResultItem> Results);
public record RunReq(Guid BookingId, string? InstrumentCode);

[ApiController]
[Route("api/instrument/simulations")]
public class SimulationsController : ControllerBase
{
    private readonly HttpClient _http;

    public SimulationsController(IHttpClientFactory f)
    {
        _http = f.CreateClient("testorder");
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunReq req)
    {
        var info = await _http.GetFromJsonAsync<ForInstrumentRes>($"/api/bridge/bookings/{req.BookingId}/for-instrument");
        if (info == null || info.Items.Count == 0) return NotFound("No parameters");

        var measuredAt = DateTimeOffset.UtcNow;
        var code = req.InstrumentCode ?? "INSTR001";
        var results = new List<ResultItem>();

        // Sinh 1 giá trị cho mỗi ParameterId (deterministic theo BookingId, ParameterId) trong [RefMin, RefMax]
        var valueMap = new Dictionary<int, decimal>();
        foreach (var item in info.Items)
        {
            if (!valueMap.ContainsKey(item.ParameterId))
            {
                var v = GenerateDeterministicValue(info.BookingId, item.ParameterId, item.RefMin, item.RefMax);
                valueMap[item.ParameterId] = v;
            }
            var vParam = valueMap[item.ParameterId];
            foreach (var no in item.TestBookingNos)
            {
                results.Add(new ResultItem(no, item.ParameterId, vParam));
            }
        }

        var payload = new PostResultsReq(info.BookingId, code, measuredAt, results);
        var res = await _http.PostAsJsonAsync($"/api/bridge/bookings/{info.BookingId}/results", payload);
        res.EnsureSuccessStatusCode();

        return Ok(new { status = "OK", bookingId = info.BookingId, count = results.Count });
    }

    private static decimal GenerateDeterministicValue(Guid bookingId, int parameterId, decimal? refMin, decimal? refMax)
    {
        // Tạo seed từ BookingId + ParameterId
        unchecked
        {
            int hash = bookingId.GetHashCode();
            hash = (hash * 397) ^ parameterId;
            var rng = new Random(hash);
            // Nếu có RefMin/RefMax → phân bố gần giữa khoảng, có nhiễu nhỏ
            if (refMin.HasValue && refMax.HasValue && refMax > refMin)
            {
                var min = refMin.Value; var max = refMax.Value;
                var mid = (min + max) / 2m;
                var span = (max - min) / 6m; // ~3 sigma trong khoảng
                // Box-Muller đơn giản từ 2 uniform
                double u1 = 1.0 - rng.NextDouble();
                double u2 = 1.0 - rng.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                var val = mid + (decimal)randStdNormal * span;
                if (val < min) val = min + (decimal)rng.NextDouble() * (mid - min) / 3m;
                if (val > max) val = max - (decimal)rng.NextDouble() * (max - mid) / 3m;
                return Math.Round(val, 3);
            }
            // fallback 0..1
            return Math.Round((decimal)rng.NextDouble(), 3);
        }
    }
}
