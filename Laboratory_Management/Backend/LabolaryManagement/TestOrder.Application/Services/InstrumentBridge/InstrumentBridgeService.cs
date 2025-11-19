using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http; // thêm để dùng IHttpClientFactory
using System.Net.Http.Json;
using TestOrder.Application.DTOs.InstrumentBridge;
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

        var items = booking.BookingTests
            .SelectMany(bt => bt.Catalog!.Parameters.Select(p =>
                new ForInstrumentItem(
                    bt.TestBookingNo,
                    bt.CatalogId ?? 0,
                    p.ParameterId,
                    p.ParameterName,
                    p.Unit,
                    p.ReferenceRange
                )))
            .OrderBy(i => i.TestBookingNo).ThenBy(i => i.ParameterId)
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

        // Đủ kết quả thì Completed (4)
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
            b.Status = (byte)4; // Completed
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
