using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestOrder.Application.Statistics.DTOs;
using TestOrder.Infrastructure.Data;
using TestOrder.Infrastructure.Enums; 

namespace TestOrder.Application.Statistics.Services;

public class TestOrderStatisticsService
{
    private readonly TestOrderDBContext _db; // ✅ Fix: TestOrderDBContext not TestOrderDbContext

    public TestOrderStatisticsService(TestOrderDBContext db)
    {
        _db = db;
    }

    public async Task<BookingStatisticsDto> GetBookingStatisticsAsync(CancellationToken ct = default)
    {
        var today = DateTime.Now.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var firstDayOfYear = new DateTime(today.Year, 1, 1);

        var allBookings = await _db.Bookings
            .AsNoTracking()
            .ToListAsync(ct);

        // Filter by date
        var bookingsToday = allBookings.Where(b => b.CreateAt.HasValue && b.CreateAt.Value.Date == today).ToList();
        var bookingsThisMonth = allBookings.Where(b => b.CreateAt.HasValue && b.CreateAt.Value.Date >= firstDayOfMonth).ToList();
        var bookingsThisYear = allBookings.Where(b => b.CreateAt.HasValue && b.CreateAt.Value.Date >= firstDayOfYear).ToList();

        // Calculate revenue (exclude cancelled - status)
        var revenueToday = bookingsToday.Where(b => b.Status != (byte)BookingStatusEnum.Cancelled).Sum(b => b.TotalPrice ?? 0);
        var revenueThisMonth = bookingsThisMonth.Where(b => b.Status != (byte)BookingStatusEnum.Cancelled).Sum(b => b.TotalPrice ?? 0);
        var revenueThisYear = bookingsThisYear.Where(b => b.Status != (byte)BookingStatusEnum.Cancelled).Sum(b => b.TotalPrice ?? 0);

        return new BookingStatisticsDto
        {
            TotalRevenueToday = (decimal)revenueToday,
            TotalRevenueThisMonth = (decimal)revenueThisMonth,
            TotalRevenueThisYear = (decimal)revenueThisYear,

            TotalBookingsToday = bookingsToday.Count,
            TotalBookingsThisMonth = bookingsThisMonth.Count,
            TotalBookingsThisYear = bookingsThisYear.Count,

            PendingBookings = allBookings.Count(b => b.Status == 0),
            CompletedBookings = allBookings.Count(b => b.Status == 2),
            CancelledBookings = allBookings.Count(b => b.Status == 3),

            GeneratedAt = DateTime.Now
        };
    }



    public async Task<CatalogStatisticsDto> GetCatalogStatisticsAsync(CancellationToken ct = default)
    {
        var catalogs = await _db.TestCatalogs.AsNoTracking().ToListAsync(ct);
        var bundles = await _db.TestBundles.AsNoTracking().ToListAsync(ct);

        return new CatalogStatisticsDto
        {
            TotalCatalogs = catalogs.Count,
            ActiveCatalogs = catalogs.Count(c => c.Price > 0),
            InactiveCatalogs = catalogs.Count(c => c.Price <= 0),

            TotalBundles = bundles.Count,
            ActiveBundles = bundles.Count(b => b.IsActive == true), // ✅ Fix nullable bool
            InactiveBundles = bundles.Count(b => b.IsActive != true), // ✅ Fix nullable bool

            GeneratedAt = DateTime.Now
        };
    }

    public async Task<MonthlyRevenue> GetRecentSixMonthRevenueAsync(CancellationToken ct = default)
    {
        var sixMonthsAgo = DateTime.Now.AddMonths(-5);
        var monthlyRevenues = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.CreateAt.HasValue && b.CreateAt.Value >= new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1))
            .GroupBy(b => new { b.CreateAt.Value.Year, b.CreateAt.Value.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = (MonthNameEnum)g.Key.Month,
                Revenue = g.Where(b => b.Status != (byte)BookingStatusEnum.Cancelled).Sum(b => b.TotalPrice ?? 0)
            })
            .OrderBy(r => r.Year).ThenBy(r => r.Month)
            .ToListAsync(ct);
        return new MonthlyRevenue
        {
            GeneratedAt = DateTime.Now,
            Revenues = monthlyRevenues.ToArray()
        };
    }
    public async Task<MonthlyBookingCount> GetRecentSixMonthBookingCountAsync(CancellationToken ct = default)
    {
        var sixMonthsAgo = DateTime.Now.AddMonths(-5);
        var monthlyBookingCounts = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.CreateAt.HasValue && b.CreateAt.Value >= new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1))
            .GroupBy(b => new { b.CreateAt.Value.Year, b.CreateAt.Value.Month })
            .Select(g => new MonthlyBookingCountDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = (MonthNameEnum)g.Key.Month,
                BookingCount = g.Count()
            })
            .OrderBy(r => r.Year).ThenBy(r => r.Month)
            .ToListAsync(ct);
        return new MonthlyBookingCount
        {
            GeneratedAt = DateTime.Now,
            BookingCounts = monthlyBookingCounts.ToArray()
        };
    }
}


#region Dtos

public class BookingMonthlyDto
{
    public int Month { get; init; }
    public int Year { get; init; }
    public MonthNameEnum MonthName { get; init; }
}

public class MonthlyRevenueDto : BookingMonthlyDto
{
    public double Revenue { get; init; }
}

public class MonthlyBookingCountDto : BookingMonthlyDto
{
    public int BookingCount { get; init; }
}


public class MonthlyRevenue
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public MonthlyRevenueDto[]? Revenues { get; set; }
}
public class MonthlyBookingCount
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public MonthlyBookingCountDto[]? BookingCounts { get; set; }
}



    public enum MonthNameEnum
{
    T1 = 1,
    T2 = 2,
    T3 = 3,
    T4 = 4,
    T5 = 5,
    T6 = 6,
    T7 = 7,
    T8 = 8,
    T9 = 9,
    T10 = 10,
    T11 = 11,
    T12 = 12
}
#endregion
