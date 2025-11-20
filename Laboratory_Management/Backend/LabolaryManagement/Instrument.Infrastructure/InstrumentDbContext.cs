using Microsoft.EntityFrameworkCore;
using Instrument.Infrastructure.Configs;
using DomainInstrument = Instrument.Domain.Entities.Instrument;
using DomainInstrumentResult = Instrument.Domain.Entities.InstrumentResult;

namespace Instrument.Infrastructure;

public class InstrumentDbContext : DbContext
{
    public InstrumentDbContext(DbContextOptions<InstrumentDbContext> options) : base(options) { }

    public DbSet<DomainInstrument> Instruments => Set<DomainInstrument>();
    public DbSet<DomainInstrumentResult> InstrumentResults => Set<DomainInstrumentResult>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfiguration(new InstrumentConfig());
        b.ApplyConfiguration(new InstrumentResultConfig());
    }
}
