using System;
using System.Threading;
using System.Threading.Tasks;
using Instrument.Application.Statistics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Instrument.Presentation.Controllers;

[ApiController]
[Route("api/statistics")]
[Tags("Thống kê Instrument")]
public class StatisticsController : ControllerBase
{
    private readonly InstrumentStatisticsService _statisticsService;

    public StatisticsController(InstrumentStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Lấy thống kê máy xét nghiệm
    /// </summary>
    [HttpGet("instruments")]
    [Authorize(Policy = "perm:Statistics.Instrument.View")]
    public async Task<IActionResult> GetInstrumentStatistics(CancellationToken ct)
    {
        try
        {
            var data = await _statisticsService.GetInstrumentStatisticsAsync(ct);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy thống kê máy xét nghiệm", details = ex.Message });
        }
    }
    [HttpGet("ReagentStatus")]
    
    public async Task<IActionResult> GetReagentStatusStatistics(CancellationToken ct)
    {
        try
        {
            var data = await _statisticsService.GetReagentStatusStatisticsAsync(ct);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy thống kê trạng thái hóa chất", details = ex.Message });
        }
    }
}
