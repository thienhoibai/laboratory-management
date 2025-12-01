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
            entity.HasKey(e => e.SlotId).HasName("PK__Appointm__0A124AAF2E60C578");

            entity.ToTable("AppointmentSlot");

            entity.HasIndex(e => new { e.AppointmentDate, e.TimeBlockId }, "UQ_TimeSlot").IsUnique();

            entity.Property(e => e.SlotId).ValueGeneratedNever();
            entity.Property(e => e.MaxBooking).HasDefaultValue(10);

            entity.HasOne(d => d.TimeBlock).WithMany(p => p.AppointmentSlots)
                .HasForeignKey(d => d.TimeBlockId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__TimeB__5165187F");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__EB5F6CBD3E4E8010");

            entity.ToTable("AuditLog");

            entity.Property(e => e.AuditLogId).ValueGeneratedNever();
            entity.Property(e => e.Action).HasMaxLength(255);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951AED9605350E");

            entity.ToTable("Booking");

            entity.Property(e => e.BookingId).ValueGeneratedNever();
            entity.Property(e => e.BookingCode).HasMaxLength(50);
            entity.Property(e => e.CancelAt).HasColumnType("datetime");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.PatientEmail).HasMaxLength(255);
            entity.Property(e => e.PatientName).HasMaxLength(255);
            entity.Property(e => e.PatientPhone).HasMaxLength(12);
            entity.Property(e => e.RanBy).HasMaxLength(30);

            entity.HasOne(d => d.AppointmentSlot).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.AppointmentSlotId)
                .HasConstraintName("FK__Booking__Appoint__5535A963");

            entity.HasOne(d => d.Bundle).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.BundleId)
                .HasConstraintName("FK__Booking__BundleI__571DF1D5");
        });

        modelBuilder.Entity<BookingTest>(entity =>
        {
            entity.HasKey(e => e.TestBookingNo).HasName("PK__BookingT__E0217BD0D3B06BE1");

            entity.ToTable("BookingTest");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingTests)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__BookingTe__Booki__6C190EBB");

            entity.HasOne(d => d.Catalog).WithMany(p => p.BookingTests)
                .HasForeignKey(d => d.CatalogId)
                .HasConstraintName("FK__BookingTe__Catal__6D0D32F4");
        });

        modelBuilder.Entity<CatalogBundle>(entity =>
        {
            entity.HasKey(e => new { e.BundleId, e.CatalogId }).HasName("PK__CatalogB__DE2527E762F90615");

            entity.ToTable("CatalogBundle");

            entity.HasOne(d => d.Bundle).WithMany(p => p.CatalogBundles)
                .HasForeignKey(d => d.BundleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CatalogBu__Bundl__628FA481");

            entity.HasOne(d => d.Catalog).WithMany(p => p.CatalogBundles)
                .HasForeignKey(d => d.CatalogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CatalogBu__Catal__6383C8BA");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__C3B4DFCA3A3F34F3");

            entity.ToTable("Comment");

            entity.Property(e => e.Comment1)
                .HasMaxLength(1000)
                .HasColumnName("Comment");

            entity.HasOne(d => d.Test).WithMany(p => p.Comments)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("FK__Comment__TestId__5DCAEF64");
        });

        modelBuilder.Entity<PaymentEnvoice>(entity =>
        {
            entity.HasKey(e => e.PaymentNo).HasName("PK__PaymentE__9B55726797ED56A6");

            entity.ToTable("PaymentEnvoice");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.PaidAt).HasColumnType("datetime");

            entity.HasOne(d => d.Booking).WithMany(p => p.PaymentEnvoices)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PaymentEn__Booki__5AEE82B9");
        });

        modelBuilder.Entity<TestBundle>(entity =>
        {
            entity.HasKey(e => e.BundleId).HasName("PK__TestBund__42003451FEA5724F");

            entity.ToTable("TestBundle");

            entity.Property(e => e.BundleName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<TestCatalog>(entity =>
        {
            entity.HasKey(e => e.CatalogId).HasName("PK__TestCata__C2513B6838281F9F");

            entity.ToTable("TestCatalog");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TestName).HasMaxLength(100);

            entity.HasMany(d => d.Parameters).WithMany(p => p.Catalogs)
                .UsingEntity<Dictionary<string, object>>(
                    "CatalogParameter",
                    r => r.HasOne<TestParameter>().WithMany()
                        .HasForeignKey("ParameterId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__CatalogPa__Param__693CA210"),
                    l => l.HasOne<TestCatalog>().WithMany()
                        .HasForeignKey("CatalogId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__CatalogPa__Catal__68487DD7"),
                    j =>
                    {
                        j.HasKey("CatalogId", "ParameterId").HasName("PK__CatalogP__ADD1FD4FA5109A19");
                        j.ToTable("CatalogParameter");
                    });
        });

        modelBuilder.Entity<TestParameter>(entity =>
        {
            entity.HasKey(e => e.ParameterId).HasName("PK__TestPara__F80C6277667FF7A1");

            entity.ToTable("TestParameter");

            entity.Property(e => e.ParameterName).HasMaxLength(100);
            entity.Property(e => e.ReferenceRange).HasMaxLength(100);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<TestReport>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__TestRepo__1ABEEF0FF5655FF7");

            entity.ToTable("TestReport");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Filename).HasMaxLength(255);

            entity.HasOne(d => d.Booking).WithMany(p => p.TestReports)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__TestRepor__Booki__73BA3083");
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__TestResu__976902086AC252E9");

            entity.ToTable("TestResult");

            entity.Property(e => e.IsNormal).HasColumnName("isNormal");
            entity.Property(e => e.ResultValue).HasMaxLength(100);

            entity.HasOne(d => d.Parameter).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.ParameterId)
                .HasConstraintName("FK__TestResul__Param__70DDC3D8");

            entity.HasOne(d => d.TestBookingNoNavigation).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.TestBookingNo)
                .HasConstraintName("FK__TestResul__TestB__6FE99F9F");
        });

        modelBuilder.Entity<TimeBlock>(entity =>
        {
            entity.HasKey(e => e.TimeBlockId).HasName("PK__TimeBloc__78D14F4E66AA06F1");

            entity.ToTable("TimeBlock");

            entity.HasIndex(e => e.TimeBlock1, "UQ__TimeBloc__3E92327B2D1F526F").IsUnique();

            entity.Property(e => e.TimeBlock1).HasColumnName("TimeBlock");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
