using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestOrder.Application.Statistics.DTOs;
using TestOrder.Infrastructure.Data; // ✅ Add this

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

        // Calculate revenue (exclude cancelled - status = 3)
        var revenueToday = bookingsToday.Where(b => b.Status != 3).Sum(b => b.TotalPrice ?? 0);
        var revenueThisMonth = bookingsThisMonth.Where(b => b.Status != 3).Sum(b => b.TotalPrice ?? 0);
        var revenueThisYear = bookingsThisYear.Where(b => b.Status != 3).Sum(b => b.TotalPrice ?? 0);

        return new BookingStatisticsDto
        {
            TotalRevenueToday = (decimal)revenueToday,
            TotalRevenueThisMonth = (decimal)revenueThisMonth,
            TotalRevenueThisYear = (decimal)revenueThisYear,
            
            TotalBookingsToday = bookingsToday.Count,
            TotalBookingsThisMonth = bookingsThisMonth.Count,
            TotalBookingsThisYear = bookingsThisYear.Count,
            
            PendingBookings = allBookings.Count(b => b.Status == 0),
            CompletedBookings = allBookings.Count(b => b.Status ==  2),
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
}
