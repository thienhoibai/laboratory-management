using Common.Errors;
using Common.Results;
using IAM.Application.Auth.DTOs.Requests;
using IAM.Application.Auth.DTOs.Responses;
using IAM.Application.Users.DTOs.Responses;
using IAM.Application.Security;
using IAM.Domain.Entities;
using IAM.Infrastructure;
using Messaging.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Security.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace IAM.Application.Auth.Services;

public partial class AuthService : IAuthService
{
    private readonly IamDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly IPasswordService _passwords;
    private readonly IPasswordPolicy _policy;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;
    private readonly INotificationPublisher _publisher;

    private const int MaxFailedAccess = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private const int PasswordHistoryLimit = 5;
    private static readonly TimeSpan ResetTokenTtl = TimeSpan.FromMinutes(15);
    private const int MaxResetAttempts = 5;
    private const int MaxResetRequestsPerHour = 5;

    public AuthService(
        IamDbContext db,
        IJwtTokenService jwt,
        IPasswordService passwords,
        IPasswordPolicy policy,
        IConfiguration config,
        ILogger<AuthService> logger,
        INotificationPublisher publisher)
    {
        _db = db;
        _jwt = jwt;
        _passwords = passwords;
        _policy = policy;
        _config = config;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<OperationResult<UserDetailDto>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (request.Password != request.ConfirmPassword)
            return OperationResult<UserDetailDto>.Fail(ErrorCodes.ValidationError);

        // Check duplicates with clear codes
        if (await _db.Users.AnyAsync(u => u.Username == request.Username, ct))
            return OperationResult<UserDetailDto>.Fail(ErrorCodes.DuplicateUsername);
        if (await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
            return OperationResult<UserDetailDto>.Fail(ErrorCodes.DuplicateEmail);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwords.Hash(request.Password),
            FullName = request.FullName,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.Users.Add(user);

        // Assign Customer role by default if present; otherwise fallback to roles with IsDefault = 1
        var customerRoleId = await _db.Roles.Where(r => r.Name == "Customer").Select(r => r.RoleId).FirstOrDefaultAsync(ct);
        if (customerRoleId != 0)
        {
            _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRoleId, AssignedAt = DateTime.Now });
        }
        else
        {
            var defaultRoleIds = await _db.Roles.Where(r => r.IsDefault).Select(r => r.RoleId).ToListAsync(ct);
            foreach (var rid in defaultRoleIds)
                _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = rid, AssignedAt = DateTime.Now });
        }

        _db.AuditLogs.Add(new AuditLog
        {
            Action = "REGISTER",
            UserId = user.UserId,
            Resource = $"User:{user.UserId}",
            Description = "Self register",
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync(ct);

        var roles = await _db.UserRoles.Where(ur => ur.UserId == user.UserId).Join(_db.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => r.Name).ToArrayAsync(ct);
        var dto = new UserDetailDto(user.UserId, user.Username, user.Email, user.FullName, user.IsActive, user.LastLoginAt, user.CreatedAt, user.UpdatedAt, roles);
        return OperationResult<UserDetailDto>.Success(dto);
    }

    public async Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, ct);
        if (user == null)
            return OperationResult<LoginResponse>.Fail(ErrorCodes.InvalidCredentials);

        // ✅ Check manual lock first (IsLocked field)
        if (user.IsLocked)
        {
            return OperationResult<LoginResponse>.Fail(ErrorCodes.AccountLocked);
        }

        // check auto lockout from failed attempts
        var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == user.UserId, ct);
        if (sec != null && sec.LockoutEnd.HasValue && sec.LockoutEnd.Value > DateTime.Now)
        {
            return OperationResult<LoginResponse>.Fail(ErrorCodes.AccountLocked);
        }

        if (!_passwords.Verify(request.Password, user.PasswordHash))
        {
            if (sec == null)
            {
                sec = new UserSecurity { UserId = user.UserId, FailedAccessCount = 1 };
                _db.UserSecurities.Add(sec);
            }
            else
            {
                sec.FailedAccessCount++;
            }

            if (sec.FailedAccessCount >= MaxFailedAccess)
            {
                sec.FailedAccessCount = 0;
                sec.LockoutEnd = DateTime.Now.Add(LockoutDuration);
                user.IsLocked = true;  // ✅ Also set IsLocked when auto-locked
                _db.AuditLogs.Add(new AuditLog
                {
                    Action = "LOCK_USER_AUTO",
                    UserId = user.UserId,
                    Resource = $"User:{user.UserId}",
                    Description = $"Auto lock after {MaxFailedAccess} failed attempts",
                    CreatedAt = DateTime.Now
                });
            }

            await _db.SaveChangesAsync(ct);
            return OperationResult<LoginResponse>.Fail(ErrorCodes.InvalidCredentials);
        }

        // success: reset counters
        if (sec != null)
        {
            sec.FailedAccessCount = 0;
            sec.LockoutEnd = null;
        }

        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == user.UserId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);

        var roles = await _db.Roles
            .Where(r => roleIds.Contains(r.RoleId))
            .Select(r => r.Name)
            .ToListAsync(ct);

        var permissionIds = await _db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync(ct);

        // Permission.Name chứa code (e.g., "Patient.View", "User.Create")
        var permissions = await _db.Permissions
            .Where(p => permissionIds.Contains(p.PermissionId))
            .Select(p => p.Name)
            .ToListAsync(ct);

        var (access, expires, refresh) = _jwt.IssueTokens(user.UserId, user.Username, roles, permissions);

        var refreshHash = _jwt.HashRefreshToken(refresh);
        _db.RefreshTokens.Add(new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = refreshHash,
            IssuedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddDays(7),
            Revoked = false
        });

        user.LastLoginAt = DateTime.Now;
        _db.AuditLogs.Add(new AuditLog
        {
            Action = "LOGIN",
            UserId = user.UserId,
            Resource = $"User:{user.UserId}",
            Description = "User login",
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync(ct);

        return OperationResult<LoginResponse>.Success(new LoginResponse(access, expires, refresh));
    }

    public async Task<OperationResult<RefreshResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var refreshHash = _jwt.HashRefreshToken(request.RefreshToken);
        var token = await _db.RefreshTokens.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == refreshHash, ct);
        if (token == null || token.Revoked || token.ExpiresAt <= DateTime.Now || token.User == null || !token.User.IsActive)
            return OperationResult<RefreshResponse>.Fail(ErrorCodes.InvalidRefreshToken);

        var roleIds = await _db.UserRoles
            .Where(ur => ur.UserId == token.UserId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);

        var roles = await _db.Roles
            .Where(r => roleIds.Contains(r.RoleId))
            .Select(r => r.Name)
            .ToListAsync(ct);

        var permissionIds = await _db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync(ct);

        // Permission.Name chứa code (e.g., "Patient.View", "User.Create")
        var permissions = await _db.Permissions
            .Where(p => permissionIds.Contains(p.PermissionId))
            .Select(p => p.Name)
            .ToListAsync(ct);

        var (access, expires, newRefresh) = _jwt.IssueTokens(token.UserId, token.User!.Username, roles, permissions);

        // rotate: revoke old and add new
        token.Revoked = true;
        _db.RefreshTokens.Add(new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = token.UserId,
            TokenHash = _jwt.HashRefreshToken(newRefresh),
            IssuedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddDays(7),
            Revoked = false
        });

        _db.AuditLogs.Add(new AuditLog
        {
            Action = "REFRESH_TOKEN",
            UserId = token.UserId,
            Resource = $"User:{token.UserId}",
            Description = "Token refreshed",
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync(ct);

        return OperationResult<RefreshResponse>.Success(new RefreshResponse(access, expires, newRefresh));
    }

    public async Task<OperationResult> LogoutAsync(LogoutRequest request, CancellationToken ct = default)
    {
        var refreshHash = _jwt.HashRefreshToken(request.RefreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshHash, ct);
        if (token == null)
            return OperationResult.Success();

        token.Revoked = true;
        _db.AuditLogs.Add(new AuditLog
        {
            Action = "LOGOUT",
            UserId = token.UserId,
            Resource = $"User:{token.UserId}",
            Description = "User logout",
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync(ct);
        return OperationResult.Success();
    }

    public async Task<OperationResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive, ct);
        if (user == null) return OperationResult.Fail(ErrorCodes.NotFound);

        if (!_passwords.Verify(request.CurrentPassword, user.PasswordHash))
            return OperationResult.Fail(ErrorCodes.InvalidCredentials);

        if (!_policy.Validate(request.NewPassword, out _))
            return OperationResult.Fail(ErrorCodes.ValidationError);

        var histories = await _db.PasswordHistories
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(PasswordHistoryLimit)
            .ToListAsync(ct);
        if (histories.Any(h => _passwords.Verify(request.NewPassword, h.PasswordHash)))
            return OperationResult.Fail(ErrorCodes.PasswordReused);

        _db.PasswordHistories.Add(new PasswordHistory
        {
            PasswordHistoryId = Guid.NewGuid(),
            UserId = userId,
            PasswordHash = user.PasswordHash,
            CreatedAt = DateTime.Now
        });

        user.PasswordHash = _passwords.Hash(request.NewPassword);

        _db.AuditLogs.Add(new AuditLog
        {
            Action = "CHANGE_PASSWORD",
            UserId = userId,
            Resource = $"User:{userId}",
            Description = "User changed password",
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync(ct);
        return OperationResult.Success();
    }

    public async Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        try
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail, ct);
            if (user == null || !user.IsActive)
            {
                return OperationResult.Success();
            }

            var oneHourAgo = DateTime.Now.AddHours(-1);
            var recent = await _db.AuditLogs.CountAsync(a => a.UserId == user.UserId && a.Action == "FORGOT_PASSWORD" && a.CreatedAt >= oneHourAgo, ct);
            if (recent >= MaxResetRequestsPerHour)
            {
                return OperationResult.Fail(ErrorCodes.RateLimited);
            }

            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(tokenBytes).TrimEnd('=')
                .Replace('+', '-').Replace('/', '_');
            using var sha = SHA256.Create();
            var tokenHash = sha.ComputeHash(Encoding.UTF8.GetBytes(token));

            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                PasswordResetTokenId = Guid.NewGuid(),
                UserId = user.UserId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.Now.Add(ResetTokenTtl),
                Attempts = 0,
                CreatedAt = DateTime.Now
            });

            _db.AuditLogs.Add(new AuditLog
            {
                Action = "FORGOT_PASSWORD",
                UserId = user.UserId,
                Resource = $"User:{user.UserId}",
                Description = "Requested password reset",
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync(ct);

            var baseUrl = _config["Email:ResetPasswordBaseUrl"] ?? "http://localhost:5274/reset-password";
            var link = $"{baseUrl}?token={token}";

            await _publisher.PublishAsync("PasswordResetRequested", new { to = user.Email, Username = user.Username, Link = link, ExpireMinutes = ((int)ResetTokenTtl.TotalMinutes).ToString() }, ct);

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ForgotPassword failed for {UsernameOrEmail}", request.UsernameOrEmail);
            return OperationResult.Success();
        }
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token)) return OperationResult.Fail(ErrorCodes.InvalidResetToken);

        using var sha = SHA256.Create();
        var tokenHash = sha.ComputeHash(Encoding.UTF8.GetBytes(request.Token));

        var prt = await _db.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
        if (prt == null) return OperationResult.Fail(ErrorCodes.InvalidResetToken);

        if (prt.UsedAt.HasValue) return OperationResult.Fail(ErrorCodes.ResetTokenUsed);
        if (prt.ExpiresAt <= DateTime.Now) return OperationResult.Fail(ErrorCodes.ResetTokenExpired);
        if (prt.Attempts >= MaxResetAttempts) return OperationResult.Fail(ErrorCodes.RateLimited);

        prt.Attempts++;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == prt.UserId, ct);
        if (user == null) return OperationResult.Fail(ErrorCodes.NotFound);

        if (!_policy.Validate(request.NewPassword, out _))
            return OperationResult.Fail(ErrorCodes.ValidationError);

        var histories = await _db.PasswordHistories.Where(h => h.UserId == user.UserId)
            .OrderByDescending(h => h.CreatedAt).Take(PasswordHistoryLimit).ToListAsync(ct);
        if (histories.Any(h => _passwords.Verify(request.NewPassword, h.PasswordHash)))
            return OperationResult.Fail(ErrorCodes.PasswordReused);

        _db.PasswordHistories.Add(new PasswordHistory
        {
            PasswordHistoryId = Guid.NewGuid(),
            UserId = user.UserId,
            PasswordHash = user.PasswordHash,
            CreatedAt = DateTime.Now
        });

        user.PasswordHash = _passwords.Hash(request.NewPassword);
        prt.UsedAt = DateTime.Now;

        _db.AuditLogs.Add(new AuditLog
        {
            Action = "RESET_PASSWORD",
            UserId = user.UserId,
            Resource = $"User:{user.UserId}",
            Description = "Password reset via email token",
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync(ct);
        return OperationResult.Success();
    }
}
