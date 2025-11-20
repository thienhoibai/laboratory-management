using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainInstrument = Instrument.Domain.Entities.Instrument;

namespace Instrument.Infrastructure.Configs;

public class InstrumentConfig : IEntityTypeConfiguration<DomainInstrument>
{
    public void Configure(EntityTypeBuilder<DomainInstrument> b)
    {
        b.ToTable("Instruments");
        b.HasKey(x => x.InstrumentId);
        b.Property(x => x.InstrumentCode).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.InstrumentCode).IsUnique();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Status).HasMaxLength(10).HasDefaultValue("ONLINE");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
