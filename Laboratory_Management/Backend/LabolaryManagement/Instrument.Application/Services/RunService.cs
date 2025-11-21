using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using Instrument.Application.DTOs;
using Instrument.Domain.Entities;
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
    /// Tạo run đơn giản (không kiểm tra warehouse)
    /// </summary>
    public async Task<StartRunResponse> StartRunAsync(StartRunRequest req)
    {
        // 1. Gọi TestOrder bridge lấy danh sách Catalog/Parameter
        var testOrderClient = _httpFactory.CreateClient("testorder");
        var bridgeRes = await testOrderClient.GetFromJsonAsync<BridgeResponse>(
            $"/api/bridge/bookings/{req.BookingId}/for-instrument");

        if (bridgeRes == null || bridgeRes.Status != 4)
            return new StartRunResponse(0, "FAILED", "Booking not ready (Status must be 4)");

        // 2. Tạo Run
        var run = new InstrumentRun
        {
            BookingId = req.BookingId,
            InstrumentCode = req.InstrumentCode,
            Status = "RUNNING"
        };

        _db.InstrumentRuns.Add(run);
        await _db.SaveChangesAsync();

        return new StartRunResponse(run.RunId, "RUNNING", "Run started successfully");
    }

    /// <summary>
    /// POST /api/instrument/runs/{runId}/drop-csv
    /// Sinh CSV kết quả (random deterministic) và gửi về TestOrder
    /// </summary>
    public async Task<DropCsvResponse> DropCsvAsync(DropCsvRequest req)
    {
        var run = await _db.InstrumentRuns.FindAsync(req.RunId);
        if (run == null)
            throw new InvalidOperationException("Run not found");

        // Lấy danh sách parameter từ TestOrder
        var testOrderClient = _httpFactory.CreateClient("testorder");
        var bridgeRes = await testOrderClient.GetFromJsonAsync<BridgeResponse>(
            $"/api/bridge/bookings/{run.BookingId}/for-instrument");

        if (bridgeRes == null)
            throw new InvalidOperationException("Booking not found");

        // Sinh kết quả deterministic
        var results = new List<IngestItemDto>();
        foreach (var item in bridgeRes.Items)
        {
            var value = GenerateDeterministicValue(run.BookingId, item.ParameterId, item.RefMin, item.RefMax);
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

        // Gửi kết quả về TestOrder (POST /api/bridge/.../results)
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

        // (Optional) Lưu path file CSV vào log
        var csvPath = $"results/{run.BookingId}_{run.RunId}.csv";

        return new DropCsvResponse(csvPath, $"Sent {results.Count} results to TestOrder");
    }

    /// <summary>
    /// POST /api/instrument/runs/{runId}/complete
    /// Hoàn thành run (simplified - no cartridge consumption)
    /// </summary>
    public async Task<CompleteRunResponse> CompleteRunAsync(CompleteRunRequest req)
    {
        var run = await _db.InstrumentRuns
            .FirstOrDefaultAsync(r => r.RunId == req.RunId);

        if (run == null)
            throw new InvalidOperationException("Run not found");

        // Cập nhật run status (không trừ on-board)
        run.Status = "COMPLETED";
        run.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new CompleteRunResponse("COMPLETED", "Run completed successfully", new List<RunUsageDto>());
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

        // Không có usages trong minimal version
        return new RunDetailDto(
            run.RunId,
            run.BookingId,
            run.InstrumentCode,
            run.Status,
            run.StartedAt,
            run.CompletedAt,
            new List<RunUsageDto>() // Empty list
        );
    }

    // Helper: Sinh giá trị deterministic
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

            var value = min + (decimal)rng.NextDouble() * (max - min);
            return Math.Round(value, 2);
        }
    }

    // DTOs cho bridge response
    private record BridgeResponse(Guid BookingId, byte Status, List<BridgeItem> Items);
    private record BridgeItem(long TestBookingNo, int CatalogId, int ParameterId, string ParameterName, string? Unit, decimal? RefMin, decimal? RefMax);
    private record IngestItemDto(long TestBookingNo, int ParameterId, string ParameterName, decimal Value, string? Unit, decimal? RefMin, decimal? RefMax);
}
