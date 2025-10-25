using Common.Errors;
using Common.Pagination;
using Common.Results;
using IAM.Application.Auth;
using IAM.Application.Security;
using IAM.Application.Users.DTOs;
using IAM.Domain.Entities;
using IAM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IAM.Application.Users
{
    public class UserService : IUserService
    {
        private readonly IamDbContext _db;
        private readonly IPasswordService _passwords;
        private readonly IPasswordPolicy _policy;

        public UserService(IamDbContext db, IPasswordService passwords, IPasswordPolicy policy)
        {
            _db = db;
            _passwords = passwords;
            _policy = policy;
        }

        public async Task<OperationResult<UserDetailDto>> CreateAsync(CreateUserRequest request, Guid actorId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || request.RoleId <= 0)
                return OperationResult<UserDetailDto>.Fail(ErrorCodes.ValidationError);

            if (!_policy.Validate(request.Password, out _))
                return OperationResult<UserDetailDto>.Fail(ErrorCodes.ValidationError);

            if (await _db.Users.AnyAsync(u => u.Username == request.Username, ct))
                return OperationResult<UserDetailDto>.Fail(ErrorCodes.Conflict);

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId, ct);
            if (role == null) return OperationResult<UserDetailDto>.Fail(ErrorCodes.ValidationError);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = request.Username,
                Email = $"{request.Username}@local",
                FullName = null,
                PasswordHash = _passwords.Hash(request.Password),
                // Admin-created accounts are active immediately for all roles per policy
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _db.Users.Add(user);
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = role.RoleId, AssignedAt = DateTime.UtcNow });

            _db.AuditLogs.Add(new AuditLog
            {
                Action = "CREATE_USER",
                UserId = actorId,
                Resource = $"User:{user.UserId}",
                Description = $"Created user {user.Username} with role #{role.RoleId}",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);

            var roles = new[] { role.Name };
            return OperationResult<UserDetailDto>.Success(new UserDetailDto(user.UserId, user.Username, user.Email, user.FullName, user.IsActive, user.LastLoginAt, user.CreatedAt, user.UpdatedAt, roles));
        }

        public async Task<OperationResult<UserDetailDto>> UpdateAsync(Guid id, UpdateUserRequest request, Guid actorId, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id, ct);
            if (user == null) return OperationResult<UserDetailDto>.Fail(ErrorCodes.NotFound);

            user.Email = request.Email ?? user.Email;
            user.FullName = request.FullName ?? user.FullName;
            user.UpdatedAt = DateTime.UtcNow;

            _db.AuditLogs.Add(new AuditLog
            {
                Action = "UPDATE_USER",
                UserId = actorId,
                Resource = $"User:{user.UserId}",
                Description = "Update user profile",
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
            var roles = await _db.UserRoles.Where(ur => ur.UserId == user.UserId).Join(_db.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => r.Name).ToArrayAsync(ct);
            return OperationResult<UserDetailDto>.Success(new UserDetailDto(user.UserId, user.Username, user.Email, user.FullName, user.IsActive, user.LastLoginAt, user.CreatedAt, user.UpdatedAt, roles));
        }

        public async Task<OperationResult> DeleteAsync(Guid id, Guid actorId, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id, ct);
            if (user == null) return OperationResult.Fail(ErrorCodes.NotFound);

            _db.Users.Remove(user);
            _db.AuditLogs.Add(new AuditLog
            {
                Action = "DELETE_USER",
                UserId = actorId,
                Resource = $"User:{id}",
                Description = "Delete user",
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
            return OperationResult.Success();
        }

        public async Task<OperationResult<UserDetailDto>> GetAsync(Guid id, Guid actorId, bool allowOther = false, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id, ct);
            if (user == null) return OperationResult<UserDetailDto>.Fail(ErrorCodes.NotFound);

            var roles = await _db.UserRoles.Where(ur => ur.UserId == user.UserId).Join(_db.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => r.Name).ToArrayAsync(ct);
            return OperationResult<UserDetailDto>.Success(new UserDetailDto(user.UserId, user.Username, user.Email, user.FullName, user.IsActive, user.LastLoginAt, user.CreatedAt, user.UpdatedAt, roles));
        }

        public async Task<PageResult<UserSummaryDto>> ListAsync(int page, int pageSize, string? search, string? role, string? type, string? status, string? sortBy, string? sort, CancellationToken ct = default)
        {
            var query = _db.Users.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(u => u.Username.Contains(s) || u.Email.Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = from u in query
                        join ur in _db.UserRoles on u.UserId equals ur.UserId
                        join r in _db.Roles on ur.RoleId equals r.RoleId
                        where r.Name == role
                        select u;
            }

            query = sortBy?.ToLowerInvariant() switch
            {
                "createdat" => (sort?.ToLowerInvariant() == "asc" ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt)),
                "username" => (sort?.ToLowerInvariant() == "desc" ? query.OrderByDescending(x => x.Username) : query.OrderBy(x => x.Username)),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var total = await query.LongCountAsync(ct);

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.FullName,
                    u.IsActive,
                    u.CreatedAt,
                    u.LastLoginAt,
                    Roles = (from ur in _db.UserRoles
                             join r in _db.Roles on ur.RoleId equals r.RoleId
                             where ur.UserId == u.UserId
                             select r.Name).ToArray()
                })
                .ToListAsync(ct);

            var mapped = items.Select(i => new UserSummaryDto(i.UserId, i.Username, i.Email, i.FullName, i.IsActive, i.CreatedAt, i.LastLoginAt, i.Roles)).ToList();
            return PageResult<UserSummaryDto>.From(mapped, page, pageSize, total);
        }

        public async Task<OperationResult> AssignRolesAsync(Guid id, AssignRolesRequest request, Guid actorId, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id, ct);
            if (user == null) return OperationResult.Fail(ErrorCodes.NotFound);

            var roles = await _db.Roles.Where(r => request.RoleIds.Contains(r.RoleId)).ToListAsync(ct);
            var existing = await _db.UserRoles.Where(ur => ur.UserId == id).ToListAsync(ct);
            _db.UserRoles.RemoveRange(existing);
            foreach (var r in roles)
                _db.UserRoles.Add(new UserRole { UserId = id, RoleId = r.RoleId, AssignedAt = DateTime.UtcNow });

            _db.AuditLogs.Add(new AuditLog { Action = "ASSIGN_ROLES", UserId = actorId, Resource = $"User:{id}", Description = $"Assign roles: {string.Join(',', roles.Select(x=>x.RoleId))}", CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync(ct);
            return OperationResult.Success();
        }

        public async Task<OperationResult> LockAsync(Guid id, Guid actorId, CancellationToken ct = default)
        {
            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == id, ct);
            if (sec == null)
            {
                sec = new UserSecurity { UserId = id, FailedAccessCount = 0 };
                _db.UserSecurities.Add(sec);
            }
            sec.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            _db.AuditLogs.Add(new AuditLog { Action = "LOCK_USER", UserId = actorId, Resource = $"User:{id}", Description = "Lock user", CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync(ct);
            return OperationResult.Success();
        }

        public async Task<OperationResult> UnlockAsync(Guid id, Guid actorId, CancellationToken ct = default)
        {
            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == id, ct);
            if (sec == null) return OperationResult.Fail(ErrorCodes.NotFound);
            sec.LockoutEnd = null;
            sec.FailedAccessCount = 0;
            _db.AuditLogs.Add(new AuditLog { Action = "UNLOCK_USER", UserId = actorId, Resource = $"User:{id}", Description = "Unlock user", CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync(ct);
            return OperationResult.Success();
        }

        public async Task<OperationResult> VerifyAsync(Guid id, VerifyUserRequest request, Guid actorId, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id, ct);
            if (user == null) return OperationResult.Fail(ErrorCodes.NotFound);

            user.IsActive = request.Approved;

            _db.AuditLogs.Add(new AuditLog { Action = "VERIFY_USER", UserId = actorId, Resource = $"User:{id}", Description = request.Approved ? "Approved" : "Rejected", CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync(ct);
            return OperationResult.Success();
        }

        public Task<OperationResult> LinkPatientAsync(Guid id, LinkPatientRequest request, Guid actorId, CancellationToken ct = default)
        {
            _db.AuditLogs.Add(new AuditLog { Action = "LINK_PATIENT", UserId = actorId, Resource = $"User:{id}", Description = $"Link patient {request.PatientId}", CreatedAt = DateTime.UtcNow });
            return _db.SaveChangesAsync(ct).ContinueWith(_ => OperationResult.Success(), ct);
        }
    }
}
