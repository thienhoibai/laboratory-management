using Microsoft.AspNetCore.Mvc;
using Instrument.Application.DTOs;
using Instrument.Application.Services;

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

    /// <summary>
    /// POST /api/instrument/runs/start - Bắt đầu một run xét nghiệm mới (Simplified)
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/instrument/runs/start
    ///     {
    ///         "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "instrumentCode": "INSTR001"
    ///     }
    ///     
    /// Flow:
    /// 1. Kiểm tra booking status = 4 (ReadyForInstrument)
    /// 2. Lấy danh sách catalog/parameter từ TestOrder
    /// 3. Tạo run mới với status RUNNING (không kiểm tra cartridge)
    /// </remarks>
    [HttpPost("start")]
    public async Task<IActionResult> StartRun([FromBody] StartRunRequest request)
    {
        try
        {
            var result = await _service.StartRunAsync(request);
            
            if (result.Status == "FAILED")
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

    /// <summary>
    /// POST /api/instrument/runs/{runId}/drop-csv - Sinh CSV kết quả và gửi về TestOrder
    /// </summary>
    /// <remarks>
    /// Endpoint này sẽ:
    /// 1. Lấy danh sách parameter từ TestOrder bridge
    /// 2. Sinh giá trị kết quả deterministic (dựa trên BookingId + ParameterId)
    /// 3. Gửi kết quả về TestOrder qua POST /api/bridge/bookings/{id}/results
    /// </remarks>
    [HttpPost("{runId:int}/drop-csv")]
    public async Task<IActionResult> DropCsv(int runId)
    {
        try
        {
            var request = new DropCsvRequest(runId);
            var result = await _service.DropCsvAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { error = "TestOrder service unavailable", details = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/instrument/runs/{runId}/complete - Hoàn thành run (Simplified)
    /// </summary>
    /// <remarks>
    /// Endpoint này sẽ:
    /// 1. Cập nhật run status = COMPLETED (không trừ cartridge)
    /// </remarks>
    [HttpPost("{runId:int}/complete")]
    public async Task<IActionResult> CompleteRun(int runId)
    {
        try
        {
            var request = new CompleteRunRequest(runId);
            var result = await _service.CompleteRunAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { error = "TestOrder service unavailable", details = ex.Message });
        }
    }
}
