using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IAM.Application.Statistics.DTOs;
using IAM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IAM.Application.Statistics.Services;

public class UserStatisticsService
{
    private readonly IamDbContext _db;

    public UserStatisticsService(IamDbContext db)
    {
        _db = db;
    }

    public async Task<UserStatisticsDto> GetUserStatisticsAsync(CancellationToken ct = default)
    {
        var today = DateTime.Now.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        // Lấy tất cả users với role Customer
        var customerRoleId = await _db.Roles
            .Where(r => r.Name == "Customer")
            .Select(r => r.RoleId)
            .FirstOrDefaultAsync(ct);

        if (customerRoleId == 0)
        {
            return new UserStatisticsDto
            {
                TotalUsers = await _db.Users.CountAsync(ct),
                GeneratedAt = DateTime.Now
            };
        }

        var customerUserIds = await _db.UserRoles
            .Where(ur => ur.RoleId == customerRoleId)
            .Select(ur => ur.UserId)
            .ToListAsync(ct);

        var customers = await _db.Users
            .Where(u => customerUserIds.Contains(u.UserId))
            .ToListAsync(ct);

        var totalUsers = await _db.Users.CountAsync(ct);
        var totalCustomers = customers.Count;
        var activeCustomers = customers.Count(u => u.IsActive);
        var inactiveCustomers = customers.Count(u => !u.IsActive);
        var newCustomersThisMonth = customers.Count(u => u.CreatedAt >= firstDayOfMonth);
        var newCustomersToday = customers.Count(u => u.CreatedAt.Date == today);

        return new UserStatisticsDto
        {
            TotalUsers = totalUsers,
            TotalCustomers = totalCustomers,
            ActiveCustomers = activeCustomers,
            InactiveCustomers = inactiveCustomers,
            NewCustomersThisMonth = newCustomersThisMonth,
            NewCustomersToday = newCustomersToday,

            GeneratedAt = DateTime.Now
        };
        
    }
    public async Task<BlockedUserStatisticDto> GetBlockedUserStatistic(CancellationToken ct = default)
    {
        var users = await _db.Users.AsNoTracking().ToListAsync(ct);
        return new BlockedUserStatisticDto
        {
            TotalUsers = users.Count,
            TotalCustomers = users.Count(u => u.IsActive),
            TotalBlockedUsers = users.Count(u => u.IsLocked),
            GeneratedAt = DateTime.Now
        };
    }
    public async Task<UserByDayDto> GetUserByDay()
    {
        var Lastweek = DateTime.Now.AddDays(-6);
        var WeeklyUser =  _db.Users.AsNoTracking()
            .Where(u => u.IsActive&& u.CreatedAt >= new DateTime(Lastweek.Year, Lastweek.Month, Lastweek.Day))
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new CountUserByDayDto
            {
                Day = g.Key,
                UserCount = g.Count()


            }).ToArrayAsync();
        return new UserByDayDto
        {
            UsersByDay = await WeeklyUser
        };
    }
}
