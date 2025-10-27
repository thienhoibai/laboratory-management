using Microsoft.EntityFrameworkCore;
using Patient.Domain.Entities;

namespace Patient.Infrastructure;

public class PatientDbContext : DbContext
{
    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }

    public DbSet<PatientEntity> Patients => Set<PatientEntity>();
    public DbSet<PatientRecordVersion> PatientRecordVersions => Set<PatientRecordVersion>();
    public DbSet<PatientEventLog> PatientEventLogs => Set<PatientEventLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientEntity>(b =>
        {
            b.ToTable("patients");
            b.HasKey(x => x.PatientId);
            b.Property(x => x.PatientId).HasColumnName("patient_id").ValueGeneratedNever();

            // Map to user_id to be compatible with existing DB
            b.Property(x => x.UserId).HasColumnName("user_id").IsRequired();

            b.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();
            b.Property(x => x.DateOfBirth).HasColumnName("date_of_birth");
            b.Property(x => x.Gender).HasColumnName("gender");

            b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(32);
            b.Property(x => x.Email).HasColumnName("email").HasMaxLength(256);
            b.Property(x => x.Address).HasColumnName("address").HasMaxLength(300);

            b.Property(x => x.IdNumber).HasColumnName("citizen_id").HasMaxLength(12).IsFixedLength();
            b.Property(x => x.InsuranceNumber).HasColumnName("insurance_number").HasMaxLength(64);

            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            b.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");

            b.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            b.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            b.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id");

            // Ignore properties not in DB
            b.Ignore(x => x.FullNameNorm);
            b.Ignore(x => x.PhoneLast4);
            b.Ignore(x => x.CreatedChannel);

            b.HasQueryFilter(x => !x.IsDeleted);
            b.HasIndex(x => x.UserId).HasDatabaseName("IX_patients_owner");
            b.HasIndex(x => x.FullName).HasDatabaseName("IX_patients_full_name");
        });

        modelBuilder.Entity<PatientRecordVersion>(b =>
        {
            b.ToTable("patient_record_versions");
            b.HasKey(x => x.VersionId);
            b.Property(x => x.VersionId).HasColumnName("version_id").ValueGeneratedOnAdd();
            b.Property(x => x.PatientId).HasColumnName("patient_id");
            b.Property(x => x.VersionNo).HasColumnName("version_no");
            b.Property(x => x.ChangedBy).HasColumnName("changed_by");
            b.Property(x => x.ChangedAt).HasColumnName("changed_at");
            b.Property(x => x.ChangeSet).HasColumnName("change_set");
            b.Property(x => x.FullSnapshot).HasColumnName("full_snapshot");
            b.HasOne(x => x.Patient).WithMany(p => p.Versions).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatientEventLog>(b =>
        {
            b.ToTable("patient_event_log");
            b.HasKey(x => x.LogId);
            b.Property(x => x.LogId).HasColumnName("log_id").ValueGeneratedOnAdd();
            b.Property(x => x.PatientId).HasColumnName("patient_id");
            b.Property(x => x.EventType).HasColumnName("event_type");
            b.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
            b.Property(x => x.Detail).HasColumnName("detail");
            b.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            b.Property(x => x.CorrelationId).HasColumnName("correlation_id");
            b.Property(x => x.TraceId).HasColumnName("trace_id");
            b.HasOne(x => x.Patient).WithMany(p => p.EventLogs).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("audit_logs");
            b.HasKey(x => x.AuditId);
            b.Property(x => x.AuditId).HasColumnName("audit_id").ValueGeneratedOnAdd();
            b.Property(x => x.Entity).HasColumnName("entity").HasMaxLength(64).IsRequired();
            b.Property(x => x.EntityId).HasColumnName("entity_id");
            b.Property(x => x.Action).HasColumnName("action").HasMaxLength(32).IsRequired();
            b.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(64);
            b.Property(x => x.DetailJson).HasColumnName("detail_json");
        });
    }
}
