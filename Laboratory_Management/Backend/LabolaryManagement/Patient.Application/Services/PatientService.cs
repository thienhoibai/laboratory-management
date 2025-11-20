using Common.Errors;
using Common.Results;
using Microsoft.EntityFrameworkCore;
using Patient.Application.DTOs;
using Patient.Domain.Entities;
using Patient.Infrastructure;
using System.Security.Cryptography;
using System.Text;

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
        var isCreateForSelf = request.CreatedChannel == "self" || request.CreatedChannel == "user";
        var ownerId = isCreateForSelf ? actorUserId : (Guid?)null; // guest if created by staff

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
            UserId = ownerId,
            CreatedChannel = request.CreatedChannel,
            CreatedByUserId = actorUserId,
            UpdatedByUserId = actorUserId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };

        _db.Patients.Add(entity);

        _db.AuditLogs.Add(new AuditLog
        {
            Entity = "Patient",
            EntityId = entity.PatientId,
            Action = "Create",
            OccurredAt = DateTime.UtcNow,
            UserId = actorUserId,
            DetailJson = System.Text.Json.JsonSerializer.Serialize(new { request.FullName, request.DateOfBirth, ownerId })
        });

        _db.PatientEventLogs.Add(new PatientEventLog
        {
            PatientId = entity.PatientId,
            EventType = ownerId == null ? "CREATE_GUEST" : "CREATE_SELF",
            ActorUserId = actorUserId,
            OccurredAt = DateTime.UtcNow,
            Detail = ownerId == null ? "Created guest patient (no owner)" : "Created patient for self"
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

        if (entity.UserId.HasValue && entity.UserId != actorUserId)
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
        entity.UpdatedAt = DateTime.Now;

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

        _db.PatientEventLogs.Add(new PatientEventLog
        {
            PatientId = patientId,
            EventType = "UPDATE",
            ActorUserId = actorUserId,
            OccurredAt = DateTime.UtcNow,
            Detail = System.Text.Json.JsonSerializer.Serialize(new { changes = newSnapshot })
        });

        await _db.SaveChangesAsync(ct);

        var dto = new PatientDetailDto(entity.PatientId, newSnapshot.FullName, newSnapshot.DateOfBirth, newSnapshot.Gender, newSnapshot.Phone, newSnapshot.Email, newSnapshot.Address, newSnapshot.IdNumber, newSnapshot.InsuranceNumber, entity.UserId, entity.IsDeleted, entity.CreatedAt, entity.UpdatedAt);
        return OperationResult<PatientDetailDto>.Success(dto);
    }

    public async Task<OperationResult> DeleteAsync(Guid patientId, Guid actorUserId, string? reason = null, string? actorIp = null, CancellationToken ct = default)
    {
        var entity = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted, ct);
        if (entity == null) return OperationResult.Fail(Common.Errors.ErrorCodes.NotFound);

        if (entity.UserId.HasValue && entity.UserId != actorUserId)
            return OperationResult.Fail(Common.Errors.ErrorCodes.Forbidden);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedByUserId = actorUserId;
        entity.UpdatedAt = DateTime.Now;

        _db.PatientEventLogs.Add(new PatientEventLog
        {
            PatientId = patientId,
            EventType = "DELETE",
            ActorUserId = actorUserId,
            OccurredAt = DateTime.UtcNow,
            Detail = reason
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
        var q = _db.Patients.AsNoTracking().IgnoreQueryFilters();
        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(p => p.FullName != null && p.FullName.Contains(name));
        if (dob.HasValue) q = q.Where(p => p.DateOfBirth == dob);
        if (isDeleted.HasValue) q = q.Where(p => p.IsDeleted == isDeleted.Value);
        if (!string.IsNullOrWhiteSpace(phoneLast4)) q = q.Where(p => p.Phone != null && p.Phone.EndsWith(phoneLast4));
        if (!string.IsNullOrWhiteSpace(idLast4)) q = q.Where(p => p.IdNumber != null && p.IdNumber.EndsWith(idLast4));

        q = sortBy?.ToLowerInvariant() switch
        {
            "name" => (sortDir?.ToLowerInvariant() == "desc" ? q.OrderByDescending(x => x.FullName) : q.OrderBy(x => x.FullName)),
            "email" => (sortDir?.ToLowerInvariant() == "desc" ? q.OrderByDescending(x => x.Email) : q.OrderBy(x => x.Email)),
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
            "email" => (sortDir?.ToLowerInvariant() == "desc" ? q.OrderByDescending(x => x.Email) : q.OrderBy(x => x.Email)),
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

    public async Task<OperationResult<PatientDto>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        var patient = await _db.Patients
            .AsNoTracking()
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .Select(p => new PatientDto
            {
                PatientId = p.PatientId,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
                Email = p.Email,
                Phone = p.Phone,
                Address = p.Address,
                IdNumber = p.IdNumber,
                InsuranceNumber = p.InsuranceNumber,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (patient == null)
            return OperationResult<PatientDto>.Fail(ErrorCodes.NotFound);

        return OperationResult<PatientDto>.Success(patient);
    }

    // ========== Guest linking flows ==========
    public async Task<OperationResult> StartLinkAsync(Guid patientId, Guid actorUserId, string mode, string baseLinkUrl, CancellationToken ct = default)
    {
        var p = await _db.Patients.FirstOrDefaultAsync(x => x.PatientId == patientId, ct);
        if (p == null) return OperationResult.Fail(ErrorCodes.NotFound);
        if (p.UserId.HasValue) return OperationResult.Success(); // already linked

        if (string.Equals(mode, "magic", StringComparison.OrdinalIgnoreCase))
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=');
            using var sha = SHA256.Create();
            var tokenHash = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            var rec = new PatientLinkToken
            {
                TokenId = Guid.NewGuid(),
                PatientId = p.PatientId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow,
                Mode = "magic"
            };
            _db.PatientLinkTokens.Add(rec);
            _db.PatientEventLogs.Add(new PatientEventLog { PatientId = p.PatientId, EventType = "LINK_MAGIC_SENT", ActorUserId = actorUserId, OccurredAt = DateTime.UtcNow });
            await _db.SaveChangesAsync(ct);

            // TODO: send email with link
            // e.g. link: ${baseLinkUrl}?token={token}
            return OperationResult.Success();
        }
        else
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            using var sha = SHA256.Create();
            var codeHash = sha.ComputeHash(Encoding.UTF8.GetBytes(code));
            var rec = new PatientOtpToken
            {
                OtpId = Guid.NewGuid(),
                PatientId = p.PatientId,
                CodeHash = codeHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Attempts = 0,
                CreatedAt = DateTime.UtcNow
            };
            _db.PatientOtpTokens.Add(rec);
            _db.PatientEventLogs.Add(new PatientEventLog { PatientId = p.PatientId, EventType = "LINK_OTP_SENT", ActorUserId = actorUserId, OccurredAt = DateTime.UtcNow });
            await _db.SaveChangesAsync(ct);

            // TODO: send email with code
            return OperationResult.Success();
        }
    }

    public async Task<OperationResult> ConfirmMagicLinkAsync(string token, Guid actorUserId, CancellationToken ct = default)
    {
        using var sha = SHA256.Create();
        var tokenHash = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        var rec = await _db.PatientLinkTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
        if (rec == null) return OperationResult.Fail(ErrorCodes.InvalidResetToken);
        if (rec.UsedAt.HasValue || rec.ExpiresAt <= DateTime.UtcNow) return OperationResult.Fail(ErrorCodes.ResetTokenExpired);

        var p = await _db.Patients.FirstOrDefaultAsync(x => x.PatientId == rec.PatientId, ct);
        if (p == null) return OperationResult.Fail(ErrorCodes.NotFound);
        if (p.UserId.HasValue) return OperationResult.Success();

        p.UserId = actorUserId;
        p.UpdatedAt = DateTime.Now;
        rec.UsedAt = DateTime.UtcNow;

        _db.PatientEventLogs.Add(new PatientEventLog { PatientId = p.PatientId, EventType = "LINK_MAGIC_CONFIRMED", ActorUserId = actorUserId, OccurredAt = DateTime.UtcNow });
        await _db.SaveChangesAsync(ct);

        // TODO (optional): call TestOrder to update QR/ticket mode
        return OperationResult.Success();
    }

    public async Task<OperationResult> RequestOtpAsync(Guid patientId, Guid actorUserId, CancellationToken ct = default)
    {
        var p = await _db.Patients.FirstOrDefaultAsync(x => x.PatientId == patientId, ct);
        if (p == null) return OperationResult.Fail(ErrorCodes.NotFound);
        if (p.UserId.HasValue) return OperationResult.Success();

        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        using var sha = SHA256.Create();
        var codeHash = sha.ComputeHash(Encoding.UTF8.GetBytes(code));
        var rec = new PatientOtpToken
        {
            OtpId = Guid.NewGuid(),
            PatientId = p.PatientId,
            CodeHash = codeHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Attempts = 0,
            CreatedAt = DateTime.UtcNow
        };
        _db.PatientOtpTokens.Add(rec);
        _db.PatientEventLogs.Add(new PatientEventLog { PatientId = p.PatientId, EventType = "LINK_OTP_SENT", ActorUserId = actorUserId, OccurredAt = DateTime.UtcNow });
        await _db.SaveChangesAsync(ct);

        // TODO: send email with code
        return OperationResult.Success();
    }

    public async Task<OperationResult> VerifyOtpAsync(Guid patientId, string code, Guid actorUserId, CancellationToken ct = default)
    {
        var rec = await _db.PatientOtpTokens.Where(x => x.PatientId == patientId).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(ct);
        if (rec == null) return OperationResult.Fail(ErrorCodes.InvalidResetToken);
        if (rec.UsedAt.HasValue || rec.ExpiresAt <= DateTime.UtcNow) return OperationResult.Fail(ErrorCodes.ResetTokenExpired);
        if (rec.Attempts >= 5) return OperationResult.Fail(ErrorCodes.RateLimited);

        using var sha = SHA256.Create();
        var codeHash = sha.ComputeHash(Encoding.UTF8.GetBytes(code));
        var ok = codeHash.SequenceEqual(rec.CodeHash);
        rec.Attempts++;
        if (!ok)
        {
            await _db.SaveChangesAsync(ct);
            return OperationResult.Fail(ErrorCodes.InvalidCredentials);
        }

        var p = await _db.Patients.FirstOrDefaultAsync(x => x.PatientId == patientId, ct);
        if (p == null) return OperationResult.Fail(ErrorCodes.NotFound);
        if (p.UserId.HasValue) return OperationResult.Success();

        p.UserId = actorUserId;
        p.UpdatedAt = DateTime.Now;
        rec.UsedAt = DateTime.UtcNow;

        _db.PatientEventLogs.Add(new PatientEventLog { PatientId = p.PatientId, EventType = "LINK_OTP_CONFIRMED", ActorUserId = actorUserId, OccurredAt = DateTime.UtcNow });
        await _db.SaveChangesAsync(ct);
        return OperationResult.Success();
    }

    public async Task<OperationResult<PatientDto>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        var patient = await _db.Patients
            .AsNoTracking()
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .Select(p => new PatientDto
            {
                PatientId = p.PatientId,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
                Email = p.Email,
                Phone = p.Phone,
                Address = p.Address,
                IdNumber = p.IdNumber,
                InsuranceNumber = p.InsuranceNumber,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (patient == null)
            return OperationResult<PatientDto>.Fail(ErrorCodes.NotFound);

        return OperationResult<PatientDto>.Success(patient);
    }
    public async Task<IReadOnlyList<PatientSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var patients = await _db.Patients
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PatientSummaryDto(
                p.PatientId,
                p.FullName,
                p.DateOfBirth,
                p.Gender,
                Last4(p.Phone),
                p.IsDeleted,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToListAsync(ct);

        return patients;
    }

}
