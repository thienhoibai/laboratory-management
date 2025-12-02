using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestOrder.Application.Statistics.Services;

namespace TestOrder.Presentation.Controllers;

[ApiController]
[Route("api/statistics")]
[Tags("Thống kê Booking & Catalog")]
public class StatisticsController : ControllerBase
{
    private readonly TestOrderStatisticsService _statisticsService;

    public StatisticsController(TestOrderStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Lấy thống kê booking và doanh thu
    /// </summary>
    [HttpGet("bookings")]
    [Authorize(Policy = "perm:Statistics.Booking.View")]
    public async Task<IActionResult> GetBookingStatistics(CancellationToken ct)
    {
        try
        {
            var data = await _statisticsService.GetBookingStatisticsAsync(ct);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy thống kê booking", details = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thống kê catalog và bundle
    /// </summary>
    [HttpGet("catalogs")]
    [Authorize(Policy = "perm:Statistics.Catalog.View")]
    public async Task<IActionResult> GetCatalogStatistics(CancellationToken ct)
    {
        try
        {
            var data = await _statisticsService.GetCatalogStatisticsAsync(ct);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy thống kê catalog", details = ex.Message });
        }
    }

    [HttpGet("monthly-revenue")]
    [Authorize(Policy = "perm:Statistics.Booking.View")]

    public async Task<IActionResult> GetMonthlyRevenue(CancellationToken ct)
    {
        try
        {
            var data = await _statisticsService.GetRecentSixMonthRevenueAsync(ct);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy thống kê doanh thu hàng tháng", details = ex.Message });
        }
    }

    [HttpGet("monthly-count")]
    [Authorize(Policy = "perm:Statistics.Booking.View")] 

    public async Task<IActionResult> GetMonthlyBookingCount(CancellationToken ct)
    {
        try
        {
            var data = await _statisticsService.GetRecentSixMonthBookingCountAsync(ct);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy thống kê số lượng booking hàng tháng", details = ex.Message });
        }
    }
}
