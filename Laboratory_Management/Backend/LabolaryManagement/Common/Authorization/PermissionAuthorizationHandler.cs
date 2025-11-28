using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Authorization;

/// <summary>
/// Handler để validate permission từ JWT token claims
/// Logic:
/// 1. Admin có tất cả quyền (bypass)
/// 2. Kiểm tra claim "perm" (đơn lẻ)
/// 3. Kiểm tra claim "permissions" (comma-separated)
/// 4. Kiểm tra claim "scope" (OAuth2/OIDC)
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        // ✅ CHECK 1: User có role "Admin"? → Bypass tất cả permission
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // ✅ CHECK 2: Kiểm tra claim "perm" với giá trị chính xác
        // JWT payload: { "perm": "Patient.Delete" }
        if (context.User.HasClaim("perm", requirement.PermissionName))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // ✅ CHECK 3: Kiểm tra claim "permissions" (dạng comma-separated)
        // JWT payload: { "permissions": "Patient.Delete,Patient.Update,Instrument.View" }
        var permissionsClaim = context.User.FindFirst("permissions")?.Value;
        if (!string.IsNullOrEmpty(permissionsClaim))
        {
            var permissions = permissionsClaim
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());
            
            if (permissions.Contains(requirement.PermissionName))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // ✅ CHECK 4: Kiểm tra claim "scope" (dùng trong OAuth2/OIDC)
        // JWT payload: { "scope": "Patient.Delete" }
        if (context.User.HasClaim("scope", requirement.PermissionName))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // ❌ Không có quyền → Trả về mà không gọi context.Succeed()
        // ASP.NET Core sẽ tự động trả về 403 Forbidden
        return Task.CompletedTask;
    }
}
