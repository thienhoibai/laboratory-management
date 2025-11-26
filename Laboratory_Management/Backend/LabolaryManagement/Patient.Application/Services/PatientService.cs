using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Errors;
using Common.Results;
using Microsoft.EntityFrameworkCore;
using Patient.Application.Patients.DTOs.Requests;
using Patient.Application.Patients.DTOs.Responses;
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

    public Task<bool> IsOwnerAsync(Guid patientId, Guid actorUserId, CancellationToken ct)
        => _db.Patients.AnyAsync(p => p.PatientId == patientId && p.UserId == actorUserId, ct);

    public async Task<OperationResult<PatientDetailDto>> CreateAsync(
        CreatePatientRequest request, Guid actorUserId, string? actorIp = null, CancellationToken ct = default)
    {
        // ✅ VALIDATION 1: Required fields (CHO PHÉP TRÙNG - nhiều người có thể cùng tên)
        if (string.IsNullOrWhiteSpace(request.FullName))
            return OperationResult<PatientDetailDto>.Fail("Họ và tên không được để trống");

        // ✅ VALIDATION 2: Required DateOfBirth (CHO PHÉP TRÙNG - nhiều người cùng ngày sinh)
        if (!request.DateOfBirth.HasValue)
            return OperationResult<PatientDetailDto>.Fail("Ngày sinh không được để trống");

        // ✅ VALIDATION 3: Age validation
        var today = DateOnly.FromDateTime(DateTime.Now);
        var age = today.Year - request.DateOfBirth.Value.Year;
        
        // Adjust age if birthday hasn't occurred this year
        if (request.DateOfBirth.Value > today.AddYears(-age))
            age--;
        
        if (age < 0 || age > 150)
            return OperationResult<PatientDetailDto>.Fail("Tuổi không hợp lệ (phải từ 0 đến 150)");

        // Future date check
        if (request.DateOfBirth.Value > today)
            return OperationResult<PatientDetailDto>.Fail("Ngày sinh không được là ngày trong tương lai");

        // ✅ VALIDATION 4: CitizenId phải UNIQUE nếu có (CMND/CCCD là duy nhất)
        if (!string.IsNullOrWhiteSpace(request.CitizenId))
        {
            var citizenIdExists = await _db.Patients
                .AnyAsync(p => p.CitizenId == request.CitizenId && !p.IsDeleted, ct);
            
            if (citizenIdExists)
                return OperationResult<PatientDetailDto>.Fail("Số CMND/CCCD này đã được sử dụng bởi bệnh nhân khác");
        }

        // ✅ VALIDATION 5: InsuranceNumber phải UNIQUE nếu có (Số BHYT là duy nhất)
        if (!string.IsNullOrWhiteSpace(request.InsuranceNumber))
        {
            var insuranceExists = await _db.Patients
                .AnyAsync(p => p.InsuranceNumber == request.InsuranceNumber && !p.IsDeleted, ct);
            
            if (insuranceExists)
                return OperationResult<PatientDetailDto>.Fail("Số bảo hiểm y tế này đã được sử dụng bởi bệnh nhân khác");
        }

        // ✅ VALIDATION 6: Email phải UNIQUE nếu có (Email cá nhân riêng)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _db.Patients
                .AnyAsync(p => p.Email == request.Email && !p.IsDeleted, ct);
            
            if (emailExists)
                return OperationResult<PatientDetailDto>.Fail("Email này đã được sử dụng bởi bệnh nhân khác");
        }

        // ✅ VALIDATION 7: Gender validation
        if (request.Gender > 3)
            return OperationResult<PatientDetailDto>.Fail("Giới tính không hợp lệ (0=Không xác định, 1=Nam, 2=Nữ, 3=Khác)");

        // ✅ NOTE: FullName CHO PHÉP TRÙNG - Nhiều người có thể cùng tên
        // ✅ NOTE: DateOfBirth CHO PHÉP TRÙNG - Nhiều người có thể cùng ngày sinh
        // ✅ NOTE: Phone CHO PHÉP TRÙNG - Trẻ em có thể dùng SĐT bố mẹ
        // ✅ NOTE: Address CHO PHÉP TRÙNG - Gia đình có thể cùng địa chỉ

        var isCreateForSelf = request.CreatedChannel == "self" || request.CreatedChannel == "user";
        var ownerId = isCreateForSelf ? actorUserId : (Guid?)null;

        var entity = new PatientEntity
        {
            PatientId = Guid.NewGuid(),
            FullName = request.FullName,
            Gender = request.Gender,
            BloodType = request.BloodType,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            CitizenId = request.CitizenId,
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
            DetailJson = JsonSerializer.Serialize(new { request.FullName, request.DateOfBirth, request.BloodType, ownerId })
        });

        await _db.SaveChangesAsync(ct);

        var dto = new PatientDetailDto(
            entity.PatientId, request.FullName, request.DateOfBirth, request.Gender, request.BloodType,
            request.Phone, request.Email, request.Address, request.CitizenId, request.InsuranceNumber,
            entity.UserId, false, entity.CreatedAt, entity.UpdatedAt);

        return OperationResult<PatientDetailDto>.Success(dto);
    }

    public async Task<OperationResult<PatientDetailDto>> UpdateAsync(
        Guid patientId, UpdatePatientRequest request, Guid actorUserId, string? actorIp = null, CancellationToken ct = default)
    {
        var entity = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted, ct);
        if (entity == null) return OperationResult<PatientDetailDto>.Fail(ErrorCodes.NotFound);

        if (entity.UserId.HasValue && entity.UserId != actorUserId)
            return OperationResult<PatientDetailDto>.Fail(ErrorCodes.Forbidden);

        // ✅ VALIDATION 1: FullName nếu update
        if (request.FullName != null && string.IsNullOrWhiteSpace(request.FullName))
            return OperationResult<PatientDetailDto>.Fail("Họ và tên không được để trống");

        // ✅ VALIDATION 2: DateOfBirth nếu update
        if (request.DateOfBirth.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var age = today.Year - request.DateOfBirth.Value.Year;
            
            if (request.DateOfBirth.Value > today.AddYears(-age))
                age--;
            
            if (age < 0 || age > 150)
                return OperationResult<PatientDetailDto>.Fail("Tuổi không hợp lệ (phải từ 0 đến 150)");

            if (request.DateOfBirth.Value > today)
                return OperationResult<PatientDetailDto>.Fail("Ngày sinh không được là ngày trong tương lai");
        }

        // ✅ VALIDATION 3: CitizenId phải unique nếu thay đổi
        if (request.CitizenId != null && request.CitizenId != entity.CitizenId)
        {
            if (!string.IsNullOrWhiteSpace(request.CitizenId))
            {
                var citizenIdExists = await _db.Patients
                    .AnyAsync(p => p.CitizenId == request.CitizenId && p.PatientId != patientId && !p.IsDeleted, ct);
                
                if (citizenIdExists)
                    return OperationResult<PatientDetailDto>.Fail("Số CMND/CCCD này đã được sử dụng bởi bệnh nhân khác");
            }
        }

        // ✅ VALIDATION 4: InsuranceNumber phải unique nếu thay đổi
        if (request.InsuranceNumber != null && request.InsuranceNumber != entity.InsuranceNumber)
        {
            if (!string.IsNullOrWhiteSpace(request.InsuranceNumber))
            {
                var insuranceExists = await _db.Patients
                    .AnyAsync(p => p.InsuranceNumber == request.InsuranceNumber && p.PatientId != patientId && !p.IsDeleted, ct);
                
                if (insuranceExists)
                    return OperationResult<PatientDetailDto>.Fail("Số bảo hiểm y tế này đã được sử dụng bởi bệnh nhân khác");
            }
        }

        // ✅ VALIDATION 5: Email phải unique nếu thay đổi
        if (request.Email != null && request.Email != entity.Email)
        {
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var emailExists = await _db.Patients
                    .AnyAsync(p => p.Email == request.Email && p.PatientId != patientId && !p.IsDeleted, ct);
                
                if (emailExists)
                    return OperationResult<PatientDetailDto>.Fail("Email này đã được sử dụng bởi bệnh nhân khác");
            }
        }

        // ✅ VALIDATION 6: Gender validation
        if (request.Gender.HasValue && request.Gender.Value > 3)
            return OperationResult<PatientDetailDto>.Fail("Giới tính không hợp lệ (0=Không xác định, 1=Nam, 2=Nữ, 3=Khác)");

        if (request.FullName != null) entity.FullName = request.FullName;
        if (request.DateOfBirth.HasValue) entity.DateOfBirth = request.DateOfBirth;
        if (request.Gender.HasValue) entity.Gender = request.Gender.Value;
        if (request.BloodType.HasValue) entity.BloodType = request.BloodType.Value; // Fixed: use .HasValue and .Value for nullable enum
        if (request.Phone != null) entity.Phone = request.Phone;
        if (request.Email != null) entity.Email = request.Email;
        if (request.Address != null) entity.Address = request.Address;
        if (request.CitizenId != null) entity.CitizenId = request.CitizenId;
        if (request.InsuranceNumber != null) entity.InsuranceNumber = request.InsuranceNumber;

        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = DateTime.Now;

        _db.AuditLogs.Add(new AuditLog
        {
            Entity = "Patient",
            EntityId = patientId,
            Action = "Update",
            OccurredAt = DateTime.UtcNow,
            UserId = actorUserId,
            DetailJson = JsonSerializer.Serialize(new { 
                entity.FullName, 
                entity.DateOfBirth, 
                entity.Gender, 
                entity.BloodType,
                entity.Phone,
                entity.Email 
            })
        });

        await _db.SaveChangesAsync(ct);

        var dto = new PatientDetailDto(
            entity.PatientId, entity.FullName, entity.DateOfBirth, entity.Gender, entity.BloodType,
            entity.Phone, entity.Email, entity.Address, entity.CitizenId, entity.InsuranceNumber,
            entity.UserId, entity.IsDeleted, entity.CreatedAt, entity.UpdatedAt);
        
        return OperationResult<PatientDetailDto>.Success(dto);
    }

    public async Task<OperationResult> DeleteAsync(
        Guid patientId, Guid actorUserId, string? reason = null, string? actorIp = null, CancellationToken ct = default)
    {
        var entity = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId && !p.IsDeleted, ct);
        if (entity == null) return OperationResult.Fail(ErrorCodes.NotFound);

        if (entity.UserId.HasValue && entity.UserId != actorUserId)
            return OperationResult.Fail(ErrorCodes.Forbidden);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedByUserId = actorUserId;
        entity.UpdatedAt = DateTime.Now;

        _db.AuditLogs.Add(new AuditLog
        {
            Entity = "Patient",
            EntityId = patientId,
            Action = "Delete",
            OccurredAt = DateTime.UtcNow,
            UserId = actorUserId,
            DetailJson = JsonSerializer.Serialize(new { reason })
        });

        await _db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult<PatientDetailDto>> GetAsync(Guid patientId, CancellationToken ct = default)
    {
        var e = await _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == patientId, ct);
        if (e == null) return OperationResult<PatientDetailDto>.Fail(ErrorCodes.NotFound);
        
        var dto = new PatientDetailDto(
            e.PatientId, e.FullName, e.DateOfBirth, e.Gender, e.BloodType,
            e.Phone, e.Email, e.Address, e.CitizenId, e.InsuranceNumber, // Đổi IdNumber → CitizenId
            e.UserId, e.IsDeleted, e.CreatedAt, e.UpdatedAt);
        
        return OperationResult<PatientDetailDto>.Success(dto);
    }

    public async Task<(IReadOnlyList<PatientSummaryDto> Items, long Total)> ListAsync(
        int page, int pageSize, string? name, DateOnly? dob, bool? isDeleted, 
        string? sortBy, string? sortDir, string? idLast4, string? phoneLast4, CancellationToken ct = default)
    {
        var q = _db.Patients.AsNoTracking().IgnoreQueryFilters();
        
        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(p => p.FullName != null && p.FullName.Contains(name));
        if (dob.HasValue) q = q.Where(p => p.DateOfBirth == dob);
        if (isDeleted.HasValue) q = q.Where(p => p.IsDeleted == isDeleted.Value);
        
        if (!string.IsNullOrWhiteSpace(phoneLast4))
            q = q.Where(p => p.Phone != null && p.Phone.Contains(phoneLast4));
        
        if (!string.IsNullOrWhiteSpace(idLast4)) 
            q = q.Where(p => p.CitizenId != null && p.CitizenId.EndsWith(idLast4)); // Đổi IdNumber → CitizenId

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
            .Select(e => new { e.PatientId, e.FullName, e.DateOfBirth, e.Gender, e.BloodType,e.Email, e.Phone, e.IsDeleted, e.CreatedAt, e.UpdatedAt })
            .ToListAsync(ct);

        // Trả về toàn bộ số điện thoại
        var items = data.Select(e => new PatientSummaryDto(
            e.PatientId, e.FullName, e.DateOfBirth, e.Gender, e.BloodType,e.Email,e.Phone, 
            e.IsDeleted, e.CreatedAt, e.UpdatedAt)).ToList();

        return (items, total);
    }

    public async Task<(IReadOnlyList<PatientSummaryDto> Items, long Total)> ListByOwnerAsync(
        Guid ownerUserId, int page, int pageSize, string? name, DateOnly? dob, 
        string? sortBy, string? sortDir, CancellationToken ct = default)
    {
        var q = _db.Patients.AsNoTracking().Where(p => p.UserId == ownerUserId);
        
        if (!string.IsNullOrWhiteSpace(name)) 
            q = q.Where(p => p.FullName != null && p.FullName.Contains(name));
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
            .Select(e => new PatientSummaryDto(
                e.PatientId, e.FullName, e.DateOfBirth, e.Gender, e.BloodType,e.Email, e.Phone, 
                e.IsDeleted, e.CreatedAt, e.UpdatedAt))
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
                BloodType = p.BloodType,
                Email = p.Email,
                Phone = p.Phone,
                Address = p.Address,
                CitizenId = p.CitizenId, // Đổi IdNumber → CitizenId
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
                p.BloodType,
                p.Email,
                p.Phone,
                p.IsDeleted,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToListAsync(ct);

        return patients;
    }

    public async Task<(IReadOnlyList<PatientSummaryDto> Items, long Total)> SearchPatientsAsync(
        int page, 
        int pageSize, 
        string? name, 
        string? phone, 
        string? email, 
        string? insuranceNumber, 
        string? citizenId, 
        string? sortBy, 
        string? sortDir, 
        CancellationToken ct = default)
    {
        var q = _db.Patients.AsNoTracking().Where(p => !p.IsDeleted);
        
        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(p => p.FullName != null && p.FullName.Contains(name));
        
        if (!string.IsNullOrWhiteSpace(phone))
            q = q.Where(p => p.Phone != null && p.Phone.Contains(phone));
        
        if (!string.IsNullOrWhiteSpace(email))
            q = q.Where(p => p.Email != null && p.Email.Contains(email));
        
        if (!string.IsNullOrWhiteSpace(insuranceNumber))
            q = q.Where(p => p.InsuranceNumber != null && p.InsuranceNumber.Contains(insuranceNumber));
        
        // Đổi IdNumber → CitizenId
        if (!string.IsNullOrWhiteSpace(citizenId))
            q = q.Where(p => p.CitizenId != null && p.CitizenId.Contains(citizenId));

        q = sortBy?.ToLowerInvariant() switch
        {
            "name" => (sortDir?.ToLowerInvariant() == "desc" ? q.OrderByDescending(x => x.FullName) : q.OrderBy(x => x.FullName)),
            "email" => (sortDir?.ToLowerInvariant() == "desc" ? q.OrderByDescending(x => x.Email) : q.OrderBy(x => x.Email)),
            "phone" => (sortDir?.ToLowerInvariant() == "desc" ? q.OrderByDescending(x => x.Phone) : q.OrderBy(x => x.Phone)),
            "createdat" => (sortDir?.ToLowerInvariant() == "asc" ? q.OrderBy(x => x.CreatedAt) : q.OrderByDescending(x => x.CreatedAt)),
            "updatedat" => (sortDir?.ToLowerInvariant() == "asc" ? q.OrderBy(x => x.UpdatedAt) : q.OrderByDescending(x => x.UpdatedAt)),
            _ => q.OrderByDescending(x => x.CreatedAt)
        };

        var total = await q.LongCountAsync(ct);
        var data = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new PatientSummaryDto(
                e.PatientId, 
                e.FullName, 
                e.DateOfBirth, 
                e.Gender, 
                e.BloodType, 
                e.Email,
                e.Phone, 
                e.IsDeleted, 
                e.CreatedAt, 
                e.UpdatedAt))
            .ToListAsync(ct);

        return (data, total);
    }
}
