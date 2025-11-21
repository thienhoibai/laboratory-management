using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Instrument.Presentation.Controllers;

// DTO khớp với TestOrder Bridge response
public record ForInstrumentItemDto(
    long TestBookingNo,
    int CatalogId,
    int ParameterId, 
    string ParameterName, 
    string? Unit, 
    decimal? RefMin, 
    decimal? RefMax
);

public record DuplicateGroup(
    int ParameterId,
    List<long> TestBookingNos
);

public record ForInstrumentResponse(
    Guid BookingId, 
    byte Status,  // Changed from int to byte to match TestOrder
    string? PatientName, 
    List<ForInstrumentItemDto> Items,
    List<DuplicateGroup> DuplicateGroups  // Added missing property
);

// DTO khớp với TestOrder Bridge /results request
public record IngestItem(
    long TestBookingNo,
    int ParameterId,
    string ParameterName,
    decimal Value,
    string? Unit,
    decimal? RefMin,
    decimal? RefMax
);

public record IngestRequest(
    string InstrumentCode, 
    DateTimeOffset MeasuredAt, 
    List<IngestItem> Items
);

public record RunReq(Guid BookingId, string? InstrumentCode);

[ApiController]
[Route("api/instrument/simulations")]
public class SimulationsController : ControllerBase
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;

    public SimulationsController(IHttpClientFactory f, IConfiguration cfg)
    {
        _http = f.CreateClient("testorder");
        _cfg = cfg;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunReq req)
    {
        // 1. Lấy danh sách parameters từ TestOrder
        var info = await _http.GetFromJsonAsync<ForInstrumentResponse>(
            $"/api/bridge/bookings/{req.BookingId}/for-instrument");
        
        if (info == null || info.Items.Count == 0) 
            return NotFound("No parameters");

        var measuredAt = DateTimeOffset.UtcNow;
        var code = req.InstrumentCode ?? "INSTR001";
        var results = new List<IngestItem>();

        // 2. Sinh random kết quả cho mỗi item
        foreach (var item in info.Items)
        {
            var value = GenerateDeterministicValue(
                info.BookingId, 
                item.ParameterId, 
                item.RefMin, 
                item.RefMax
            );
            
            results.Add(new IngestItem(
                item.TestBookingNo, 
                item.ParameterId,
                item.ParameterName,
                value,
                item.Unit,
                item.RefMin,
                item.RefMax
            ));
        }

        // 3. Gửi kết quả về TestOrder với X-Bridge-Token header
        var payload = new IngestRequest(code, measuredAt, results);
        
        // Tạo HttpRequestMessage để thêm custom header
        var request = new HttpRequestMessage(HttpMethod.Post, 
            $"/api/bridge/bookings/{info.BookingId}/results")
        {
            Content = JsonContent.Create(payload)
        };
        
        // Thêm X-Bridge-Token header từ config
        var bridgeToken = _cfg["Bridge:Token"];
        if (!string.IsNullOrEmpty(bridgeToken))
        {
            request.Headers.Add("X-Bridge-Token", bridgeToken);
        }
        
        var res = await _http.SendAsync(request);
        res.EnsureSuccessStatusCode();

        return Ok(new { 
            status = "OK", 
            bookingId = info.BookingId, 
            count = results.Count 
        });
    }

    private static decimal GenerateDeterministicValue(
        Guid bookingId, 
        int parameterId, 
        decimal? refMin, 
        decimal? refMax)
    {
        unchecked
        {
            int hash = bookingId.GetHashCode();
            hash = (hash * 397) ^ parameterId;
            var rng = new Random(hash);
            
            if (refMin.HasValue && refMax.HasValue && refMax > refMin)
            {
                var min = refMin.Value; 
                var max = refMax.Value;
                var mid = (min + max) / 2m;
                var span = (max - min) / 6m;
                
                // Box-Muller
                double u1 = 1.0 - rng.NextDouble();
                double u2 = 1.0 - rng.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                var val = mid + (decimal)randStdNormal * span;
                
                if (val < min) val = min + (decimal)rng.NextDouble() * (mid - min) / 3m;
                if (val > max) val = max - (decimal)rng.NextDouble() * (max - mid) / 3m;
                
                return Math.Round(val, 3);
            }
            
            return Math.Round((decimal)rng.NextDouble(), 3);
        }
    }
}
