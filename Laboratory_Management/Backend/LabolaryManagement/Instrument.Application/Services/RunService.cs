using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using Instrument.Application.Runs.DTOs.Requests;
using Instrument.Application.Runs.DTOs.Responses;
using Instrument.Domain.Entities;
using Instrument.Domain.Enums;
using Instrument.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Instrument.Application.Services;

public class RunService
{
    private readonly InstrumentDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _cfg;

    public RunService(InstrumentDbContext db, IHttpClientFactory httpFactory, IConfiguration cfg)
    {
        _db = db;
        _httpFactory = httpFactory;
        _cfg = cfg;
    }

    /// <summary>
    /// POST /api/instrument/runs/start
    /// Tạo run, sinh kết quả random, gửi về TestOrder, và hoàn thành tự động
    /// </summary>
    public async Task<StartRunResponse> StartAndCompleteRunAsync(StartRunRequest req)
    {
        // 1. Gọi TestOrder bridge lấy danh sách Catalog/Parameter
        var testOrderClient = _httpFactory.CreateClient("testorder");
        var bridgeRes = await testOrderClient.GetFromJsonAsync<BridgeResponse>(
            $"/api/bridge/bookings/{req.BookingId}/for-instrument");

        if (bridgeRes == null || bridgeRes.Status != 4)
            return new StartRunResponse(0, RunStatus.Failed, "Booking not ready (Status must be 4)", 0);

        // ✅ THÊM VALIDATION: Kiểm tra Items có dữ liệu không
        if (bridgeRes.Items == null || bridgeRes.Items.Count == 0)
        {
            return new StartRunResponse(0, RunStatus.Failed, 
                $"No test parameters found for BookingId {req.BookingId}. Please ensure the booking has tests assigned with parameters.", 
                0);
        }

        // 2. Tạo Run
        var run = new InstrumentRun
        {
            BookingId = req.BookingId,
            InstrumentCode = req.InstrumentCode,
            Status = RunStatus.Running,
            StartedAt = DateTime.UtcNow
        };

        _db.InstrumentRuns.Add(run);
        await _db.SaveChangesAsync();

        // 3. Sinh kết quả random deterministic
        var results = new List<IngestItemDto>();
        foreach (var item in bridgeRes.Items)
        {
            var value = GenerateDeterministicValue(req.BookingId, item.ParameterId, item.RefMin, item.RefMax);
            results.Add(new IngestItemDto(
                item.TestBookingNo,
                item.ParameterId,
                item.ParameterName,
                value,
                item.Unit,
                item.RefMin,
                item.RefMax
            ));
        }

        // 4. Gửi kết quả về TestOrder
        var ingestReq = new
        {
            instrumentCode = run.InstrumentCode,
            measuredAt = DateTimeOffset.UtcNow,
            items = results
        };

        var bridgeToken = _cfg["Bridge:Token"];
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/bridge/bookings/{run.BookingId}/results")
        {
            Content = JsonContent.Create(ingestReq)
        };
        
        if (!string.IsNullOrEmpty(bridgeToken))
            request.Headers.Add("X-Bridge-Token", bridgeToken);
    
        var response = await testOrderClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // 5. Hoàn thành run tự động
        run.Status = RunStatus.Completed;
        run.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new StartRunResponse(
            run.RunId, 
            RunStatus.Completed, 
            $"Run completed successfully. Sent {results.Count} results to TestOrder.",
            results.Count);
    }

    /// <summary>
    /// GET /api/instrument/runs/{runId}
    /// Trả chi tiết run
    /// </summary>
    public async Task<RunDetailDto?> GetRunDetailAsync(int runId)
    {
        var run = await _db.InstrumentRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RunId == runId);

        if (run == null) return null;

        return new RunDetailDto(
            run.RunId,
            run.BookingId,
            run.InstrumentCode,
            run.Status,
            run.StartedAt,
            run.CompletedAt
        );
    }

    // Helper: Sinh giá trị deterministic với phân bổ: 70% Normal, 15% Low, 15% High
    private static decimal GenerateDeterministicValue(Guid bookingId, int parameterId, decimal? refMin, decimal? refMax)
    {
        unchecked
        {
            int hash = bookingId.GetHashCode();
            hash = (hash * 397) ^ parameterId;
            var rng = new Random(hash);

            var min = refMin ?? 1m;
            var max = refMax ?? 10m;

            if (max <= min) max = min + 1;

            var range = max - min;
            
            // Quyết định xem giá trị sẽ Normal, Low hay High
            var randomPercent = rng.NextDouble();
            
            if (randomPercent < 0.15) // 15% Low (thấp hơn min)
            {
                // Giá trị từ (min - 30% range) đến (min - 5% range)
                var lowValue = min - (decimal)(rng.NextDouble() * 0.25 + 0.05) * range;
                return Math.Round(Math.Max(lowValue, min * 0.5m), 2); // Không giảm quá 50% của min
            }
            else if (randomPercent < 0.30) // 15% High (cao hơn max)
            {
                // Giá trị từ (max + 5% range) đến (max + 30% range)
                var highValue = max + (decimal)(rng.NextDouble() * 0.25 + 0.05) * range;
                return Math.Round(Math.Min(highValue, max * 1.5m), 2); // Không tăng quá 150% của max
            }
            else // 70% Normal (trong khoảng)
            {
                var value = min + (decimal)rng.NextDouble() * range;
                return Math.Round(value, 2);
            }
        }
    }

    // DTOs cho bridge response
    private record BridgeResponse(Guid BookingId, byte Status, List<BridgeItem> Items);
    private record BridgeItem(long TestBookingNo, int CatalogId, int ParameterId, string ParameterName, string? Unit, decimal? RefMin, decimal? RefMax);
    private record IngestItemDto(long TestBookingNo, int ParameterId, string ParameterName, decimal Value, string? Unit, decimal? RefMin, decimal? RefMax);
}
