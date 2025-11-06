using Common.Results;
using Patient.Application.DTOs;

namespace Patient.Application.Services;

public interface IPatientService
{
    Task<OperationResult<PatientDetailDto>> CreateAsync(CreatePatientRequest request, Guid actorUserId, string? actorIp = null, CancellationToken ct = default);
    Task<OperationResult<PatientDetailDto>> UpdateAsync(Guid patientId, UpdatePatientRequest request, Guid actorUserId, string? actorIp = null, CancellationToken ct = default);
    Task<OperationResult> DeleteAsync(Guid patientId, Guid actorUserId, string? reason = null, string? actorIp = null, CancellationToken ct = default);
    Task<OperationResult<PatientDetailDto>> GetAsync(Guid patientId, CancellationToken ct = default);
    Task<(IReadOnlyList<PatientSummaryDto> Items, long Total)> ListAsync(int page, int pageSize, string? name, DateOnly? dob, bool? isDeleted, string? sortBy, string? sortDir, string? idLast4, string? phoneLast4, CancellationToken ct = default);
    Task<(IReadOnlyList<PatientVersionDto> Items, long Total)> GetVersionsAsync(Guid patientId, int page, int pageSize, string? sortDir, CancellationToken ct = default);
    Task<bool> IsOwnerAsync(Guid patientId, Guid actorUserId, CancellationToken ct);
    Task<(IReadOnlyList<PatientSummaryDto> Items, long Total)> ListByOwnerAsync(Guid ownerUserId, int page, int pageSize, string? name, DateOnly? dob, string? sortBy, string? sortDir, CancellationToken ct = default);

    Task<OperationResult<PatientDto>> GetByUserIdAsync(Guid userId, CancellationToken ct);

    // Guest linking flows
    Task<OperationResult> StartLinkAsync(Guid patientId, Guid actorUserId, string mode, string baseLinkUrl, CancellationToken ct = default);
    Task<OperationResult> ConfirmMagicLinkAsync(string token, Guid actorUserId, CancellationToken ct = default);
    Task<OperationResult> RequestOtpAsync(Guid patientId, Guid actorUserId, CancellationToken ct = default);
    Task<OperationResult> VerifyOtpAsync(Guid patientId, string code, Guid actorUserId, CancellationToken ct = default);
}
