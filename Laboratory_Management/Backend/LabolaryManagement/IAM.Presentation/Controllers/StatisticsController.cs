using System;
using System.Threading;
using System.Threading.Tasks;
using IAM.Application.Statistics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAM.Presentation.Controllers;

[ApiController]
[Route("api/statistics")]
[Tags("Thống kê User")]
public class StatisticsController : ControllerBase
{
    private readonly UserStatisticsService _statisticsService;

    public StatisticsController(UserStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Lấy thống kê người dùng
    /// </summary>
    [HttpGet("users")]
    [Authorize(Policy = "perm:Statistics.User.View")]
    public async Task<IActionResult> GetUserStatistics(CancellationToken ct)
    {
        try
        {
            var data = await _statisticsService.GetUserStatisticsAsync(ct);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy thống kê người dùng", details = ex.Message });
        }
    }
}
