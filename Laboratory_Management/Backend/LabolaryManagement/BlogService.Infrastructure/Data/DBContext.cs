using System;
using System.Collections.Generic;
using BlogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Infrastructure.Data;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlogPost> BlogPosts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__BlogPost__AA12601820DD6B56");

            entity.ToTable("BlogPost");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.IsPublished).HasDefaultValue(false);
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__BlogPost__Catego__4BAC3F29");

            entity.HasMany(d => d.Tags).WithMany(p => p.Posts)
                .UsingEntity<Dictionary<string, object>>(
                    "BlogPostTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BlogPostT__TagId__5441852A"),
                    l => l.HasOne<BlogPost>().WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__BlogPostT__PostI__534D60F1"),
                    j =>
                    {
                        j.HasKey("PostId", "TagId").HasName("PK__BlogPost__7C45AF8237FE0883");
                        j.ToTable("BlogPostTag");
                    });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0B647D211F");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comment__C3B4DFCACD7609A9");

            entity.ToTable("Comment");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__Comment__PostId__571DF1D5");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tag__657CF9AC49C3CEA6");

            entity.ToTable("Tag");

            entity.Property(e => e.TagName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
