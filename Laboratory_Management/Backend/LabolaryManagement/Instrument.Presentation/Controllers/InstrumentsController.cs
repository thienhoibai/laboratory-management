using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Instrument.Application.Instruments.DTOs.Requests;
using Instrument.Application.Instruments.DTOs.Responses;
using Instrument.Application.Services;
using Instrument.Domain.Enums;

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
    /// GET /api/instruments - Lấy danh sách máy xét nghiệm với phân trang, tìm kiếm và lọc
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số lượng mỗi trang (mặc định: 10)</param>
    /// <param name="search">Tìm kiếm theo InstrumentCode hoặc Name</param>
    /// <param name="status">Lọc theo InstrumentStatus (0=Online, 1=Offline, 2=Fault, 3=Maintenance)</param>
    /// <param name="runStatus">Lọc theo RunStatus (0=Running, 1=Completed, 2=Failed)</param>
    /// <param name="reagentStatus">Lọc theo ReagentStatus (0=OK, 1=Low, 2=Out)</param>
    [HttpGet]
    [Authorize(Policy = "perm:Instrument.List")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] InstrumentStatus? status = null,
        [FromQuery] RunStatus? runStatus = null,
        [FromQuery] ReagentStatus? reagentStatus = null)
    {
        var (items, total) = await _service.GetAllWithFilterAsync(
            page, pageSize, search, status, runStatus, reagentStatus);
        
        return Ok(new
        {
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)total / pageSize),
            items
        });
    }

    /// <summary>
    /// GET /api/instruments/{id:int} - Lấy thông tin chi tiết máy theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "perm:Instrument.View")]
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
    [Authorize(Policy = "perm:Instrument.View")]
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
    ///         "status": 0
    ///     }
    ///     
    /// Status values: 0=Online, 1=Offline, 2=Fault, 3=Maintenance
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = "perm:Instrument.Create")]
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
    ///         "status": 3,
    ///         "reagentStatus": 1
    ///     }
    ///     
    /// Status values: 0=Online, 1=Offline, 2=Fault, 3=Maintenance
    /// ReagentStatus values: 0=OK, 1=Low, 2=Out
    /// </remarks>
    [HttpPut("{code}")]
    [Authorize(Policy = "perm:Instrument.Update")]
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
    [Authorize(Policy = "perm:Instrument.Delete")]
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
    /// GET /api/instruments/{code}/status - Lấy trạng thái máy + run đang chạy
    /// </summary>
    [HttpGet("{code}/status")]
    public async Task<IActionResult> GetStatus(string code)
    {
        var status = await _service.GetStatusAsync(code);
        
        if (status == null)
            return NotFound(new { error = $"Instrument '{code}' not found." });

        return Ok(status);
    }
}
