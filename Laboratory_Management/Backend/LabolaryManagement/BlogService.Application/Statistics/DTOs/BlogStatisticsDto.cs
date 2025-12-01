using System;

namespace BlogService.Application.Statistics.DTOs;

public record BlogStatisticsDto
{
    public int TotalPosts { get; init; }
    public int PublishedPosts { get; init; }
    public int DraftPosts { get; init; }
    public int PendingPosts { get; init; }
    public int PostsThisMonth { get; init; }
    public int PostsToday { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.Now;
}
