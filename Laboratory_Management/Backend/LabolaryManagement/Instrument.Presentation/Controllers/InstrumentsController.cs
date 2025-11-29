using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Instrument.Application.Instruments.DTOs.Requests;
using Instrument.Application.Instruments.DTOs.Responses;
using Instrument.Application.Services;
using Instrument.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;


namespace Instrument.Presentation.Controllers;

[ApiController]
[Route("api/instruments")]
[Tags("Instruments Management")]
public class InstrumentsController : ControllerBase
{
    private readonly InstrumentService _service;
    private readonly ILogger<InstrumentsController> _logger;

    public InstrumentsController(InstrumentService service, ILogger<InstrumentsController> logger)
    {
        _service = service;
        _logger = logger;
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
    ///         "status": 0
    ///     }
    ///     
    /// Status values: 0=Online, 1=Offline, 2=Fault, 3=Maintenance
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = "perm:Instrument.Create")]
    public async Task<IActionResult> Create([FromForm] InstrumentCreateHttpRequest request)
    {
        string? imagePath = null;

        if (request.Image != null)
        {
            try
            {
                _logger.LogInformation("🔵 Starting image upload process...");
                _logger.LogInformation($"📊 Image name: {request.Image.FileName}");
                _logger.LogInformation($"📊 Image size: {request.Image.Length} bytes");
                _logger.LogInformation($"📊 Content type: {request.Image.ContentType}");

                // Lấy đường dẫn tuyệt đối
                var currentDir = Directory.GetCurrentDirectory();
                _logger.LogInformation($"📁 Current directory: {currentDir}");

                var folder = Path.Combine(currentDir, "Images");
                _logger.LogInformation($"📁 Target folder: {folder}");

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    _logger.LogInformation($"✅ Created directory: {folder}");
                }
                else
                {
                    _logger.LogInformation($"✅ Directory already exists: {folder}");
                }

                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
                var savePath = Path.Combine(folder, fileName);
                
                _logger.LogInformation($"💾 Saving to: {savePath}");

                // Lưu file
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                    await stream.FlushAsync();
                }

                // Verify file đã được tạo
                if (System.IO.File.Exists(savePath))
                {
                    var fileInfo = new FileInfo(savePath);
                    _logger.LogInformation($"✅ File saved successfully!");
                    _logger.LogInformation($"📊 File size on disk: {fileInfo.Length} bytes");
                    _logger.LogInformation($"📊 File created at: {fileInfo.CreationTime}");
                }
                else
                {
                    _logger.LogError($"❌ File NOT found after saving: {savePath}");
                    return BadRequest(new { error = "Failed to save image file" });
                }

                // Path lưu vào DB
                imagePath = $"Images/{fileName}";
                _logger.LogInformation($"💾 Image path for DB: {imagePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error saving image: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { error = $"Failed to save image: {ex.Message}" });
            }
        }
        else
        {
            _logger.LogInformation("ℹ️ No image provided");
        }

        try
        {
            // Map Presentation DTO -> Application DTO
            var appDto = new CreateInstrumentRequest(
                request.InstrumentCode,
                request.Name,
                (InstrumentStatus)request.Status,
                imagePath);
            
            var instrument = await _service.CreateAsync(appDto);

            _logger.LogInformation($"✅ Instrument created: {instrument.InstrumentCode}");
            _logger.LogInformation($"📷 Image path in DB: {instrument.ImagePath ?? "none"}");

            return CreatedAtAction(
                nameof(GetByCode),
                new { code = instrument.InstrumentCode },
                instrument
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError($"❌ Error creating instrument: {ex.Message}");
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
    public async Task<IActionResult> Update(string code, [FromForm] UpdateInstrumentHttpRequest request)
    {
        try
        {
            string? imagePath = null;

            // Nếu có upload file → lưu file
            if (request.Image != null)
            {
                var fileName = $"{Guid.NewGuid()}_{request.Image.FileName}";
                var savePath = Path.Combine("/images/instruments", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using var stream = new FileStream(savePath, FileMode.Create);
                await request.Image.CopyToAsync(stream);

                imagePath = $"/images/instruments/{fileName}";
            }
            var appRequest = new Instrument.Application.Instruments.DTOs.Requests.UpdateInstrumentRequest(
            request.Name,            
            request.Status,           
            request.ReagentStatus,    
            imagePath                 
);

            var instrument = await _service.UpdateAsync(code, appRequest);

            if (instrument == null)
                return NotFound(new { error = $"Instrument '{code}' not found." });

            return Ok(instrument);
        }
        catch (Exception ex)
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
