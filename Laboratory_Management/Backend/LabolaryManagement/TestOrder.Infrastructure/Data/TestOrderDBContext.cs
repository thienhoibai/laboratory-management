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

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingTest> BookingTests { get; set; }

    public virtual DbSet<CatalogBundle> CatalogBundles { get; set; }

    public virtual DbSet<CatalogParameter> CatalogParameters { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<PaymentEnvoice> PaymentEnvoices { get; set; }

    public virtual DbSet<TestBundle> TestBundles { get; set; }

    public virtual DbSet<TestCatalog> TestCatalogs { get; set; }

    public virtual DbSet<TestParameter> TestParameters { get; set; }

    public virtual DbSet<TestReport> TestReports { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    public virtual DbSet<TimeBlock> TimeBlocks { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=LAPTOP-SHE2A3S2\\SQLEXPRESS;Database=TestOrderDB;User=sa;Password=12345;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__Appointm__0A124AAF146A6270");

            entity.ToTable("AppointmentSlot");

            entity.HasIndex(e => new { e.AppointmentDate, e.TimeBlockId }, "UQ_TimeSlot").IsUnique();

            entity.Property(e => e.MaxBooking).HasDefaultValue(10);

            entity.HasOne(d => d.TimeBlock).WithMany(p => p.AppointmentSlots)
                .HasForeignKey(d => d.TimeBlockId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__TimeB__1CBC4616");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951AED45FF0B5E");

            entity.ToTable("Booking");

            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(30);
            entity.Property(e => e.RanBy).HasMaxLength(30);

            entity.HasOne(d => d.AppointmentSlot).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.AppointmentSlotId)
                .HasConstraintName("FK__Booking__Appoint__1EA48E88");

            entity.HasOne(d => d.Bundle).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.BundleId)
                .HasConstraintName("FK__Booking__BundleI__02FC7413");
        });

        modelBuilder.Entity<BookingTest>(entity =>
        {
            entity.HasKey(e => e.TestBookingNo).HasName("PK__BookingT__E0217BD0D0D56492");

            entity.ToTable("BookingTest");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingTests)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__BookingTe__Booki__5441852A");

            entity.HasOne(d => d.Catalog).WithMany(p => p.BookingTests)
                .HasForeignKey(d => d.CatalogId)
                .HasConstraintName("FK__BookingTe__Catal__5535A963");
        });

        modelBuilder.Entity<CatalogBundle>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("CatalogBundle");

            entity.HasOne(d => d.Bundle).WithMany()
                .HasForeignKey(d => d.BundleId)
                .HasConstraintName("FK__CatalogBu__Bundl__07C12930");

            entity.HasOne(d => d.Catalog).WithMany()
                .HasForeignKey(d => d.CatalogId)
                .HasConstraintName("FK__CatalogBu__Catal__08B54D69");
        });

        modelBuilder.Entity<CatalogParameter>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("CatalogParameter");

            entity.HasOne(d => d.Catalog).WithMany()
                .HasForeignKey(d => d.CatalogId)
                .HasConstraintName("FK__CatalogPa__Catal__6EF57B66");

            entity.HasOne(d => d.Parameter).WithMany()
                .HasForeignKey(d => d.ParameterId)
                .HasConstraintName("FK__CatalogPa__Param__6FE99F9F");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__C3B4DFCAF6A33FD5");

            entity.ToTable("Comment");

            entity.Property(e => e.Comment1)
                .HasMaxLength(1000)
                .HasColumnName("Comment");

            entity.HasOne(d => d.Test).WithMany(p => p.Comments)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("FK__Comment__TestId__4CA06362");
        });

        modelBuilder.Entity<PaymentEnvoice>(entity =>
        {
            entity.HasKey(e => e.PaymentNo).HasName("PK__PaymentE__9B5572677EF44743");

            entity.ToTable("PaymentEnvoice");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Method).HasMaxLength(15);
            entity.Property(e => e.Token).HasMaxLength(50);

            entity.HasOne(d => d.Booking).WithMany(p => p.PaymentEnvoices)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__PaymentEn__Booki__72C60C4A");
        });

        modelBuilder.Entity<TestBundle>(entity =>
        {
            entity.HasKey(e => e.BundleId).HasName("PK__TestBund__42003451C368B937");

            entity.ToTable("TestBundle");

            entity.Property(e => e.BundleName).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasColumnName("isActive");
        });

        modelBuilder.Entity<TestCatalog>(entity =>
        {
            entity.HasKey(e => e.CatalogId).HasName("PK__TestCata__C2513B68D7AA1C69");

            entity.ToTable("TestCatalog");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TestName).HasMaxLength(100);
        });

        modelBuilder.Entity<TestParameter>(entity =>
        {
            entity.HasKey(e => e.ParameterId).HasName("PK__TestPara__F80C627796B5D220");

            entity.ToTable("TestParameter");

            entity.Property(e => e.ParameterName).HasMaxLength(100);
            entity.Property(e => e.ReferenceRange).HasMaxLength(100);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<TestReport>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__TestRepo__1ABEEF0FD2B4BE19");

            entity.ToTable("TestReport");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Filename).HasMaxLength(255);

            entity.HasOne(d => d.Booking).WithMany(p => p.TestReports)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__TestRepor__Booki__66603565");
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__TestResu__976902082DE6D008");

            entity.ToTable("TestResult");

            entity.Property(e => e.ResultValue).HasMaxLength(100);

            entity.HasOne(d => d.Parameter).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.ParameterId)
                .HasConstraintName("FK__TestResul__Param__59063A47");

            entity.HasOne(d => d.TestBookingNoNavigation).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.TestBookingNo)
                .HasConstraintName("FK__TestResul__TestB__5812160E");
        });

        modelBuilder.Entity<TimeBlock>(entity =>
        {
            entity.HasKey(e => e.TimeBlockId).HasName("PK__TimeBloc__78D14F4E4EDFA26D");

            entity.ToTable("TimeBlock");

            entity.HasIndex(e => e.TimeBlock1, "UQ__TimeBloc__3E92327B313B072E").IsUnique();

            entity.Property(e => e.TimeBlock1).HasColumnName("TimeBlock");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
