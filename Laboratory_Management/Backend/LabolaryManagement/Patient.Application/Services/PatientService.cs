using Common.Results;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Patient.Application.DTOs;
using Patient.Application.Security;
using Patient.Domain.Entities;
using Patient.Infrastructure;

namespace Patient.Application.Services;

public class PatientService : IPatientService
{
    private readonly PatientDbContext _db;
    private readonly ISensitiveDataProtector _pii;
    private readonly IPublishEndpoint _bus;

    public PatientService(PatientDbContext db, ISensitiveDataProtector pii, IPublishEndpoint bus)
    {
        _db = db; _pii = pii; _bus = bus;
    }

    private static string NormalizeName(string? name)
        => string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim().ToUpperInvariant();

    private static string? Last4(string? phone)
        => string.IsNullOrWhiteSpace(phone) || phone.Length < 4 ? null : phone[^4..];

    public Task<bool> IsOwnerAsync(Guid patientId, Guid actorUserId, CancellationToken ct)
        => _db.Patients.AnyAsync(p => p.PatientId == patientId && p.UserId == actorUserId, ct);

    public async Task<OperationResult<PatientDetailDto>> CreateAsync(
    CreatePatientRequest request, Guid actorUserId, string? actorIp = null, CancellationToken ct = default)
    {
        var entity = new PatientEntity
        {
            // Nếu DB tự sinh patient_id thì bỏ dòng này
            PatientId = Guid.NewGuid(),
            FullNameEnc = _pii.Encrypt(request.FullName),
            DobEnc = request.DateOfBirth.HasValue ? _pii.Encrypt(request.DateOfBirth.Value.ToString("yyyy-MM-dd")) : null,
            Gender = request.Gender,
            PhoneEnc = _pii.Encrypt(request.Phone),
            EmailEnc = _pii.Encrypt(request.Email),
            AddressEnc = _pii.Encrypt(request.Address),
            IdNumberEnc = _pii.Encrypt(request.IdNumber),
            InsuranceNumberEnc = _pii.Encrypt(request.InsuranceNumber),
            FullNameNorm = NormalizeName(request.FullName),
            DateOfBirth = request.DateOfBirth,
            PhoneLast4 = Last4(request.Phone),
            // Ép owner = actor, không nhận từ request
            UserId = actorUserId,
            CreatedChannel = request.CreatedChannel,
            CreatedByUserId = actorUserId,
            UpdatedByUserId = actorUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _db.Patients.Add(entity);

        var version = new PatientRecordVersion
        {
            Patient = entity,
            VersionNo = 1,
            ChangedBy = actorUserId,
            ChangedAt = DateTime.UtcNow,
            FullSnapshot = System.Text.Json.JsonSerializer.Serialize(new
            {
                entity.PatientId,
                request.FullName,
                request.DateOfBirth,
                request.Gender,
                request.Phone,
                request.Email,
                request.Address,
                request.IdNumber,
                request.InsuranceNumber,
                UserId = actorUserId
            })
        };
        var log = new PatientEventLog
        {
            Patient = entity,
            EventType = "Created",
            ActorUserId = actorUserId,
            Detail = actorIp == null ? null : System.Text.Json.JsonSerializer.Serialize(new { actorIp }),
            OccurredAt = DateTime.UtcNow
        };

        _db.PatientRecordVersions.Add(version);
        _db.PatientEventLogs.Add(log);

        await _db.SaveChangesAsync(ct);

        await _bus.Publish(new Contracts.Events.Patient.PatientCreatedV1(
            entity.PatientId, actorUserId, entity.FullNameNorm ?? string.Empty, DateTime.UtcNow), ct);

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

        // Double-safety: chỉ chủ sở hữu được cập nhật
        if (entity.UserId != actorUserId)
            return OperationResult<PatientDetailDto>.Fail(Common.Errors.ErrorCodes.Forbidden);

        var oldSnapshot = new
        {
            FullName = _pii.Decrypt(entity.FullNameEnc),
            DateOfBirth = entity.DateOfBirth,
            Gender = entity.Gender,
            Phone = _pii.Decrypt(entity.PhoneEnc),
            Email = _pii.Decrypt(entity.EmailEnc),
            Address = _pii.Decrypt(entity.AddressEnc),
            IdNumber = _pii.Decrypt(entity.IdNumberEnc),
            InsuranceNumber = _pii.Decrypt(entity.InsuranceNumberEnc),
            UserId = entity.UserId
        };

        if (request.FullName != null) { entity.FullNameEnc = _pii.Encrypt(request.FullName); entity.FullNameNorm = NormalizeName(request.FullName); }
        if (request.DateOfBirth.HasValue) { entity.DobEnc = _pii.Encrypt(request.DateOfBirth.Value.ToString("yyyy-MM-dd")); entity.DateOfBirth = request.DateOfBirth; }
        if (request.Gender.HasValue) entity.Gender = request.Gender.Value;
        if (request.Phone != null) { entity.PhoneEnc = _pii.Encrypt(request.Phone); entity.PhoneLast4 = Last4(request.Phone); }
        if (request.Email != null) entity.EmailEnc = _pii.Encrypt(request.Email);
        if (request.Address != null) entity.AddressEnc = _pii.Encrypt(request.Address);
        if (request.IdNumber != null) entity.IdNumberEnc = _pii.Encrypt(request.IdNumber);
        if (request.InsuranceNumber != null) entity.InsuranceNumberEnc = _pii.Encrypt(request.InsuranceNumber);
        // KHÔNG cho đổi chủ
        // if (request.UserId.HasValue) entity.UserId = request.UserId;

        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        var newSnapshot = new
        {
            FullName = _pii.Decrypt(entity.FullNameEnc),
            DateOfBirth = entity.DateOfBirth,
            Gender = entity.Gender,
            Phone = _pii.Decrypt(entity.PhoneEnc),
            Email = _pii.Decrypt(entity.EmailEnc),
            Address = _pii.Decrypt(entity.AddressEnc),
            IdNumber = _pii.Decrypt(entity.IdNumberEnc),
            InsuranceNumber = _pii.Decrypt(entity.InsuranceNumberEnc),
            UserId = entity.UserId
        };

        var changes = new List<object>();
        void diff(string field, object? oldV, object? newV)
        {
            if (!Equals(oldV, newV)) changes.Add(new { field, old = oldV, @new = newV });
        }

        diff("FullName", oldSnapshot.FullName, newSnapshot.FullName);
        diff("DateOfBirth", oldSnapshot.DateOfBirth, newSnapshot.DateOfBirth);
        diff("Gender", oldSnapshot.Gender, newSnapshot.Gender);
        diff("Phone", oldSnapshot.Phone, newSnapshot.Phone);
        diff("Email", oldSnapshot.Email, newSnapshot.Email);
        diff("Address", oldSnapshot.Address, newSnapshot.Address);
        diff("IdNumber", oldSnapshot.IdNumber, newSnapshot.IdNumber);
        diff("InsuranceNumber", oldSnapshot.InsuranceNumber, newSnapshot.InsuranceNumber);
        diff("UserId", oldSnapshot.UserId, newSnapshot.UserId);

        var latestVersionNo = await _db.PatientRecordVersions.Where(v => v.PatientId == patientId).Select(v => (int?)v.VersionNo).MaxAsync(ct) ?? 0;
        _db.PatientRecordVersions.Add(new PatientRecordVersion
        {
            PatientId = patientId,
            VersionNo = latestVersionNo + 1,
            ChangedBy = actorUserId,
            ChangedAt = DateTime.UtcNow,
            ChangeSet = System.Text.Json.JsonSerializer.Serialize(changes),
            FullSnapshot = System.Text.Json.JsonSerializer.Serialize(newSnapshot)
        });

        _db.PatientEventLogs.Add(new PatientEventLog
        {
            PatientId = patientId,
            EventType = "Updated",
            ActorUserId = actorUserId,
            Detail = System.Text.Json.JsonSerializer.Serialize(new { changesCount = changes.Count }),
            OccurredAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        await _bus.Publish(new Contracts.Events.Patient.PatientUpdatedV1(
            entity.PatientId, actorUserId, DateTime.UtcNow, changes.Count), ct);

        var dto = new PatientDetailDto(entity.PatientId, newSnapshot.FullName, newSnapshot.DateOfBirth, newSnapshot.Gender, newSnapshot.Phone, newSnapshot.Email, newSnapshot.Address, newSnapshot.IdNumber, newSnapshot.InsuranceNumber, entity.UserId, entity.IsDeleted, entity.CreatedAt, entity.UpdatedAt);
        return OperationResult<PatientDetailDto>.Success(dto);
    }

    public async Task<OperationResult> DeleteAsync(Guid patientId, Guid actorUserId, string? reason = null, string? actorIp = null, CancellationToken ct = default)
    {
        var entity = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted, ct);
        if (entity == null) return OperationResult.Fail(Common.Errors.ErrorCodes.NotFound);

        // Double-safety: chỉ chủ sở hữu được xóa
        if (entity.UserId != actorUserId)
            return OperationResult.Fail(Common.Errors.ErrorCodes.Forbidden);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedByUserId = actorUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        _db.PatientEventLogs.Add(new PatientEventLog
        {
            PatientId = patientId,
            EventType = "Deleted",
            ActorUserId = actorUserId,
            Detail = reason == null ? null : System.Text.Json.JsonSerializer.Serialize(new { reason }),
            OccurredAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        await _bus.Publish(new Contracts.Events.Patient.PatientDeletedV1(
            patientId, actorUserId, DateTime.UtcNow), ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult<PatientDetailDto>> GetAsync(Guid patientId, CancellationToken ct = default)
    {
        var e = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == patientId, ct);
        if (e == null) return OperationResult<PatientDetailDto>.Fail(Common.Errors.ErrorCodes.NotFound);
        var dto = new PatientDetailDto(e.PatientId, _pii.Decrypt(e.FullNameEnc), e.DateOfBirth, e.Gender, _pii.Decrypt(e.PhoneEnc), _pii.Decrypt(e.EmailEnc), _pii.Decrypt(e.AddressEnc), _pii.Decrypt(e.IdNumberEnc), _pii.Decrypt(e.InsuranceNumberEnc), e.UserId, e.IsDeleted, e.CreatedAt, e.UpdatedAt);
        return OperationResult<PatientDetailDto>.Success(dto);
    }


    public async Task<(IReadOnlyList<PatientSummaryDto> Items, long Total)> ListAsync(int page, int pageSize, string? name, DateOnly? dob, bool? isDeleted, string? sortBy, string? sortDir, CancellationToken ct = default)
    {
        var q = _db.Patients.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(name))
        {
            var norm = name.Trim().ToUpperInvariant();
            q = q.Where(p => p.FullNameNorm != null && p.FullNameNorm.Contains(norm));
        }
        if (dob.HasValue) q = q.Where(p => p.DateOfBirth == dob);
        if (isDeleted.HasValue) q = q.Where(p => p.IsDeleted == isDeleted.Value);

        q = sortBy?.ToLowerInvariant() switch
        {
            "name" => (sortDir?.ToLowerInvariant() == "desc" ? q.OrderByDescending(x => x.FullNameNorm) : q.OrderBy(x => x.FullNameNorm)),
            "createdat" => (sortDir?.ToLowerInvariant() == "asc" ? q.OrderBy(x => x.CreatedAt) : q.OrderByDescending(x => x.CreatedAt)),
            "updatedat" => (sortDir?.ToLowerInvariant() == "asc" ? q.OrderBy(x => x.UpdatedAt) : q.OrderByDescending(x => x.UpdatedAt)),
            _ => q.OrderByDescending(x => x.UpdatedAt)
        };

        var total = await q.LongCountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new PatientSummaryDto(e.PatientId, e.FullNameNorm, e.DateOfBirth, e.Gender, e.PhoneLast4, e.IsDeleted, e.CreatedAt, e.UpdatedAt))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyList<PatientVersionDto> Items, long Total)> GetVersionsAsync(Guid patientId, int page, int pageSize, string? sortDir, CancellationToken ct = default)
    {
        var q = _db.PatientRecordVersions.AsNoTracking().Where(v => v.PatientId == patientId);
        q = sortDir?.ToLowerInvariant() == "asc" ? q.OrderBy(v => v.ChangedAt) : q.OrderByDescending(v => v.ChangedAt);
        var total = await q.LongCountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(v => new PatientVersionDto(v.VersionId, v.VersionNo, v.ChangedBy, v.ChangedAt, v.ChangeSet, v.FullSnapshot))
            .ToListAsync(ct);
        return (items, total);
    }
}
