using Microsoft.EntityFrameworkCore;
using Instrument.Application.Instruments.DTOs.Requests;
using Instrument.Application.Instruments.DTOs.Responses;
using Instrument.Application.Runs.DTOs.Responses;
using Instrument.Infrastructure;
using Instrument.Domain.Enums;
using DomainInstrument = Instrument.Domain.Entities.Instrument;

namespace Instrument.Application.Services;

public class InstrumentService
{
    private readonly InstrumentDbContext _db;

    public InstrumentService(InstrumentDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// GET /api/instruments - Lấy danh sách tất cả máy
    /// </summary>
    public async Task<List<InstrumentListItem>> GetAllAsync()
    {
        var instruments = await _db.Instruments
            .AsNoTracking()
            .OrderBy(i => i.InstrumentCode)
            .ToListAsync();

        return instruments.Select(i => new InstrumentListItem(
            i.InstrumentId,
            i.InstrumentCode,
            i.Name,
            i.Status,
            i.ReagentStatus,
            i.ImageUrl
        )).ToList();
    }

    /// <summary>
    /// GET /api/instruments (with pagination & filters) - Lấy danh sách máy với phân trang, tìm kiếm và lọc
    /// </summary>
    public async Task<(List<InstrumentListItem> Items, long Total)> GetAllWithFilterAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        InstrumentStatus? status = null,
        RunStatus? runStatus = null,
        ReagentStatus? reagentStatus = null)
    {
        var query = _db.Instruments.AsNoTracking();

        // Tìm kiếm theo code hoặc name
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(i =>
                i.InstrumentCode.ToLower().Contains(searchLower) ||
                i.Name.ToLower().Contains(searchLower));
        }

        // Lọc theo status
        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        // Lọc theo reagentStatus
        if (reagentStatus.HasValue)
        {
            query = query.Where(i => i.ReagentStatus == reagentStatus.Value);
        }

        // Lọc theo runStatus (cần join với InstrumentRuns)
        if (runStatus.HasValue)
        {
            query = query.Where(i => _db.InstrumentRuns
                .Any(r => r.InstrumentCode == i.InstrumentCode && r.Status == runStatus.Value));
        }

        // Đếm tổng số
        var total = await query.CountAsync();

        // Phân trang
        var instruments = await query
            .OrderBy(i => i.InstrumentCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = instruments.Select(i => new InstrumentListItem(
            i.InstrumentId,
            i.InstrumentCode,
            i.Name,
            i.Status,
            i.ReagentStatus,
            i.ImageUrl
        )).ToList();

        return (items, total);
    }

    /// <summary>
    /// GET /api/instruments/{code} - Lấy thông tin chi tiết máy theo code
    /// </summary>
    public async Task<InstrumentResponse?> GetByCodeAsync(string instrumentCode)
    {
        var instrument = await _db.Instruments
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InstrumentCode == instrumentCode);

        if (instrument == null)
            return null;

        return new InstrumentResponse(
            instrument.InstrumentId,
            instrument.InstrumentCode,
            instrument.Name,
            instrument.Status,
            instrument.ReagentStatus,
            instrument.ImageUrl,
            instrument.CreatedAt
        );
    }

    /// <summary>
    /// GET /api/instruments/{id:int} - Lấy thông tin chi tiết máy theo ID
    /// </summary>
    public async Task<InstrumentResponse?> GetByIdAsync(int id)
    {
        var instrument = await _db.Instruments
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InstrumentId == id);

        if (instrument == null)
            return null;

        return new InstrumentResponse(
            instrument.InstrumentId,
            instrument.InstrumentCode,
            instrument.Name,
            instrument.Status,
            instrument.ReagentStatus,
            instrument.ImageUrl,
            instrument.CreatedAt
        );
    }

    /// <summary>
    /// POST /api/instruments - Tạo máy mới
    /// </summary>
    public async Task<InstrumentResponse> CreateAsync(CreateInstrumentRequest request)
    {
        // Kiểm tra trùng mã máy
        var existing = await _db.Instruments
            .FirstOrDefaultAsync(i => i.InstrumentCode == request.InstrumentCode);

        if (existing != null)
            throw new InvalidOperationException($"Instrument with code '{request.InstrumentCode}' already exists.");

        var instrument = new DomainInstrument
        {
            InstrumentCode = request.InstrumentCode,
            Name = request.Name,
            Status = request.Status,
            ReagentStatus = ReagentStatus.OK,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        _db.Instruments.Add(instrument);
        await _db.SaveChangesAsync();

        return new InstrumentResponse(
            instrument.InstrumentId,
            instrument.InstrumentCode,
            instrument.Name,
            instrument.Status,
            instrument.ReagentStatus,
            instrument.ImageUrl,
            instrument.CreatedAt
        );
    }

    /// <summary>
    /// PUT /api/instruments/{code} - Cập nhật thông tin máy
    /// </summary>
    public async Task<InstrumentResponse?> UpdateAsync(string instrumentCode, UpdateInstrumentRequest request)
    {
        var instrument = await _db.Instruments
            .FirstOrDefaultAsync(i => i.InstrumentCode == instrumentCode);

        if (instrument == null)
            return null;

        // Cập nhật các trường nếu có giá trị
        if (!string.IsNullOrWhiteSpace(request.Name))
            instrument.Name = request.Name;

        if (request.Status.HasValue)
            instrument.Status = request.Status.Value;

        if (request.ReagentStatus.HasValue)
            instrument.ReagentStatus = request.ReagentStatus.Value;

        if (request.ImageUrl != null)
            instrument.ImageUrl = request.ImageUrl;

        await _db.SaveChangesAsync();

        return new InstrumentResponse(
            instrument.InstrumentId,
            instrument.InstrumentCode,
            instrument.Name,
            instrument.Status,
            instrument.ReagentStatus,
            instrument.ImageUrl,
            instrument.CreatedAt
        );
    }

    /// <summary>
    /// DELETE /api/instruments/{code} - Xóa máy
    /// </summary>
    public async Task<bool> DeleteAsync(string instrumentCode)
    {
        var instrument = await _db.Instruments
            .FirstOrDefaultAsync(i => i.InstrumentCode == instrumentCode);

        if (instrument == null)
            return false;

        // Kiểm tra xem máy có đang có run đang chạy không
        var hasActiveRun = await _db.InstrumentRuns
            .AnyAsync(r => r.InstrumentCode == instrumentCode && r.Status == RunStatus.Running);

        if (hasActiveRun)
            throw new InvalidOperationException("Cannot delete instrument with active runs.");

        _db.Instruments.Remove(instrument);
        await _db.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// GET /api/instruments/{code}/status - Lấy trạng thái đầy đủ của máy (bao gồm run hiện tại)
    /// </summary>
    public async Task<InstrumentStatusDto?> GetStatusAsync(string instrumentCode)
    {
        var instrument = await _db.Instruments
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InstrumentCode == instrumentCode);

        if (instrument == null)
            return null;

        var currentRun = await _db.InstrumentRuns
            .AsNoTracking()
            .Where(r => r.InstrumentCode == instrumentCode && r.Status == RunStatus.Running)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefaultAsync();

        RunDetailDto? runDetail = null;
        if (currentRun != null)
        {
            runDetail = new RunDetailDto(
                currentRun.RunId,
                currentRun.BookingId,
                currentRun.InstrumentCode,
                currentRun.Status,
                currentRun.StartedAt,
                currentRun.CompletedAt
            );
        }

        return new InstrumentStatusDto(
            instrument.InstrumentCode,
            instrument.Name,
            instrument.Status,
            instrument.ReagentStatus,
            runDetail
        );
    }
}
