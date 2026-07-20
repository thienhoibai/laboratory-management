using System;
using System.Threading;
using System.Threading.Tasks;
using BlogService.Application.Statistics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogService.Presentation.Controllers;

[ApiController]
[Route("api/statistics")]
[Tags("Statistics")]
public class StatisticsController : ControllerBase
{
    private readonly BlogStatisticsService _statisticsService;

    public StatisticsController(BlogStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Lấy thống kê blog
    /// </summary>
    [HttpGet("blogs")]
    [Authorize(Policy = "perm:Statistics.Blog.View")]
    public async Task<IActionResult> GetBlogStatistics(CancellationToken ct)
    {
        try
        {
            var data = await _statisticsService.GetBlogStatisticsAsync(ct);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy thống kê blog", details = ex.Message });
        }
    }
}
