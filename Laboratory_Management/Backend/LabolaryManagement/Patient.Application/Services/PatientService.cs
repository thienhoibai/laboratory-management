using Common.Results;
using Microsoft.EntityFrameworkCore;
using Patient.Application.DTOs;
using Patient.Domain.Entities;
using Patient.Infrastructure;

namespace Patient.Application.Services;

public class PatientService : IPatientService
{
    private readonly PatientDbContext _db;

    public PatientService(PatientDbContext db)
    {
        _db = db;
    }

    private static string? Last4(string? s)
        => string.IsNullOrWhiteSpace(s) || s.Length < 4 ? null : s[^4..];

    public Task<bool> IsOwnerAsync(Guid patientId, Guid actorUserId, CancellationToken ct)
        => _db.Patients.AnyAsync(p => p.PatientId == patientId && p.UserId == actorUserId, ct);

    public async Task<OperationResult<PatientDetailDto>> CreateAsync(
        CreatePatientRequest request, Guid actorUserId, string? actorIp = null, CancellationToken ct = default)
    {
        var entity = new PatientEntity
        {
            PatientId = Guid.NewGuid(),
            FullName = request.FullName,
            Gender = request.Gender,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            IdNumber = request.IdNumber,
            InsuranceNumber = request.InsuranceNumber,
            DateOfBirth = request.DateOfBirth,
            UserId = actorUserId,
            CreatedChannel = request.CreatedChannel,
            CreatedByUserId = actorUserId,
            UpdatedByUserId = actorUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _db.Patients.Add(entity);

        // Only write audit log; skip event log and record versions (tables not present in DB)
        _db.AuditLogs.Add(new AuditLog
        {
            Entity = "Patient",
            EntityId = entity.PatientId,
            Action = "Create",
            OccurredAt = DateTime.UtcNow,
            UserId = actorUserId,
            DetailJson = System.Text.Json.JsonSerializer.Serialize(new { request.FullName, request.DateOfBirth })
        });

        await _db.SaveChangesAsync(ct);

        var dto = new PatientDetailDto(
            entity.PatientId, request.FullName, request.DateOfBirth, request.Gender,
            request.Phone, request.Email, request.Address, request.IdNumber, request.InsuranceNumber,
            entity.UserId, false, entity.CreatedAt, entity.UpdatedAt);

        return OperationResult<PatientDetailDto>.Success(dto);
    }

    public async Task<OperationResult<PatientDetailDto>> UpdateAsync(Guid patientId, UpdatePatientRequest request, Guid actorUserId, string? actorIp = null, CancellationToken ct = default)
    {
        var entity = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted, ct);
        if (entity == null) return OperationResult<PatientDetailDto>.Fail(Common.Errors.ErrorCodes.NotFound);

        if (!await IsOwnerAsync(patientId, actorUserId, ct))
            return OperationResult<PatientDetailDto>.Fail(Common.Errors.ErrorCodes.Forbidden);

        var oldSnapshot = new
        {
            entity.FullName,
            entity.DateOfBirth,
            entity.Gender,
            entity.Phone,
            entity.Email,
            entity.Address,
            entity.IdNumber,
            entity.InsuranceNumber,
            entity.UserId
        };

        if (request.FullName != null) { entity.FullName = request.FullName; }
        if (request.DateOfBirth.HasValue) { entity.DateOfBirth = request.DateOfBirth; }
        if (request.Gender.HasValue) entity.Gender = request.Gender.Value;
        if (request.Phone != null) { entity.Phone = request.Phone; }
        if (request.Email != null) entity.Email = request.Email;
        if (request.Address != null) entity.Address = request.Address;
        if (request.IdNumber != null) { entity.IdNumber = request.IdNumber; }
        if (request.InsuranceNumber != null) entity.InsuranceNumber = request.InsuranceNumber;

        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        var newSnapshot = new
        {
            entity.FullName,
            entity.DateOfBirth,
            entity.Gender,
            entity.Phone,
            entity.Email,
            entity.Address,
            entity.IdNumber,
            entity.InsuranceNumber,
            entity.UserId
        };

        // Audit only
        _db.AuditLogs.Add(new AuditLog
        {
            Entity = "Patient",
            EntityId = patientId,
            Action = "Update",
            OccurredAt = DateTime.UtcNow,
            UserId = actorUserId,
            DetailJson = System.Text.Json.JsonSerializer.Serialize(new { changes = newSnapshot })
        });

        await _db.SaveChangesAsync(ct);

        var dto = new PatientDetailDto(entity.PatientId, newSnapshot.FullName, newSnapshot.DateOfBirth, newSnapshot.Gender, newSnapshot.Phone, newSnapshot.Email, newSnapshot.Address, newSnapshot.IdNumber, newSnapshot.InsuranceNumber, entity.UserId, entity.IsDeleted, entity.CreatedAt, entity.UpdatedAt);
        return OperationResult<PatientDetailDto>.Success(dto);
    }

    public async Task<OperationResult> DeleteAsync(Guid patientId, Guid actorUserId, string? reason = null, string? actorIp = null, CancellationToken ct = default)
    {
        var entity = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted, ct);
        if (entity == null) return OperationResult.Fail(Common.Errors.ErrorCodes.NotFound);

        if (!await IsOwnerAsync(patientId, actorUserId, ct))
            return OperationResult.Fail(Common.Errors.ErrorCodes.Forbidden);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedByUserId = actorUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            Entity = "Patient",
            EntityId = patientId,
            Action = "Delete",
            OccurredAt = DateTime.UtcNow,
            UserId = actorUserId,
            DetailJson = reason
        });

        await _db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult<PatientDetailDto>> GetAsync(Guid patientId, CancellationToken ct = default)
    {
        var e = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == patientId, ct);
        if (e == null) return OperationResult<PatientDetailDto>.Fail(Common.Errors.ErrorCodes.NotFound);
        var dto = new PatientDetailDto(e.PatientId, e.FullName, e.DateOfBirth, e.Gender, e.Phone, e.Email, e.Address, e.IdNumber, e.InsuranceNumber, e.UserId, e.IsDeleted, e.CreatedAt, e.UpdatedAt);
        return OperationResult<PatientDetailDto>.Success(dto);
    }

    public async Task<(IReadOnlyList<PatientSummaryDto> Items, long Total)> ListAsync(int page, int pageSize, string? name, DateOnly? dob, bool? isDeleted, string? sortBy, string? sortDir, string? idLast4, string? phoneLast4, CancellationToken ct = default)
    {
        var q = _db.Patients.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(name))
        {
            q = q.Where(p => p.FullName != null && p.FullName.Contains(name));
        }
        if (dob.HasValue) q = q.Where(p => p.DateOfBirth == dob);
        if (isDeleted.HasValue) q = q.Where(p => p.IsDeleted == isDeleted.Value);
        if (!string.IsNullOrWhiteSpace(phoneLast4)) q = q.Where(p => p.Phone != null && p.Phone.EndsWith(phoneLast4));
        if (!string.IsNullOrWhiteSpace(idLast4)) q = q.Where(p => p.IdNumber != null && p.IdNumber.EndsWith(idLast4));

        q = sortBy?.ToLowerInvariant() switch
        {
            "name" => (sortDir?.ToLowerInvariant() == "desc" ? q.OrderByDescending(x => x.FullName) : q.OrderBy(x => x.FullName)),
            "createdat" => (sortDir?.ToLowerInvariant() == "asc" ? q.OrderBy(x => x.CreatedAt) : q.OrderByDescending(x => x.CreatedAt)),
            "updatedat" => (sortDir?.ToLowerInvariant() == "asc" ? q.OrderBy(x => x.UpdatedAt) : q.OrderByDescending(x => x.UpdatedAt)),
            _ => q.OrderByDescending(x => x.CreatedAt)
        };

        var total = await q.LongCountAsync(ct);
        var data = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new { e.PatientId, e.FullName, e.DateOfBirth, e.Gender, e.Phone, e.IsDeleted, e.CreatedAt, e.UpdatedAt })
            .ToListAsync(ct);

        var items = data.Select(e => new PatientSummaryDto(e.PatientId, e.FullName, e.DateOfBirth, e.Gender, Last4(e.Phone), e.IsDeleted, e.CreatedAt, e.UpdatedAt)).ToList();

        return (items, total);
    }

    public Task<(IReadOnlyList<PatientVersionDto> Items, long Total)> GetVersionsAsync(Guid patientId, int page, int pageSize, string? sortDir, CancellationToken ct = default)
    {
        // Version table not present in DB, return empty
        return Task.FromResult(((IReadOnlyList<PatientVersionDto>)Array.Empty<PatientVersionDto>(), 0L));
    }

    public async Task<(IReadOnlyList<PatientSummaryDto> Items, long Total)> ListByOwnerAsync(Guid ownerUserId, int page, int pageSize, string? name, DateOnly? dob, string? sortBy, string? sortDir, CancellationToken ct = default)
    {
        var q = _db.Patients.AsNoTracking().Where(p => p.UserId == ownerUserId);
        if (!string.IsNullOrWhiteSpace(name)) q = q.Where(p => p.FullName != null && p.FullName.Contains(name));
        if (dob.HasValue) q = q.Where(p => p.DateOfBirth == dob);

        q = sortBy?.ToLowerInvariant() switch
        {
            "name" => (sortDir?.ToLowerInvariant() == "desc" ? q.OrderByDescending(x => x.FullName) : q.OrderBy(x => x.FullName)),
            "createdat" => (sortDir?.ToLowerInvariant() == "asc" ? q.OrderBy(x => x.CreatedAt) : q.OrderByDescending(x => x.CreatedAt)),
            "updatedat" => (sortDir?.ToLowerInvariant() == "asc" ? q.OrderBy(x => x.UpdatedAt) : q.OrderByDescending(x => x.UpdatedAt)),
            _ => q.OrderByDescending(x => x.CreatedAt)
        };

        var total = await q.LongCountAsync(ct);
        var list = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new PatientSummaryDto(e.PatientId, e.FullName, e.DateOfBirth, e.Gender, Last4(e.Phone), e.IsDeleted, e.CreatedAt, e.UpdatedAt))
            .ToListAsync(ct);
        return (list, total);
    }
}
