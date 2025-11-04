using Common.Errors;
using Common.Pagination;
using Common.Results;
using IAM.Application.Auth; // add for IPasswordService
using IAM.Application.Security;
using IAM.Application.Users.DTOs;
using IAM.Domain.Entities;
using IAM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace IAM.Application.Users
{
    public class UserService : IUserService
    {
        private readonly IamDbContext _db;
        private readonly IPasswordService _passwords;
        private readonly IPasswordPolicy _policy;
        private readonly Messaging.Notifications.INotificationPublisher _publisher; // use messaging publisher

        public UserService(IamDbContext db, IPasswordService passwords, IPasswordPolicy policy, Messaging.Notifications.INotificationPublisher publisher)
        {
            _db = db;
            _passwords = passwords;
            _policy = policy;
            _publisher = publisher;
        }

        public async Task<OperationResult<UserDetailDto>> CreateAsync(CreateUserRequest request, Guid actorId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || request.RoleId <= 0)
                return OperationResult<UserDetailDto>.Fail(ErrorCodes.ValidationError);

            if (await _db.Users.AnyAsync(u => u.Username == request.Username, ct))
                return OperationResult<UserDetailDto>.Fail(ErrorCodes.DuplicateUsername);
            if (await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
                return OperationResult<UserDetailDto>.Fail(ErrorCodes.DuplicateEmail);

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId, ct);
            if (role == null) return OperationResult<UserDetailDto>.Fail(ErrorCodes.ValidationError);

            // Random strong password
            var plainPassword = GenerateStrongPassword();
            if (!_policy.Validate(plainPassword, out _))
                return OperationResult<UserDetailDto>.Fail(ErrorCodes.ValidationError);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                FullName = null,
                PasswordHash = _passwords.Hash(plainPassword),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AuthProvider = "Password"
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

            // Send email with credentials (best-effort)
            try
            {
                await _publisher.PublishAsync("InviteUser", new
                {
                    to = user.Email,
                    Username = user.Username,
                    Password = plainPassword
                }, ct);
            }
            catch (Exception ex)
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    Action = "INVITE_USER_EMAIL_FAILED",
                    UserId = actorId,
                    Resource = $"User:{user.UserId}",
                    Description = ex.Message,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync(ct);
            }

            var roles = new[] { role.Name };
            return OperationResult<UserDetailDto>.Success(new UserDetailDto(user.UserId, user.Username, user.Email, user.FullName, user.IsActive, user.LastLoginAt, user.CreatedAt, user.UpdatedAt, roles));
        }

        private static string GenerateStrongPassword()
        {
            // 12 chars: upper, lower, digits, symbols
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digit = "23456789";
            const string symbol = "!@#$%^&*()-_=+[]{}";
            using var rnd = RandomNumberGenerator.Create();

            static string Pick(string chars, int n, RandomNumberGenerator rnd)
            {
                var bytes = new byte[n];
                rnd.GetBytes(bytes);
                var result = new char[n];
                for (int i = 0; i < n; i++) result[i] = chars[bytes[i] % chars.Length];
                return new string(result);
            }

            var parts = new[]
            {
                Pick(upper, 3, rnd),
                Pick(lower, 5, rnd),
                Pick(digit, 2, rnd),
                Pick(symbol, 2, rnd)
            };
            return string.Concat(parts.OrderBy(_ => Guid.NewGuid()));
        }

        public async Task<OperationResult<UserDetailDto>> UpdateAsync(Guid id, UpdateUserRequest request, Guid actorId, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id, ct);
            if (user == null) return OperationResult<UserDetailDto>.Fail(ErrorCodes.NotFound);

            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                if (await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
                    return OperationResult<UserDetailDto>.Fail(ErrorCodes.DuplicateEmail);
                user.Email = request.Email!;
            }
            if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName;
            // Phone not stored in User entity in current schema; ignore

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
                "email" => (sort?.ToLowerInvariant() == "desc" ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email)),
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
            // Lock until far future to represent locked status
            sec.LockoutEnd = DateTime.UtcNow.AddYears(100);
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
    }
}
