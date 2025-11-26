using Microsoft.AspNetCore.Mvc;
using Instrument.Application.Runs.DTOs.Requests;
using Instrument.Application.Services;
using Instrument.Domain.Enums;

namespace Instrument.Presentation.Controllers;

[ApiController]
[Route("api/instrument/runs")]
[Tags("Run Management")]
public class RunsController : ControllerBase
{
    private readonly RunService _service;

    public RunsController(RunService service)
    {
        _service = service;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartRun([FromBody] StartRunRequest request)
    {
        try
        {
            var result = await _service.StartAndCompleteRunAsync(request);
            
            if (result.Status == RunStatus.Failed)
                return BadRequest(new { error = result.Message });

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { error = "TestOrder service unavailable", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/instrument/runs/{runId} - Lấy thông tin chi tiết run
    /// </summary>
    [HttpGet("{runId:int}")]
    public async Task<IActionResult> GetRunDetail(int runId)
    {
        var run = await _service.GetRunDetailAsync(runId);
        
        if (run == null)
            return NotFound(new { error = $"Run with ID {runId} not found." });

        return Ok(run);
    }
}
