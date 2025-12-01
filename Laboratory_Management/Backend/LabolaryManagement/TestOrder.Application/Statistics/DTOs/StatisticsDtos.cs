using System;

namespace TestOrder.Application.Statistics.DTOs;

public record BookingStatisticsDto
{
    // Doanh thu
    public decimal TotalRevenueToday { get; init; }
    public decimal TotalRevenueThisMonth { get; init; }
    public decimal TotalRevenueThisYear { get; init; }
    
    // Số lượng booking
    public int TotalBookingsToday { get; init; }
    public int TotalBookingsThisMonth { get; init; }
    public int TotalBookingsThisYear { get; init; }
    
    // Trạng thái
    public int PendingBookings { get; init; }
    public int CompletedBookings { get; init; }
    public int CancelledBookings { get; init; }
    
    public DateTime GeneratedAt { get; init; } = DateTime.Now;
}

public record CatalogStatisticsDto
{
    public int TotalCatalogs { get; init; }
    public int ActiveCatalogs { get; init; }
    public int InactiveCatalogs { get; init; }
    
    public int TotalBundles { get; init; }
    public int ActiveBundles { get; init; }
    public int InactiveBundles { get; init; }
    
    public DateTime GeneratedAt { get; init; } = DateTime.Now;
}
