using Common.Errors;
using Common.Results;
using IAM.Application.Roles.DTOs.Requests;
using IAM.Application.Roles.DTOs.Responses;
using IAM.Domain.Entities;
using IAM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IAM.Application.Roles.Services;

public class RoleService : IRoleService
{
    private static readonly HashSet<string> SystemRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin", "Manager", "Staff", "Customer"
    };

    private readonly IamDbContext _db;
    public RoleService(IamDbContext db) => _db = db;

    public async Task<OperationResult<RoleDetailDto>> CreateAsync(CreateRoleRequest request, Guid actorId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return OperationResult<RoleDetailDto>.Fail(ErrorCodes.ValidationError);

        if (await _db.Roles.AnyAsync(r => r.Name == request.Name, ct))
            return OperationResult<RoleDetailDto>.Fail(ErrorCodes.Conflict);

        var role = new Role
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Roles.Add(role);

        _db.AuditLogs.Add(new AuditLog
        {
            Action = "CREATE_ROLE",
            UserId = actorId,
            Resource = $"Role:{role.Name}",
            Description = $"Create role {role.Name}",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return OperationResult<RoleDetailDto>.Success(new RoleDetailDto(role.RoleId, role.Name, role.Description, role.IsDefault, Array.Empty<string>()));
    }

    public async Task<OperationResult> DeleteAsync(int roleId, Guid actorId, CancellationToken ct = default)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId, ct);
        if (role == null) return OperationResult.Fail(ErrorCodes.NotFound);

        if (SystemRoles.Contains(role.Name))
            return OperationResult.Fail(ErrorCodes.Forbidden);

        var hasUsers = await _db.UserRoles.AnyAsync(ur => ur.RoleId == roleId, ct);
        if (hasUsers)
            return OperationResult.Fail(ErrorCodes.Conflict);

        _db.Roles.Remove(role);
        _db.AuditLogs.Add(new AuditLog
        {
            Action = "DELETE_ROLE",
            UserId = actorId,
            Resource = $"Role:{role.Name}",
            Description = $"Delete role {role.Name}",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        return OperationResult.Success();
    }

    public async Task<OperationResult> AssignPermissionsAsync(int roleId, AssignPermissionsRequest request, Guid actorId, CancellationToken ct = default)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId, ct);
        if (role == null) return OperationResult.Fail(ErrorCodes.NotFound);

        var permissions = await _db.Permissions.Where(p => request.PermissionIds.Contains(p.PermissionId)).ToListAsync(ct);
        var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(ct);
        _db.RolePermissions.RemoveRange(existing);
        foreach (var p in permissions)
            _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = p.PermissionId, GrantedAt = DateTime.UtcNow });

        _db.AuditLogs.Add(new AuditLog
        {
            Action = "ASSIGN_ROLE_PERMS",
            UserId = actorId,
            Resource = $"Role:{role.Name}",
            Description = $"Assign permissions: {string.Join(',', permissions.Select(x=>x.PermissionId))}",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        return OperationResult.Success();
    }
}
