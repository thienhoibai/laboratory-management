using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Patient.Infrastructure.MedicalRecordServiceDB;

namespace Patient.Infrastructure.Data;

public partial class MedicalRecordServiceDBContext : DbContext
{
    public MedicalRecordServiceDBContext()
    {
    }

    public MedicalRecordServiceDBContext(DbContextOptions<MedicalRecordServiceDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<TestHistory> TestHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=TUAN\\SQLEXPRESS01;database=MedicalRecordServiceDB;uid=sa;pwd=12345;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78E9C4885EF3");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Diagnosis).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.RecordName).HasMaxLength(255);
            entity.Property(e => e.Treatment).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TestHistory>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__TestHist__8CC3316091055EE6");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TestType).HasMaxLength(255);

            entity.HasOne(d => d.Record).WithMany(p => p.TestHistories)
                .HasForeignKey(d => d.RecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestHistories_MedicalRecords");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
