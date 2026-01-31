using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft;
using AuthSystemAPI.Models;
using AuthSystemAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthSystemAPI.Data
{
    // Crucial: Inherit from IdentityDbContext, NOT just DbContext
    // This automatically adds tables for Users, Roles, Claims, and Tokens.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Custom DbSets for Permission-Based Authorization
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");

            // Configure Permission entity
            builder.Entity<Permission>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Description).HasMaxLength(500);
            });

            // Configure RolePermission entity (Many-to-Many junction table)
            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => rp.Id);

                // Foreign key to Role
                entity.HasOne(rp => rp.Role)
                    .WithMany()
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key to Permission
                entity.HasOne(rp => rp.Permission)
                    .WithMany()
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Ensure unique combination of RoleId and PermissionId
                entity.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
            });

        }
    }
    
}