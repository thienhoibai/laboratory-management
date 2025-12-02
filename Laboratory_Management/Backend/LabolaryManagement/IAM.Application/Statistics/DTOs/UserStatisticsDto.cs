using System;

namespace IAM.Application.Statistics.DTOs;

public record UserStatisticsDto
{
    public int TotalUsers { get; init; }
    public int TotalCustomers { get; init; }
    public int ActiveCustomers { get; init; }
    public int InactiveCustomers { get; init; }
    public int NewCustomersThisMonth { get; init; }
    public int NewCustomersToday { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.Now;

}
