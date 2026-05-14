using InnovatEPAM.Portal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InnovatEPAM.Portal.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Idea> Ideas => Set<Idea>();
    public DbSet<IdeaAttachment> IdeaAttachments => Set<IdeaAttachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        builder.Entity<Idea>(entity =>
        {
            entity.ToTable("Ideas");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Title).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Description).HasMaxLength(2000);
            entity.Property(i => i.Status).IsRequired().HasConversion<int>();

            entity.HasOne(i => i.Submitter)
                .WithMany(u => u.SubmittedIdeas)
                .HasForeignKey(i => i.SubmitterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.UpdatedByAdmin)
                .WithMany(u => u.UpdatedIdeas)
                .HasForeignKey(i => i.LastModifiedByAdminId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(i => i.Category)
                .HasMaxLength(50)
                .IsRequired(false);

            entity.Property(i => i.CategoryData)
                .IsRequired(false);

            entity.HasIndex(i => i.SubmitterId);
            entity.HasIndex(i => i.Status);
            entity.HasIndex(i => i.Category);
        });

        builder.Entity<IdeaAttachment>(entity =>
        {
            entity.ToTable("IdeaAttachments");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FileName).IsRequired().HasMaxLength(255);
            entity.Property(a => a.FilePath).IsRequired().HasMaxLength(500);

            entity.HasOne(a => a.Idea)
                .WithMany(i => i.IdeaAttachments)
                .HasForeignKey(a => a.IdeaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => a.IdeaId);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.OldStatus).IsRequired().HasMaxLength(50);
            entity.Property(a => a.NewStatus).IsRequired().HasMaxLength(50);

            entity.HasOne(a => a.Idea)
                .WithMany(i => i.AuditLogs)
                .HasForeignKey(a => a.IdeaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.ChangedByAdmin)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.ChangedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => a.IdeaId);
            entity.HasIndex(a => a.ChangedByAdminId);
            entity.HasIndex(a => a.ChangedDate);
        });

        SeedRoles(builder);
    }

    private static void SeedRoles(ModelBuilder builder)
    {
        var submitterRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var adminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = submitterRoleId,
                Name = "Submitter",
                NormalizedName = "SUBMITTER",
                ConcurrencyStamp = submitterRoleId.ToString()
            },
            new IdentityRole<Guid>
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = adminRoleId.ToString()
            }
        );
    }
}
