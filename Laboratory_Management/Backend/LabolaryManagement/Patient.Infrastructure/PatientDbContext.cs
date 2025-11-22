using Microsoft.EntityFrameworkCore;
using Patient.Domain.Entities;

namespace Patient.Infrastructure;

public class PatientDbContext : DbContext
{
    public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }

    public DbSet<PatientEntity> Patients => Set<PatientEntity>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PatientEntity>(b =>
        {
            b.ToTable("patients");
            b.HasKey(x => x.PatientId);
            b.Property(x => x.PatientId).HasColumnName("patient_id").ValueGeneratedNever();

            // Map to user_id (nullable for guest patients)
            b.Property(x => x.UserId).HasColumnName("user_id");

            b.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();
            b.Property(x => x.DateOfBirth).HasColumnName("date_of_birth");
            b.Property(x => x.Gender).HasColumnName("gender");
            b.Property(x => x.BloodType).HasColumnName("blood_type").HasMaxLength(5);

            b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(32);
            b.Property(x => x.Email).HasColumnName("email").HasMaxLength(256);
            b.Property(x => x.Address).HasColumnName("address").HasMaxLength(300);

            b.Property(x => x.CitizenId).HasColumnName("citizen_id").HasMaxLength(12).IsFixedLength();
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
            b.Ignore(x => x.CreatedChannel);

            b.HasQueryFilter(x => !x.IsDeleted);
            b.HasIndex(x => x.UserId).HasDatabaseName("IX_patients_user_id");
            b.HasIndex(x => x.FullName).HasDatabaseName("IX_patients_full_name");
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
