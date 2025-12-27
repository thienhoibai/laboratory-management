using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlogService.Application.Statistics.DTOs;
using BlogService.Infrastructure.Data; // ✅ Add this
using Microsoft.EntityFrameworkCore;

namespace BlogService.Application.Statistics.Services;

public class BlogStatisticsService
{
    private readonly DBContext _db; // ✅ Fix: DBContext not BlogDbContext

    public BlogStatisticsService(DBContext db)
    {
        _db = db;
    }

    public async Task<BlogStatisticsDto> GetBlogStatisticsAsync(CancellationToken ct = default)
    {
        var today = DateTime.Now.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var allPosts = await _db.BlogPosts.AsNoTracking().ToListAsync(ct);

        return new BlogStatisticsDto
        {
            TotalPosts = allPosts.Count,
            PublishedPosts = allPosts.Count(p => p.Status == 1), // Published
            DraftPosts = allPosts.Count(p => p.Status == 0), // Draft
            PendingPosts = allPosts.Count(p => p.Status == 2), // Pending approval
            PostsThisMonth = allPosts.Count(p => p.CreatedDate.HasValue && p.CreatedDate.Value >= firstDayOfMonth), // ✅ Fix: CreatedDate
            PostsToday = allPosts.Count(p => p.CreatedDate.HasValue && p.CreatedDate.Value.Date == today), // ✅ Fix: CreatedDate
            GeneratedAt = DateTime.Now
        };
    }
}
