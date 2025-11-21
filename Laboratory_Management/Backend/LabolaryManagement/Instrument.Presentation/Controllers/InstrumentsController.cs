using Microsoft.AspNetCore.Mvc;
using Instrument.Application.DTOs;
using Instrument.Application.Services;

namespace Instrument.Presentation.Controllers;

[ApiController]
[Route("api/instruments")]
[Tags("Instruments Management")]
public class InstrumentsController : ControllerBase
{
    private readonly InstrumentService _service;

    public InstrumentsController(InstrumentService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/instruments - Lấy danh sách tất cả máy xét nghiệm
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var instruments = await _service.GetAllAsync();
        return Ok(instruments);
    }

    /// <summary>
    /// GET /api/instruments/{id:int} - Lấy thông tin chi tiết máy theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var instrument = await _service.GetByIdAsync(id);
        
        if (instrument == null)
            return NotFound(new { error = $"Instrument with ID {id} not found." });

        return Ok(instrument);
    }

    /// <summary>
    /// GET /api/instruments/{code} - Lấy thông tin chi tiết máy theo code
    /// </summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var instrument = await _service.GetByCodeAsync(code);
        
        if (instrument == null)
            return NotFound(new { error = $"Instrument '{code}' not found." });

        return Ok(instrument);
    }

    /// <summary>
    /// POST /api/instruments - Tạo máy xét nghiệm mới
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/instruments
    ///     {
    ///         "instrumentCode": "INSTR001",
    ///         "name": "Máy xét nghiệm sinh hóa",
    ///         "status": "ONLINE"
    ///     }
    ///     
    /// Status values: ONLINE, OFFLINE, FAULT, MAINTENANCE
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInstrumentRequest request)
    {
        try
        {
            var instrument = await _service.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetByCode), 
                new { code = instrument.InstrumentCode }, 
                instrument);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/instruments/{code} - Cập nhật thông tin máy
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/instruments/INSTR001
    ///     {
    ///         "name": "Máy xét nghiệm tự động",
    ///         "status": "MAINTENANCE",
    ///         "reagentStatus": "LOW"
    ///     }
    ///     
    /// Status values: ONLINE, OFFLINE, FAULT, MAINTENANCE
    /// ReagentStatus values: OK, LOW, OUT
    /// </remarks>
    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateInstrumentRequest request)
    {
        try
        {
            var instrument = await _service.UpdateAsync(code, request);
            
            if (instrument == null)
                return NotFound(new { error = $"Instrument '{code}' not found." });

            return Ok(instrument);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE /api/instruments/{code} - Xóa máy xét nghiệm
    /// </summary>
    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        try
        {
            var deleted = await _service.DeleteAsync(code);
            
            if (!deleted)
                return NotFound(new { error = $"Instrument '{code}' not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/instruments/{code}/heartbeat - Cập nhật heartbeat của máy
    /// </summary>
    /// <remarks>
    /// Máy xét nghiệm gọi endpoint này định kỳ để báo hiệu đang hoạt động.
    /// Tự động chuyển status về ONLINE nếu đang OFFLINE.
    /// </remarks>
    [HttpPost("{code}/heartbeat")]
    public async Task<IActionResult> Heartbeat(string code)
    {
        var updated = await _service.UpdateHeartbeatAsync(code);
        
        if (!updated)
            return NotFound(new { error = $"Instrument '{code}' not found." });

        return Ok(new { message = "Heartbeat updated", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// GET /api/instruments/{code}/status - Lấy trạng thái đầy đủ của máy
    /// </summary>
    /// <remarks>
    /// Trả về thông tin máy + run đang chạy (nếu có) + cartridge status
    /// </remarks>
    [HttpGet("{code}/status")]
    public async Task<IActionResult> GetStatus(string code)
    {
        var status = await _service.GetStatusAsync(code);
        
        if (status == null)
            return NotFound(new { error = $"Instrument '{code}' not found." });

        return Ok(status);
    }
}
