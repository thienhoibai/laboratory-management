using IAM.Application.Permissions.DTOs;
using IAM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IAM.Application.Permissions
{
    public class PermissionQuery : IPermissionQuery
    {
        private readonly IamDbContext _db;
        public PermissionQuery(IamDbContext db) => _db = db;

        public async Task<List<PermissionGroupDto>> GetGroupsAsync(CancellationToken ct = default)
        {
            var all = await _db.Permissions.AsNoTracking()
                .Select(p => p.Name)
                .ToListAsync(ct);

            var groups = all
                .Select(n =>
                {
                    var parts = n.Split('.', 2);
                    var module = parts.Length > 1 ? parts[0] : n;
                    var action = parts.Length > 1 ? parts[1] : "View";
                    return new { module, action, key = n };
                })
                .GroupBy(x => x.module)
                .OrderBy(g => g.Key)
                .Select(g => new PermissionGroupDto(
                    Module: g.Key,
                    Label: g.Key, // simple label; FE can localize
                    Permissions: g.OrderBy(x => x.action).Select(x => new PermissionItemDto(x.key, x.action)).ToList()
                ))
                .ToList();

            return groups;
        }
    }
}
