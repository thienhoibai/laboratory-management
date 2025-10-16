using IAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure
{
    public class IamDbContext : DbContext
    {
        public IamDbContext(DbContextOptions<IamDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<UserSecurity> UserSecurities => Set<UserSecurity>();
        public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(x => x.UserId);
                b.Property(x => x.UserId).ValueGeneratedNever();
                b.Property(x => x.Username).IsRequired().HasMaxLength(64);
                b.Property(x => x.Email).IsRequired().HasMaxLength(256);
                b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
                b.Property(x => x.FullName).HasMaxLength(128);
                b.Property(x => x.IsActive).HasDefaultValue(true);
                b.Property(x => x.LastLoginAt);
                b.Property(x => x.CreatedAt);
                b.Property(x => x.UpdatedAt);
                b.Property(x => x.AuthProvider).IsRequired().HasMaxLength(32);
                b.Property(x => x.EmailVerifiedAt);
                b.HasIndex(x => x.Username).IsUnique();
                b.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<Role>(b =>
            {
                b.ToTable("Roles");
                b.HasKey(x => x.RoleId);
                b.Property(x => x.RoleId).ValueGeneratedOnAdd();
                b.Property(x => x.Name).IsRequired().HasMaxLength(64);
                b.Property(x => x.Description).HasMaxLength(256);
                b.Property(x => x.IsDefault).HasDefaultValue(false);
                b.Property(x => x.CreatedAt);
                b.Property(x => x.UpdatedAt);
                b.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<Permission>(b =>
            {
                b.ToTable("Permissions");
                b.HasKey(x => x.PermissionId);
                b.Property(x => x.PermissionId).ValueGeneratedOnAdd();
                b.Property(x => x.Name).IsRequired().HasMaxLength(64);
                b.Property(x => x.Description).HasMaxLength(256);
                b.Property(x => x.CreatedAt);
                b.Property(x => x.UpdatedAt);
                b.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<UserRole>(b =>
            {
                b.ToTable("UserRoles");
                b.HasKey(x => new { x.UserId, x.RoleId });
                b.Property(x => x.AssignedAt);
                b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RolePermission>(b =>
            {
                b.ToTable("RolePermissions");
                b.HasKey(x => new { x.RoleId, x.PermissionId });
                b.Property(x => x.GrantedAt);
                b.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RefreshToken>(b =>
            {
                b.ToTable("RefreshTokens");
                b.HasKey(x => x.RefreshTokenId);
                b.Property(x => x.RefreshTokenId).ValueGeneratedNever();
                b.Property(x => x.TokenHash).IsRequired().HasMaxLength(255);
                b.Property(x => x.IssuedAt);
                b.Property(x => x.ExpiresAt);
                b.Property(x => x.Revoked).HasDefaultValue(false);
                b.Property(x => x.Device).HasMaxLength(128);
                b.Property(x => x.IPAddress).HasMaxLength(64);
                b.HasIndex(x => x.TokenHash).IsUnique();
                b.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditLog>(b =>
            {
                b.ToTable("AuditLogs");
                b.HasKey(x => x.AuditLogId);
                b.Property(x => x.AuditLogId).ValueGeneratedOnAdd();
                b.Property(x => x.AuditLogId).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.Property(x => x.Action).IsRequired().HasMaxLength(64);
                b.Property(x => x.Resource).IsRequired().HasMaxLength(128);
                b.Property(x => x.Description).HasMaxLength(512);
                b.Property(x => x.ActorIp).HasMaxLength(64);
                b.Property(x => x.CreatedAt);
                b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<UserSecurity>(b =>
            {
                b.ToTable("UserSecurities");
                b.HasKey(x => x.UserId);
                b.Property(x => x.FailedAccessCount).HasDefaultValue(0);
                b.Property(x => x.LockoutEnd);
                b.HasOne(x => x.User).WithOne().HasForeignKey<UserSecurity>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PasswordHistory>(b =>
            {
                b.ToTable("PasswordHistories");
                b.HasKey(x => x.PasswordHistoryId);
                b.Property(x => x.PasswordHistoryId).ValueGeneratedNever();
                b.Property(x => x.PasswordHistoryId).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
                b.Property(x => x.CreatedAt);
                b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PasswordResetToken>(b =>
            {
                b.ToTable("PasswordResetTokens");
                b.HasKey(x => x.PasswordResetTokenId);
                b.Property(x => x.PasswordResetTokenId).ValueGeneratedNever();
                b.Property(x => x.PasswordResetTokenId).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.Property(x => x.TokenHash).IsRequired();
                b.Property(x => x.ExpiresAt);
                b.Property(x => x.UsedAt);
                b.Property(x => x.RequestedIp).HasMaxLength(64);
                b.Property(x => x.Attempts).HasDefaultValue(0);
                b.Property(x => x.CreatedAt);
                b.HasIndex(x => x.TokenHash).IsUnique();
                b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            TouchTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            TouchTimestamps();
            return base.SaveChanges();
        }

        private void TouchTimestamps()
        {
            var now = DateTime.UtcNow;
            foreach (var e in ChangeTracker.Entries())
            {
                if (e.Entity is User u)
                {
                    if (e.State == EntityState.Added)
                    {
                        if (u.CreatedAt == default) u.CreatedAt = now;
                        u.UpdatedAt = now;
                    }
                    else if (e.State == EntityState.Modified)
                    {
                        u.UpdatedAt = now;
                    }
                }
                else if (e.Entity is Role r)
                {
                    if (e.State == EntityState.Added)
                    {
                        if (r.CreatedAt == default) r.CreatedAt = now;
                        r.UpdatedAt = now;
                    }
                    else if (e.State == EntityState.Modified)
                    {
                        r.UpdatedAt = now;
                    }
                }
                else if (e.Entity is Permission p)
                {
                    if (e.State == EntityState.Added)
                    {
                        if (p.CreatedAt == default) p.CreatedAt = now;
                        p.UpdatedAt = now;
                    }
                    else if (e.State == EntityState.Modified)
                    {
                        p.UpdatedAt = now;
                    }
                }
            }
        }
    }
}
