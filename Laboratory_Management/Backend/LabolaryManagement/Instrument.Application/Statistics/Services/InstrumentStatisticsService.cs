using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Instrument.Application.Statistics.DTOs;
using Instrument.Domain.Enums;
using Instrument.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Instrument.Application.Statistics.Services;

public class InstrumentStatisticsService
{
    private readonly InstrumentDbContext _db;

    public InstrumentStatisticsService(InstrumentDbContext db)
    {
        _db = db;
    }

    public async Task<InstrumentStatisticsDto> GetInstrumentStatisticsAsync(CancellationToken ct = default)
    {
        var instruments = await _db.Instruments.AsNoTracking().ToListAsync(ct);

        return new InstrumentStatisticsDto
        {
            TotalInstruments = instruments.Count,
            OnlineInstruments = instruments.Count(i => i.Status == InstrumentStatus.Online),
            OfflineInstruments = instruments.Count(i => i.Status == InstrumentStatus.Offline),
            MaintenanceInstruments = instruments.Count(i => i.Status == InstrumentStatus.Maintenance),
            FaultInstruments = instruments.Count(i => i.Status == InstrumentStatus.Fault),
            GeneratedAt = DateTime.Now
        };
    }
}
