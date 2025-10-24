using Microsoft.EntityFrameworkCore;
using Patient.Domain.Entities;

namespace Patient.Infrastructure;

public class PatientDbContext : DbContext
{
    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }

    public DbSet<PatientEntity> Patients => Set<PatientEntity>();
    public DbSet<PatientRecordVersion> PatientRecordVersions => Set<PatientRecordVersion>();
    public DbSet<PatientEventLog> PatientEventLogs => Set<PatientEventLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientEntity>(b =>
        {
            b.ToTable("patients");
            b.HasKey(x => x.PatientId);
            b.Property(x => x.PatientId).HasColumnName("patient_id").ValueGeneratedNever();
            b.Property(x => x.FullNameEnc).HasColumnName("full_name_enc");
            b.Property(x => x.DobEnc).HasColumnName("dob_enc");
            b.Property(x => x.Gender).HasColumnName("gender");
            b.Property(x => x.PhoneEnc).HasColumnName("phone_enc");
            b.Property(x => x.EmailEnc).HasColumnName("email_enc");
            b.Property(x => x.AddressEnc).HasColumnName("address_enc");
            b.Property(x => x.IdNumberEnc).HasColumnName("id_number_enc");
            b.Property(x => x.InsuranceNumberEnc).HasColumnName("insurance_number_enc");
            b.Property(x => x.FullNameNorm).HasColumnName("full_name_norm").HasMaxLength(256);
            b.Property(x => x.DateOfBirth).HasColumnName("date_of_birth");
            b.Property(x => x.PhoneLast4).HasColumnName("phone_last4").HasMaxLength(4).IsFixedLength();
            b.Property(x => x.UserId).HasColumnName("user_id");
            b.Property(x => x.CreatedChannel).HasColumnName("created_channel").HasMaxLength(32);
            b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            b.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            b.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            b.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            b.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id");

            // Indexes + soft delete filter
            b.HasIndex(x => new { x.FullNameNorm, x.DateOfBirth });
            b.HasIndex(x => x.PhoneLast4);
            b.HasQueryFilter(x => !x.IsDeleted);
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
            b.HasIndex(x => new { x.PatientId, x.ChangedAt }).HasDatabaseName("IX_prv_patient_time");
            b.HasOne(x => x.Patient)
             .WithMany(p => p.Versions)
             .HasForeignKey(x => x.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatientEventLog>(b =>
        {
            b.ToTable("patient_event_log");
            b.HasKey(x => x.LogId);
            b.Property(x => x.LogId).HasColumnName("log_id").ValueGeneratedOnAdd();
            b.Property(x => x.PatientId).HasColumnName("patient_id");
            b.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(64);
            b.Property(x => x.ActorUserId).HasColumnName("actor_user_id");
            b.Property(x => x.Detail).HasColumnName("detail");
            b.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            b.Property(x => x.CorrelationId).HasColumnName("correlation_id");
            b.Property(x => x.TraceId).HasColumnName("trace_id");
            b.HasIndex(x => new { x.PatientId, x.OccurredAt }).HasDatabaseName("IX_log_patient_time");
            b.HasIndex(x => new { x.EventType, x.OccurredAt }).HasDatabaseName("IX_log_event_time");
            b.HasOne(x => x.Patient)
             .WithMany(p => p.EventLogs)
             .HasForeignKey(x => x.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
