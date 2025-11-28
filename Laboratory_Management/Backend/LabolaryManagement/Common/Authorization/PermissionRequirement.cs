using Microsoft.AspNetCore.Authorization;

namespace Common.Authorization;

/// <summary>
/// Requirement để kiểm tra permission động từ JWT claims
/// Sử dụng chung cho tất cả services trong solution
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Tên permission cần kiểm tra (ví dụ: "Patient.Delete", "Instrument.Update")
    /// </summary>
    public string PermissionName { get; }

    public PermissionRequirement(string permissionName)
    {
        PermissionName = permissionName ?? throw new ArgumentNullException(nameof(permissionName));
    }
}
