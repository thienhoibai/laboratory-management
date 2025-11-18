using Common.Errors;
using Common.Results;
using Google.Apis.Auth;
using IAM.Application.Auth.DTOs;
using IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Security.Jwt;

namespace IAM.Application.Auth
{
    public partial class AuthService
    {
        public async Task<OperationResult<LoginResponse>> LoginWithGoogleAsync(GoogleLoginRequest request, string? ip = null, string? userAgent = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
                return OperationResult<LoginResponse>.Fail(ErrorCodes.InvalidGoogleToken);

            var clientId = _config["Auth:Google:ClientId"] ?? string.Empty;
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            }
            catch
            {
                return OperationResult<LoginResponse>.Fail(ErrorCodes.InvalidGoogleToken);
            }

            if (payload == null || string.IsNullOrEmpty(payload.Email) || payload.EmailVerified != true)
                return OperationResult<LoginResponse>.Fail(ErrorCodes.InvalidGoogleToken);

            var email = payload.Email;
            var name = payload.Name;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            if (user != null)
            {
                if (user.AuthProvider.Equals("Password", StringComparison.OrdinalIgnoreCase))
                {
                    return OperationResult<LoginResponse>.Fail(ErrorCodes.AccountLinkRequired);
                }

                if (!user.IsActive)
                    return OperationResult<LoginResponse>.Fail(ErrorCodes.Forbidden);
            }
            else
            {
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = email,
                    Email = email,
                    PasswordHash = string.Empty,
                    FullName = name,
                    AuthProvider = "Google",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    EmailVerifiedAt = DateTime.Now
                };
                _db.Users.Add(user);

                var customerRoleId = await _db.Roles.Where(r => r.Name == "Customer").Select(r => r.RoleId).FirstOrDefaultAsync(ct);
                if (customerRoleId != 0)
                {
                    _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = customerRoleId, AssignedAt = DateTime.Now });
                }
            }

            var roleIds = await _db.UserRoles.Where(ur => ur.UserId == user.UserId).Select(ur => ur.RoleId).ToListAsync(ct);
            var roles = await _db.Roles.Where(r => roleIds.Contains(r.RoleId)).Select(r => r.Name).ToListAsync(ct);
            var permissionIds = await _db.RolePermissions.Where(rp => roleIds.Contains(rp.RoleId)).Select(rp => rp.PermissionId).Distinct().ToListAsync(ct);
            var permissions = await _db.Permissions.Where(p => permissionIds.Contains(p.PermissionId)).Select(p => p.Name).ToListAsync(ct);

            var (access, expires, refresh) = _jwt.IssueTokens(user.UserId, user.Username, roles, permissions);
            var refreshHash = _jwt.HashRefreshToken(refresh);
            _db.RefreshTokens.Add(new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = user.UserId,
                TokenHash = refreshHash,
                IssuedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(7),
            });

            _db.AuditLogs.Add(new AuditLog
            {
                Action = "LOGIN_GOOGLE",
                UserId = user.UserId,
                Resource = $"User:{user.UserId}",
                Description = $"Login via Google: {email}",
                ActorIp = ip,
                CreatedAt = DateTime.Now
            });

            await _db.SaveChangesAsync(ct);

            return OperationResult<LoginResponse>.Success(new LoginResponse(access, expires, refresh));
        }
    }
}
