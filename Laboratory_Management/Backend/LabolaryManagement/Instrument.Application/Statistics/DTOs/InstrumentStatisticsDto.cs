using System;

namespace Instrument.Application.Statistics.DTOs;

public record InstrumentStatisticsDto
{
    public int TotalInstruments { get; init; }
    public int OnlineInstruments { get; init; }
    public int OfflineInstruments { get; init; }
    public int MaintenanceInstruments { get; init; }
    public int FaultInstruments { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.Now;
}
