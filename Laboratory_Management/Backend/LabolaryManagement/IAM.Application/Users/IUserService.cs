using Common.Pagination;
using Common.Results;
using IAM.Application.Users.DTOs;

namespace IAM.Application.Users
{
    public interface IUserService
    {
        Task<OperationResult<UserDetailDto>> CreateAsync(CreateUserRequest request, Guid actorId, CancellationToken ct = default);
        Task<OperationResult<UserDetailDto>> UpdateAsync(Guid id, UpdateUserRequest request, Guid actorId, CancellationToken ct = default);
        Task<OperationResult> DeleteAsync(Guid id, Guid actorId, CancellationToken ct = default);
        Task<OperationResult<UserDetailDto>> GetAsync(Guid id, Guid actorId, bool allowOther = false, CancellationToken ct = default);
        Task<PageResult<UserSummaryDto>> ListAsync(int page, int pageSize, string? search, string? role, string? type, string? status, string? sortBy, string? sort, CancellationToken ct = default);
        Task<OperationResult> AssignRolesAsync(Guid id, AssignRolesRequest request, Guid actorId, CancellationToken ct = default);
        Task<OperationResult> LockAsync(Guid id, Guid actorId, CancellationToken ct = default);
        Task<OperationResult> UnlockAsync(Guid id, Guid actorId, CancellationToken ct = default);
    }
}
