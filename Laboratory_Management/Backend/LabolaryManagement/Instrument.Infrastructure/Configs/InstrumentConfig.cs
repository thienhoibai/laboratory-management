using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Instrument.Domain.Enums;
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
        
        // Convert enum to byte in database
        b.Property(x => x.Status)
            .HasConversion<byte>()
            .HasDefaultValue(InstrumentStatus.Online);
        
        b.Property(x => x.ReagentStatus)
            .HasConversion<byte>()
            .HasDefaultValue(ReagentStatus.OK);
        
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
    }
}
