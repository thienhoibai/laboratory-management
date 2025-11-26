using Microsoft.EntityFrameworkCore;
using Instrument.Infrastructure.Configs;
using Instrument.Domain.Enums;
using DomainInstrument = Instrument.Domain.Entities.Instrument;
using Instrument.Domain.Entities;

namespace Instrument.Infrastructure;

/// <summary>
/// MINIMAL VERSION - Chỉ quản lý Instrument và Run
/// Đã loại bỏ: Cartridge, LoadTx, RunUsage, InstrumentResult
/// </summary>
public class InstrumentDbContext : DbContext
{
    public InstrumentDbContext(DbContextOptions<InstrumentDbContext> options) : base(options) { }

    // ✅ CORE TABLES - Chỉ 2 bảng
    public DbSet<DomainInstrument> Instruments => Set<DomainInstrument>();
    public DbSet<InstrumentRun> InstrumentRuns => Set<InstrumentRun>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ===== INSTRUMENTS TABLE =====
        b.ApplyConfiguration(new InstrumentConfig());
        
        // ===== INSTRUMENT RUNS TABLE =====
        b.Entity<InstrumentRun>(entity =>
        {
            entity.ToTable("InstrumentRuns");
            entity.HasKey(e => e.RunId);
            
            entity.Property(e => e.InstrumentCode).HasMaxLength(50).IsRequired();
            
            // Convert enum to byte in database
            entity.Property(e => e.Status)
                .HasConversion<byte>()
                .HasDefaultValue(RunStatus.Running);
            
            entity.Property(e => e.StartedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            
            // Indexes
            entity.HasIndex(e => e.BookingId);
            entity.HasIndex(e => e.InstrumentCode);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.InstrumentCode, e.Status });
        });
    }
}
