using IAM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IAM.Application.Roles.Services;

public class RolePermissionService : IRolePermissionService
{
    private readonly IamDbContext _db;
    public RolePermissionService(IamDbContext db) => _db = db;

    public async Task<List<string>> GetPermissionKeysByRoleAsync(int roleId, CancellationToken ct = default)
    {
        var keys = await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Join(_db.Permissions, rp => rp.PermissionId, p => p.PermissionId, (rp, p) => p.Name)
            .OrderBy(n => n)
            .ToListAsync(ct);
        return keys;
    }

    public async Task UpdateRolePermissionsByKeysAsync(int roleId, IEnumerable<string> keys, Guid actorId, CancellationToken ct = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId, ct);
        if (role == null) throw new InvalidOperationException("Role not found");

        var keySet = new HashSet<string>(keys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var allMap = await _db.Permissions.AsNoTracking().ToDictionaryAsync(p => p.Name, p => p.PermissionId, StringComparer.OrdinalIgnoreCase, ct);

        // validate all keys exist
        foreach (var k in keySet)
            if (!allMap.ContainsKey(k))
                throw new InvalidOperationException($"Permission key not found: {k}");

        var current = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(ct);
        var currentIds = current.Select(c => c.PermissionId).ToHashSet();
        var targetIds = keySet.Select(k => allMap[k]).ToHashSet();

        var toRemove = current.Where(c => !targetIds.Contains(c.PermissionId)).ToList();
        var toAddIds = targetIds.Except(currentIds).ToList();

        if (toRemove.Count > 0) _db.RolePermissions.RemoveRange(toRemove);
        foreach (var pid in toAddIds)
            _db.RolePermissions.Add(new Domain.Entities.RolePermission { RoleId = roleId, PermissionId = pid, GrantedAt = DateTime.UtcNow });

        role.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            Action = "ASSIGN_ROLE_PERMS",
            UserId = actorId,
            Resource = $"Role:{role.Name}",
            Description = $"Set permissions: {string.Join(',', keySet.OrderBy(x=>x))}",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    // New: update role permissions by delta (add/remove keys)
    public async Task UpdateRolePermissionsDeltaByKeysAsync(
        int roleId,
        IEnumerable<string> addKeys,
        IEnumerable<string> removeKeys,
        Guid actorId,
        CancellationToken ct = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId, ct)
                   ?? throw new InvalidOperationException("Role not found");

        var addSet = new HashSet<string>(addKeys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var removeSet = new HashSet<string>(removeKeys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        // avoid duplicates between add/remove
        addSet.ExceptWith(removeSet);

        var allMap = await _db.Permissions.AsNoTracking()
            .ToDictionaryAsync(p => p.Name, p => p.PermissionId, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var k in addSet)
            if (!allMap.ContainsKey(k))
                throw new InvalidOperationException($"Permission key not found: {k}");
        foreach (var k in removeSet)
            if (!allMap.ContainsKey(k))
                throw new InvalidOperationException($"Permission key not found: {k}");

        var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(ct);
        var existingIds = existing.Select(e => e.PermissionId).ToHashSet();

        var addIds = addSet.Select(k => allMap[k]).Where(id => !existingIds.Contains(id)).ToList();
        var removeIds = removeSet.Select(k => allMap[k]).ToHashSet();
        var toRemove = existing.Where(e => removeIds.Contains(e.PermissionId)).ToList();

        if (toRemove.Count > 0) _db.RolePermissions.RemoveRange(toRemove);
        foreach (var pid in addIds)
            _db.RolePermissions.Add(new Domain.Entities.RolePermission { RoleId = roleId, PermissionId = pid, GrantedAt = DateTime.UtcNow });

        role.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            Action = "ASSIGN_ROLE_PERMS_DELTA",
            UserId = actorId,
            Resource = $"Role:{role.Name}",
            Description = $"Delta permissions: +[{string.Join(',', addSet.OrderBy(x => x))}] -[{string.Join(',', removeSet.OrderBy(x => x))}]",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
