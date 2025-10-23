using System;
using System.Collections.Generic;
using Patient.Domain;
using Microsoft.EntityFrameworkCore;
using Patient.Infrastructure.PatientDB;


namespace Patient.Infrastructure.Data;

public partial class PatientDBContext : DbContext
{
    public PatientDBContext()
    {
    }

    public PatientDBContext(DbContextOptions<PatientDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Patient.Domain.Patient> Patients { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=TUAN\\SQLEXPRESS01;database=PatientServiceDB;uid=sa;pwd=12345;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient.Domain.Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patients__970EC366200F05A4");

            entity.ToTable(tb => tb.HasTrigger("trg_Update_Patient"));

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.BloodGroup).HasMaxLength(5);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
