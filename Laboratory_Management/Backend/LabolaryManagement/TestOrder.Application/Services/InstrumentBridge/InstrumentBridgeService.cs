using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http; // thêm để dùng IHttpClientFactory
using System.Net.Http.Json;
using TestOrder.Application.DTOs.InstrumentBridge;
using TestOrder.Application.DTOs.Bookings;
using TestOrder.Infrastructure.Data;
using TestOrder.Infrastructure.Models;     // Booking/BookingTest/TestCatalog/TestResult

namespace TestOrder.Application.InstrumentBridge;

public class InstrumentBridgeService
{
    private readonly TestOrderDBContext _ctx;          // Đổi tên DbContext cho khớp project của bạn
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _cfg;

    public InstrumentBridgeService(TestOrderDBContext ctx, IHttpClientFactory httpFactory, IConfiguration cfg)
    {
        _ctx = ctx; _httpFactory = httpFactory; _cfg = cfg;
    }

    public async Task<ForInstrumentResponse> GetForInstrumentAsync(Guid bookingId)
    {
        var booking = await _ctx.Set<Booking>()
            .AsNoTracking()
            .Include(b => b.BookingTests)
                .ThenInclude(bt => bt.Catalog)
                    .ThenInclude(c => c.Parameters)
            .SingleOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking is null) throw new InvalidOperationException("Booking not found");

        // Lấy giới tính qua PatientService (nếu cấu hình); nếu không → null
        string? patientSex = null;
        if (booking.PatientId.HasValue)
        {
            var baseUrl = _cfg["PatientServiceBaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                try
                {
                    var client = _httpFactory.CreateClient("patient");
                    var dto = await client.GetFromJsonAsync<PatientDto>($"/api/patients/{booking.PatientId.Value}");
                    patientSex = NormalizeSex(dto?.Sex); // "M"/"F"/null
                }
                catch { /* ignore & fallback */ }
            }
        }

        // Group theo ParameterId, giữ danh sách TestBookingNo và CatalogId
        var dict = new Dictionary<int, (string name, string? unit, decimal? min, decimal? max, HashSet<long> nos, HashSet<int> catalogs)>();

        foreach (var bt in booking.BookingTests)
        {
            if (bt.Catalog?.Parameters == null) continue;
            foreach (var p in bt.Catalog.Parameters)
            {
                if (!dict.TryGetValue(p.ParameterId, out var entry))
                {
                    decimal? min = p.MinRange.HasValue ? (decimal?)Convert.ToDecimal(p.MinRange.Value) : null;
                    decimal? max = p.MaxRange.HasValue ? (decimal?)Convert.ToDecimal(p.MaxRange.Value) : null;
                    entry = (p.ParameterName, p.Unit, min, max, new HashSet<long>(), new HashSet<int>());
                }
                entry.nos.Add(bt.TestBookingNo);
                if (bt.CatalogId.HasValue) entry.catalogs.Add(bt.CatalogId.Value);
                // Nếu chưa có min/max mà có MinRange/MaxRange thì gán
                if (!entry.min.HasValue && p.MinRange.HasValue)
                    entry.min = (decimal)Convert.ToDecimal(p.MinRange.Value);
                if (!entry.max.HasValue && p.MaxRange.HasValue)
                    entry.max = (decimal)Convert.ToDecimal(p.MaxRange.Value);
                dict[p.ParameterId] = entry;
            }
        }

        var items = dict
            .OrderBy(kv => kv.Key)
            .Select(kv => new ForInstrumentItem(
                kv.Key,
                kv.Value.name,
                kv.Value.unit,
                kv.Value.min,
                kv.Value.max,
                kv.Value.nos.OrderBy(x => x).ToList(),
                kv.Value.catalogs.OrderBy(x => x).ToList()
            ))
            .ToList();

        return new ForInstrumentResponse(booking.BookingId, booking.PatientName, patientSex, items);
    }

    public async Task SaveResultsFromInstrumentAsync(PostResultsRequest req)
    {
        // Validate TestBookingNo thuộc booking
        var validNos = await _ctx.Set<BookingTest>()
            .Where(bt => bt.BookingId == req.BookingId)
            .Select(bt => bt.TestBookingNo)
            .ToListAsync();

        var invalidNos = req.Results.Select(r => r.TestBookingNo).Distinct().Except(validNos).ToArray();
        if (invalidNos.Length > 0)
            throw new InvalidOperationException($"Invalid TestBookingNo(s): {string.Join(",", invalidNos)}");

        // Upsert theo (TestBookingNo, ParameterId)
        foreach (var r in req.Results)
        {
            var row = await _ctx.Set<TestResult>()
                .SingleOrDefaultAsync(x => x.TestBookingNo == r.TestBookingNo && x.ParameterId == r.ParameterId);

            var valStr = r.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (row is null)
            {
                await _ctx.Set<TestResult>().AddAsync(new TestResult
                {
                    TestBookingNo = r.TestBookingNo,
                    ParameterId = r.ParameterId,
                    ResultValue = valStr
                });
            }
            else
            {
                row.ResultValue = valStr;
            }
        }
        await _ctx.SaveChangesAsync();

        // Đủ kết quả thì Completed
        var expected = await _ctx.Set<BookingTest>()
            .Where(bt => bt.BookingId == req.BookingId)
            .Join(_ctx.Set<TestCatalog>(), bt => bt.CatalogId, c => c.CatalogId, (bt, c) => new { bt, c })
            .SelectMany(x => x.c.Parameters.Select(p => new { x.bt.TestBookingNo, p.ParameterId }))
            .CountAsync();

        var actual = await _ctx.Set<TestResult>()
            .Join(_ctx.Set<BookingTest>(), r => r.TestBookingNo, bt => bt.TestBookingNo, (r, bt) => new { r, bt })
            .Where(x => x.bt.BookingId == req.BookingId)
            .Select(x => new { x.r.TestBookingNo, x.r.ParameterId })
            .Distinct()
            .CountAsync();

        if (expected > 0 && actual >= expected)
        {
            var b = await _ctx.Set<Booking>().SingleAsync(x => x.BookingId == req.BookingId);
            b.Status = (byte)BookingStatusEnum.Completed; // 6
            await _ctx.SaveChangesAsync();
        }
    }

    private static string? NormalizeSex(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var t = s.Trim().ToLowerInvariant();
        if (t.StartsWith("m") || t.StartsWith("na")) return "M";
        if (t.StartsWith("f") || t.StartsWith("nữ") || t.StartsWith("nu")) return "F";
        return null;
    }

    private sealed class PatientDto { public string? Sex { get; set; } }
}
