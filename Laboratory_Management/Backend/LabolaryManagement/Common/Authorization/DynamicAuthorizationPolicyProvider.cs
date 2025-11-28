using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Common.Authorization;

/// <summary>
/// Policy Provider động - Tự động tạo policy cho bất kỳ permission nào
/// Khi controller yêu cầu policy "perm:XXX", provider này sẽ tự động tạo policy với PermissionRequirement("XXX")
/// 
/// Lợi ích:
/// - Không cần đăng ký trước permission trong Program.cs
/// - Thêm permission mới trong database → Tự động hoạt động
/// - Áp dụng được cho tất cả services trong solution
/// </summary>
public class DynamicAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private const string POLICY_PREFIX = "perm:";

    public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        // Fallback provider để xử lý các policy khác (không phải "perm:XXX")
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    /// <summary>
    /// Trả về default policy (yêu cầu authenticated user)
    /// </summary>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    /// <summary>
    /// Trả về fallback policy khi không tìm thấy policy cụ thể
    /// </summary>
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    /// <summary>
    /// Xử lý logic chính: Tạo policy động cho "perm:XXX"
    /// </summary>
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // ✅ Nếu policy bắt đầu bằng "perm:" → Tạo policy động
        if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            // Lấy tên permission (ví dụ: "perm:Patient.Delete" → "Patient.Delete")
            var permissionName = policyName.Substring(POLICY_PREFIX.Length);
            
            // Tạo policy với PermissionRequirement
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permissionName))
                .Build();
            
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // ✅ Nếu không phải "perm:" → Dùng default policy provider
        // (Ví dụ: policy "Admin", "User", v.v.)
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
