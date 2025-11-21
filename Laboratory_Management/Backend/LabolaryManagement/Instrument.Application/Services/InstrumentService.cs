using Microsoft.EntityFrameworkCore;
using Instrument.Application.DTOs;
using Instrument.Infrastructure;
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
            i.LastHeartbeatAt
        )).ToList();
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
            instrument.LastHeartbeatAt,
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
            instrument.LastHeartbeatAt,
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
            Status = request.Status ?? "ONLINE",
            ReagentStatus = "OK",
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
            instrument.LastHeartbeatAt,
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

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            // Validate status
            var validStatuses = new[] { "ONLINE", "OFFLINE", "FAULT", "MAINTENANCE" };
            if (!validStatuses.Contains(request.Status.ToUpper()))
                throw new InvalidOperationException($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");

            instrument.Status = request.Status.ToUpper();
        }

        if (!string.IsNullOrWhiteSpace(request.ReagentStatus))
        {
            // Validate reagent status
            var validReagentStatuses = new[] { "OK", "LOW", "OUT" };
            if (!validReagentStatuses.Contains(request.ReagentStatus.ToUpper()))
                throw new InvalidOperationException($"Invalid reagent status. Valid values: {string.Join(", ", validReagentStatuses)}");

            instrument.ReagentStatus = request.ReagentStatus.ToUpper();
        }

        await _db.SaveChangesAsync();

        return new InstrumentResponse(
            instrument.InstrumentId,
            instrument.InstrumentCode,
            instrument.Name,
            instrument.Status,
            instrument.ReagentStatus,
            instrument.LastHeartbeatAt,
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
            .AnyAsync(r => r.InstrumentCode == instrumentCode && r.Status == "RUNNING");

        if (hasActiveRun)
            throw new InvalidOperationException("Cannot delete instrument with active runs.");

        _db.Instruments.Remove(instrument);
        await _db.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// POST /api/instruments/{code}/heartbeat - Cập nhật heartbeat
    /// </summary>
    public async Task<bool> UpdateHeartbeatAsync(string instrumentCode)
    {
        var instrument = await _db.Instruments
            .FirstOrDefaultAsync(i => i.InstrumentCode == instrumentCode);

        if (instrument == null)
            return false;

        instrument.LastHeartbeatAt = DateTime.UtcNow;
        
        // Tự động chuyển status về ONLINE nếu đang offline
        if (instrument.Status == "OFFLINE")
            instrument.Status = "ONLINE";

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

        // Lấy run đang chạy (nếu có)
        var currentRun = await _db.InstrumentRuns
            .AsNoTracking()
            .Where(r => r.InstrumentCode == instrumentCode && r.Status == "RUNNING")
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefaultAsync();

        RunDetailDto? runDetail = null;
        if (currentRun != null)
        {
            // Không có usages trong minimal version
            runDetail = new RunDetailDto(
                currentRun.RunId,
                currentRun.BookingId,
                currentRun.InstrumentCode,
                currentRun.Status,
                currentRun.StartedAt,
                currentRun.CompletedAt,
                new List<RunUsageDto>() // Empty list
            );
        }

        return new InstrumentStatusDto(
            instrument.InstrumentCode,
            instrument.Name,
            instrument.Status,
            instrument.ReagentStatus,
            instrument.LastHeartbeatAt,
            runDetail
        );
    }
}
