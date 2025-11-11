using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainInstrumentResult = Instrument.Domain.Entities.InstrumentResult;

namespace Instrument.Infrastructure.Configs;

public class InstrumentResultConfig : IEntityTypeConfiguration<DomainInstrumentResult>
{
    public void Configure(EntityTypeBuilder<DomainInstrumentResult> b)
    {
        b.ToTable("InstrumentResults");
        b.HasKey(x => x.InstrumentResultId);
        b.Property(x => x.InstrumentCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.ParameterName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Unit).HasMaxLength(50);
        b.Property(x => x.ReferenceRange).HasMaxLength(100);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        b.HasIndex(x => x.BookingId);
        b.HasIndex(x => x.ParameterId);
    }
}
