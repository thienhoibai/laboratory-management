using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Data;

public partial class TestOrderDBContext : DbContext
{
    public TestOrderDBContext()
    {
    }

    public TestOrderDBContext(DbContextOptions<TestOrderDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppointmentSlot> AppointmentSlots { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingTest> BookingTests { get; set; }

    public virtual DbSet<CatalogBundle> CatalogBundles { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<PaymentEnvoice> PaymentEnvoices { get; set; }

    public virtual DbSet<TestBundle> TestBundles { get; set; }

    public virtual DbSet<TestCatalog> TestCatalogs { get; set; }

    public virtual DbSet<TestParameter> TestParameters { get; set; }

    public virtual DbSet<TestReport> TestReports { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    public virtual DbSet<TimeBlock> TimeBlocks { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__Appointm__0A124AAF1AEF3F93");

            entity.ToTable("AppointmentSlot");

            entity.HasIndex(e => new { e.AppointmentDate, e.TimeBlockId }, "UQ_TimeSlot").IsUnique();

            entity.Property(e => e.SlotId).ValueGeneratedNever();
            entity.Property(e => e.MaxBooking).HasDefaultValue(10);

            entity.HasOne(d => d.TimeBlock).WithMany(p => p.AppointmentSlots)
                .HasForeignKey(d => d.TimeBlockId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__TimeB__4F7CD00D");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__EB5F6CBD2DA0B57C");

            entity.ToTable("AuditLog");

            entity.Property(e => e.AuditLogId).ValueGeneratedNever();
            entity.Property(e => e.Action).HasMaxLength(255);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951AED1D7754EF");

            entity.ToTable("Booking");

            entity.Property(e => e.BookingId).ValueGeneratedNever();
            entity.Property(e => e.BookingCode).HasMaxLength(10);
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.PatientEmail).HasMaxLength(255);
            entity.Property(e => e.PatientName).HasMaxLength(255);
            entity.Property(e => e.PatientPhone).HasMaxLength(12);
            entity.Property(e => e.RanBy).HasMaxLength(30);

            entity.HasOne(d => d.AppointmentSlot).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.AppointmentSlotId)
                .HasConstraintName("FK__Booking__Appoint__534D60F1");

            entity.HasOne(d => d.Bundle).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.BundleId)
                .HasConstraintName("FK__Booking__BundleI__5535A963");
        });

        modelBuilder.Entity<BookingTest>(entity =>
        {
            entity.HasKey(e => e.TestBookingNo).HasName("PK__BookingT__E0217BD0168EB5BA");

            entity.ToTable("BookingTest");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingTests)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__BookingTe__Booki__6A30C649");

            entity.HasOne(d => d.Catalog).WithMany(p => p.BookingTests)
                .HasForeignKey(d => d.CatalogId)
                .HasConstraintName("FK__BookingTe__Catal__6B24EA82");
        });

        modelBuilder.Entity<CatalogBundle>(entity =>
        {
            entity.HasKey(e => new { e.BundleId, e.CatalogId }).HasName("PK__CatalogB__DE2527E772CF0586");

            entity.ToTable("CatalogBundle");

            entity.HasOne(d => d.Bundle).WithMany(p => p.CatalogBundles)
                .HasForeignKey(d => d.BundleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CatalogBu__Bundl__60A75C0F");

            entity.HasOne(d => d.Catalog).WithMany(p => p.CatalogBundles)
                .HasForeignKey(d => d.CatalogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CatalogBu__Catal__619B8048");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__C3B4DFCA84C66532");

            entity.ToTable("Comment");

            entity.Property(e => e.Comment1)
                .HasMaxLength(1000)
                .HasColumnName("Comment");

            entity.HasOne(d => d.Test).WithMany(p => p.Comments)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("FK__Comment__TestId__5BE2A6F2");
        });

        modelBuilder.Entity<PaymentEnvoice>(entity =>
        {
            entity.HasKey(e => e.PaymentNo).HasName("PK__PaymentE__9B5572678151F97F");

            entity.ToTable("PaymentEnvoice");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.PaidAt).HasColumnType("datetime");

            entity.HasOne(d => d.Booking).WithMany(p => p.PaymentEnvoices)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PaymentEn__Booki__59063A47");
        });

        modelBuilder.Entity<TestBundle>(entity =>
        {
            entity.HasKey(e => e.BundleId).HasName("PK__TestBund__42003451F23E4E2F");

            entity.ToTable("TestBundle");

            entity.Property(e => e.BundleName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<TestCatalog>(entity =>
        {
            entity.HasKey(e => e.CatalogId).HasName("PK__TestCata__C2513B68A462525E");

            entity.ToTable("TestCatalog");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TestName).HasMaxLength(100);

            entity.HasMany(d => d.Parameters).WithMany(p => p.Catalogs)
                .UsingEntity<Dictionary<string, object>>(
                    "CatalogParameter",
                    r => r.HasOne<TestParameter>().WithMany()
                        .HasForeignKey("ParameterId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__CatalogPa__Param__6754599E"),
                    l => l.HasOne<TestCatalog>().WithMany()
                        .HasForeignKey("CatalogId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__CatalogPa__Catal__66603565"),
                    j =>
                    {
                        j.HasKey("CatalogId", "ParameterId").HasName("PK__CatalogP__ADD1FD4F6B9CF3A1");
                        j.ToTable("CatalogParameter");
                    });
        });

        modelBuilder.Entity<TestParameter>(entity =>
        {
            entity.HasKey(e => e.ParameterId).HasName("PK__TestPara__F80C627783885F14");

            entity.ToTable("TestParameter");

            entity.Property(e => e.ParameterName).HasMaxLength(100);
            entity.Property(e => e.ReferenceRange).HasMaxLength(100);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<TestReport>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__TestRepo__1ABEEF0F29F256DC");

            entity.ToTable("TestReport");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Filename).HasMaxLength(255);

            entity.HasOne(d => d.Booking).WithMany(p => p.TestReports)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__TestRepor__Booki__71D1E811");
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__TestResu__97690208CA0BCD8B");

            entity.ToTable("TestResult");

            entity.Property(e => e.ResultValue).HasMaxLength(100);

            entity.HasOne(d => d.Parameter).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.ParameterId)
                .HasConstraintName("FK__TestResul__Param__6EF57B66");

            entity.HasOne(d => d.TestBookingNoNavigation).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.TestBookingNo)
                .HasConstraintName("FK__TestResul__TestB__6E01572D");
        });

        modelBuilder.Entity<TimeBlock>(entity =>
        {
            entity.HasKey(e => e.TimeBlockId).HasName("PK__TimeBloc__78D14F4EF028F318");

            entity.ToTable("TimeBlock");

            entity.HasIndex(e => e.TimeBlock1, "UQ__TimeBloc__3E92327B692B26C4").IsUnique();

            entity.Property(e => e.TimeBlock1).HasColumnName("TimeBlock");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
