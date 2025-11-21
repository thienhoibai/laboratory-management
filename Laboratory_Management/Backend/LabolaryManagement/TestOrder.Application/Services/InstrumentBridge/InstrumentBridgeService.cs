using Microsoft.EntityFrameworkCore;
using TestOrder.Application.DTOs.InstrumentBridge;
using TestOrder.Infrastructure.Data;
using TestOrder.Infrastructure.Models;
using BookingEntity = TestOrder.Infrastructure.Models.Booking;

namespace TestOrder.Application.Services.InstrumentBridge;

public class InstrumentBridgeService
{
    private readonly TestOrderDBContext _db;

    public InstrumentBridgeService(TestOrderDBContext db) => _db = db;

    /// <summary>
    /// GET /for-instrument: Trả danh sách test con & tham số để máy sinh kết quả
    /// </summary>
    public async Task<ForInstrumentResponse?> GetForInstrumentAsync(Guid bookingId)
    {
        // Lấy booking và kiểm tra Status == 4
        var booking = await _db.Set<BookingEntity>().AsNoTracking()
            .Where(b => b.BookingId == bookingId)
            .Select(b => new { b.BookingId, b.Status, b.PatientName, b.BundleId })
            .FirstOrDefaultAsync();

        if (booking == null || booking.Status != 4) 
            return null;

        // 🔍 DEBUG: Kiểm tra số lượng BookingTest
        var bookingTestCount = await _db.Set<BookingTest>()
            .Where(bt => bt.BookingId == bookingId)
            .CountAsync();

        Console.WriteLine($"[DEBUG] BookingId: {bookingId}, BundleId: {booking.BundleId}, BookingTest count: {bookingTestCount}");

        // ✅ FIX: Sử dụng SelectMany với navigation property
        var items = await _db.Set<BookingTest>()
            .AsNoTracking()
            .Where(bt => bt.BookingId == bookingId)
            .Include(bt => bt.Catalog)
                .ThenInclude(c => c.Parameters)
            .SelectMany(bt => bt.Catalog.Parameters.Select(param => new ForInstrumentItemDto(
                bt.TestBookingNo,
                bt.Catalog.CatalogId,
                param.ParameterId,
                param.ParameterName,
                param.Unit,
                param.MinRange.HasValue ? (decimal)param.MinRange.Value : null,
                param.MaxRange.HasValue ? (decimal)param.MaxRange.Value : null
            )))
            .ToListAsync();

        Console.WriteLine($"[DEBUG] Items returned: {items.Count}");

        // Tìm duplicate groups (cùng ParameterId xuất hiện ở nhiều TestBookingNo)
        var duplicateGroups = items
            .GroupBy(i => i.ParameterId)
            .Where(g => g.Select(x => x.TestBookingNo).Distinct().Count() > 1)
            .Select(g => new DuplicateGroup(
                g.Key,
                g.Select(x => x.TestBookingNo).Distinct().OrderBy(x => x).ToList()
            ))
            .ToList();

        return new ForInstrumentResponse(
            booking.BookingId,
            booking.Status ?? 0,
            booking.PatientName,
            items,
            duplicateGroups
        );
    }

    /// <summary>
    /// POST /results: Instrument gửi kết quả JSON, validate + upsert + báo completed
    /// </summary>
    public async Task<IngestResponse?> IngestResultsAsync(Guid bookingId, IngestRequest req)
    {
        // Kiểm tra booking tồn tại và Status == 4
        var booking = await _db.Set<BookingEntity>().FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking == null || booking.Status != 4) 
            return null;

        // ✅ FIX: Sử dụng SelectMany với navigation property
        var expectedPairs = await _db.Set<BookingTest>()
            .AsNoTracking()
            .Where(bt => bt.BookingId == bookingId)
            .Include(bt => bt.Catalog)
                .ThenInclude(c => c.Parameters)
            .SelectMany(bt => bt.Catalog.Parameters.Select(param => new { bt.TestBookingNo, param.ParameterId }))
            .ToListAsync();

        var expectedSet = expectedPairs.Select(x => (x.TestBookingNo, x.ParameterId)).ToHashSet();

        int accepted = 0, rejected = 0;

        // Upsert từng item
        foreach (var item in req.Items)
        {
            var key = (item.TestBookingNo, item.ParameterId);
            
            // Validate: chỉ chấp nhận nếu thuộc expectedSet
            if (!expectedSet.Contains(key))
            {
                rejected++;
                continue;
            }

            // Upsert theo (TestBookingNo, ParameterId)
            var existing = await _db.Set<TestResult>()
                .FirstOrDefaultAsync(r => r.TestBookingNo == item.TestBookingNo 
                                       && r.ParameterId == item.ParameterId);

            var valueStr = item.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (existing == null)
            {
                _db.Set<TestResult>().Add(new TestResult
                {
                    TestBookingNo = item.TestBookingNo,
                    ParameterId = item.ParameterId,
                    ResultValue = valueStr
                });
            }
            else
            {
                existing.ResultValue = valueStr;
                _db.Update(existing);
            }

            accepted++;
        }

        await _db.SaveChangesAsync();

        // FIX: Kiểm tra đủ kết quả chưa - Load by TestBookingNo list để tránh EF Core translation error
        var allTestBookingNos = expectedSet.Select(x => x.TestBookingNo).Distinct().ToList();
        
        var existingPairs = await _db.Set<TestResult>()
            .Where(r => r.TestBookingNo.HasValue && allTestBookingNos.Contains(r.TestBookingNo.Value))
            .Select(r => new { r.TestBookingNo, r.ParameterId })
            .ToListAsync();

        var existingSet = existingPairs
            .Where(x => x.TestBookingNo.HasValue && x.ParameterId.HasValue)
            .Select(x => (x.TestBookingNo!.Value, x.ParameterId!.Value))
            .ToHashSet();

        var missing = expectedSet.Except(existingSet)
            .Select(x => new MissingPair(x.Item1, x.Item2))
            .ToList();

        bool completed = missing.Count == 0;

        // Tuỳ chọn: auto set Completed (comment out nếu không muốn)
        // if (completed) 
        // { 
        //     booking.Status = 6; // Completed
        //     await _db.SaveChangesAsync(); 
        // }

        return new IngestResponse(accepted, rejected, completed, missing);
    }

    /// <summary>
    /// GET /expected: Trả tổng số cặp cần có để booking xem đã đủ chưa
    /// </summary>
    public async Task<ExpectedResponse?> GetExpectedAsync(Guid bookingId)
    {
        var info = await GetForInstrumentAsync(bookingId);
        if (info == null) 
            return null;

        var expectedPairs = info.Items.Count;

        // FIX: Đếm existing - Load by TestBookingNo list để tránh EF Core translation error
        var expectedSet = info.Items.Select(x => (x.TestBookingNo, x.ParameterId)).ToHashSet();
        var allTestBookingNos = expectedSet.Select(x => x.TestBookingNo).Distinct().ToList();
        
        var existingPairs = await _db.Set<TestResult>()
            .Where(r => r.TestBookingNo.HasValue && allTestBookingNos.Contains(r.TestBookingNo.Value))
            .Select(r => new { r.TestBookingNo, r.ParameterId })
            .ToListAsync();

        var existingSet = existingPairs
            .Where(x => x.TestBookingNo.HasValue && x.ParameterId.HasValue)
            .Select(x => (x.TestBookingNo!.Value, x.ParameterId!.Value))
            .ToHashSet();

        var missing = expectedSet.Except(existingSet)
            .Select(x => new MissingPair(x.Item1, x.Item2))
            .ToList();

        return new ExpectedResponse(
            bookingId,
            expectedPairs,
            existingSet.Count,
            missing.Count == 0,
            missing
        );
    }
}
